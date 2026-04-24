using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using TheXDS.MCART.Helpers;
using TheXDS.MCART.Security;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Extensions;
using TheXDS.Triton.InMemory.Services;
using TheXDS.Triton.Models;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Tests.SecurityEssentials.Extensions;

internal class AuthenticatedServiceExtensionsTests
{
    [ExcludeFromCodeCoverage]
    private class TestAuthService : AuthenticatedService
    {
        public static TestAuthService Create()
        {
            LoginCredential admin = new()
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = PasswordStorage.CreateHash<Pbkdf2Storage>("password".ToSecureString()),
                Enabled = true
            };
            ICollection<Model> store = [admin];
            ITransactionFactory factory = new CollectionTransFactory(store);
            TestAuthService service = new(factory);
            service.AuthenticationBroker.Authenticate(new LoginCredential() { Username = "test", Granted = PermissionFlags.Elevate });
            return service;
        }

        private TestAuthService(ITransactionFactory factory) : base(new TestUserService(factory), factory)
        {
        }

        public bool HasOperationRan { get; set; }

        public Task RunMe()
        {
            HasOperationRan = true;
            return Task.CompletedTask;
        }
    }

    [Test]
    public async Task Sudo_executes_privileged_operation()
    {
        var service = TestAuthService.Create();
        Assert.That((await service.Sudo("admin", "password".ToSecureString(), x => x.RunMe())).IsSuccessful, Is.True);
        Assert.That(service.HasOperationRan, Is.True);
    }

    [Test]
    public async Task Sudo_with_wrong_password_fails()
    {
        var service = TestAuthService.Create();
        var result = await service.Sudo("admin", "incorrect".ToSecureString(), x => x.RunMe());
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.Forbidden));
        Assert.That(service.HasOperationRan, Is.False);
    }

    [Test]
    public async Task Sudo_with_unknown_user_fails()
    {
        var service = TestAuthService.Create();
        var result = await service.Sudo("unknown", "doesntmatter".ToSecureString(), x => x.RunMe());
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.Forbidden));
        Assert.That(service.HasOperationRan, Is.False);
    }

    [Test]
    public async Task Sudo_without_authentication_fails()
    {
        var service = TestAuthService.Create();
        service.AuthenticationBroker.Authenticate(null);

        var result = await service.Sudo("unknown", "doesntmatter".ToSecureString(), x => x.RunMe());
        Assert.That(result.IsSuccessful, Is.False);
        Assert.That(result.Reason, Is.EqualTo(FailureReason.Forbidden));
        Assert.That(service.HasOperationRan, Is.False);
    }
}