using NUnit.Framework;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Component;
using TheXDS.Triton.Middleware;
using TheXDS.Triton.Models;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Tests.SecurityEssentials;

internal class DataLayerMiddlewareTests
{
    private class TestSecurityActorProvider : ISecurityActorProvider
    {
        public SecurityObject? SecurityObject { get; set; }
        public SecurityObject? GetCurrentActor()
        {
            return SecurityObject;
        }
    }

    [Test]
    public void GetModelContextString_generic_Test()
    {
        var a = typeof(CrudAction).FullName;
        var b = CrudAction.Write.ToString();
        var c = typeof(LoginCredential).CSharpName();

        Assert.That(DataLayerSecurityMiddleware.GetModelContextString<LoginCredential>(CrudAction.Write), Is.EqualTo($"{a}.{b};{c}"));
    }

    [Test]
    public void GetModelContextString_Test()
    {
        var a = typeof(CrudAction).FullName;
        var b = CrudAction.Write.ToString();
        var c = typeof(LoginCredential).CSharpName();

        Assert.That(DataLayerSecurityMiddleware.GetModelContextString(CrudAction.Write, typeof(LoginCredential)), Is.EqualTo($"{a}.{b};{c}"));
    }

    [Test]
    public async Task Middleware_Test()
    {
        static async Task<LoginCredential> GetCredential(string credential, IUserService userService)
        {
            var cred = (await userService.GetCredential(credential)).Result;
            Assert.That(cred, Is.Not.Null);
            return cred!;
        }

        TestSecurityActorProvider prov = new();
        TestUserService svc = new();
        ITransactionMiddleware middleware = new DataLayerSecurityMiddleware(prov, svc);
        var root = await GetCredential("root", svc);
        var disabled = await GetCredential("disabled", svc);

        prov.SecurityObject = null;
        Assert.That(middleware.PrologueAction(CrudAction.Write, [new(ChangeTrackerChangeType.Create, new LoginCredential())])!.Reason, Is.EqualTo(FailureReason.Tamper));

        prov.SecurityObject =  root;
        Assert.That(middleware.PrologueAction(CrudAction.Commit, null), Is.Null);
        Assert.That(middleware.PrologueAction(CrudAction.Write, [new(ChangeTrackerChangeType.Create, new LoginCredential())]), Is.Null);

        prov.SecurityObject = disabled;
        Assert.That(middleware.PrologueAction(CrudAction.Commit, null), Is.Null);
        Assert.That(middleware.PrologueAction(CrudAction.Write, [new(ChangeTrackerChangeType.Create, new LoginCredential())])!.Reason, Is.EqualTo(FailureReason.Forbidden));
    }
}
