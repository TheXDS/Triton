using NUnit.Framework;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Middleware;
using TheXDS.Triton.Services;
using TheXDS.Triton.Tests.Models;

namespace TheXDS.Triton.Tests;

internal abstract class TransactionMiddlewareExecutionTests<T> where T : ITransactionFactory, new()
{
    [Test]
    public void Commit_operation_executes_Middleware()
    {
        new MiddlewareRunCheck()
        {
            ExpectedAction = CrudAction.Commit,
        }.ExecuteTest(t => Assert.That(t.Commit().IsSuccessful, Is.True));
    }

    [Test]
    public void Create_operation_executes_Middleware()
    {
        var g = Guid.NewGuid().ToString();
        new MiddlewareRunCheck()
        {
            ExpectedAction = CrudAction.Write,
            ExpectedPrologModelType = typeof(User),
            ExpectedEpilogModelType = typeof(User),
            ExtraEpilogueAssertions = m =>
            {
                Assert.That(m[0].OldEntity, Is.Null);
                Assert.That(m[0].NewEntity?.IdAsString, Is.EqualTo(g));
            }
        }.ExecuteTest(t => Assert.That(t.Create(new User(g, g)).IsSuccessful, Is.True));
    }

    [Test]
    public void Read_operation_executes_Middleware()
    {
        var g = Guid.NewGuid().ToString();
        User? u = null;
        new MiddlewareRunCheck()
        {
            ExpectedAction = CrudAction.Read,
            ExpectedPrologModelType = typeof(User),
            ExpectedEpilogModelType = typeof(User),
            ExtraEpilogueAssertions = m => Assert.That(m[0].NewEntity?.IdAsString, Is.EqualTo(g))
        }.ExecuteTest(
            t => _ = t.Create(u = new(g, "user")),
            t => _ = t.Read<User, string>(g));
    }

    [Test]
    public void Update_operation_executes_Middleware()
    {
        var g = Guid.NewGuid().ToString();
        User? u = null;
        new MiddlewareRunCheck()
        {
            ExpectedAction = CrudAction.Write,
            ExpectedPrologModelType = typeof(User),
            ExpectedEpilogModelType = typeof(User),
            ExtraPrologueAssertions = m => Assert.That(((User?)m[0].NewEntity)?.PublicName, Is.EqualTo(g)),
            ExtraEpilogueAssertions = m => Assert.That(((User?)m[0].NewEntity)?.IdAsString, Is.EqualTo(g))
        }.ExecuteTest(
            t => _ = t.Create(u = new(g, "user")),
            t =>
            {
                var newU = new User(g, g);
                t.Update(newU);
            });
    }

    [Test]
    public void Delete_operation_executes_Middleware()
    {
        var g = Guid.NewGuid().ToString();
        User? u = null;
        new MiddlewareRunCheck()
        {
            ExpectedAction = CrudAction.Write,
            ExpectedPrologModelType = typeof(User),
            ExtraPrologueAssertions = m => Assert.That(((User?)m[0].OldEntity)?.Id, Is.EqualTo(g)),
            ExpectedEpilogModelType = typeof(User),
            ExtraEpilogueAssertions = m => Assert.That(((User?)m[0].OldEntity)?.Id, Is.EqualTo(g))
        }.ExecuteTest(
            t => _ = t.Create(u = new(g, "user")),
            t => t.Delete<User, string>(g));
    }

    private class MiddlewareRunCheck : ITransactionMiddleware
    {
        public void ExecuteTest(Action<ICrudReadWriteTransaction> callback) => ExecuteTest(null, callback);

        public void ExecuteTest(Action<ICrudReadWriteTransaction>? setupCallback, Action<ICrudReadWriteTransaction> callback)
        {
            IMiddlewareConfigurator transactionConfig = new TransactionConfiguration();
            TritonService service = new(transactionConfig, new T());
            if (setupCallback is not null)
            {
                using var setupTransaction = service.GetTransaction();
                setupCallback?.Invoke(setupTransaction);
                setupTransaction.Commit();
            }
            transactionConfig.Attach(this);
            using var transaction = service.GetTransaction();
            Assert.That(PrologueRan, Is.False);
            Assert.That(EpilogueRan, Is.False);
            callback.Invoke(transaction);
            Assert.That(PrologueRan, Is.True);
            Assert.That(EpilogueRan, Is.True);
        }

        public CrudAction ExpectedAction { get; init; }

        public Type? ExpectedPrologModelType { get; init; }

        public Action<ChangeTrackerItem[]>? ExtraPrologueAssertions { get; init; }

        public Type? ExpectedEpilogModelType { get; init; }

        public Action<ChangeTrackerItem[]>? ExtraEpilogueAssertions { get; init; }

        public bool PrologueRan { get; private set; }

        public bool EpilogueRan { get; private set; }

        ServiceResult? ITransactionMiddleware.PrologueAction(in CrudAction action, IEnumerable<ChangeTrackerItem>? entities)
        {
            if (!PrologueRan)
            {
                Assert.That(EpilogueRan, Is.False);
                Assert.That(action, Is.EqualTo(ExpectedAction));
                if (ExpectedPrologModelType is null)
                {
                    Assert.That(entities, Is.Null.Or.Empty);
                }
                else
                {
                    Assert.That(entities, Is.Not.Null);
                    Assert.That(entities!.FirstOrDefault()?.Model, Is.EqualTo(ExpectedPrologModelType));
                    ExtraPrologueAssertions?.Invoke([.. entities.NotNull()]);
                }
                PrologueRan = true;
            }
            return null;
        }

        ServiceResult? ITransactionMiddleware.EpilogueAction(in CrudAction action, IEnumerable<ChangeTrackerItem>? entities)
        {
            if (!EpilogueRan)
            {
                Assert.That(PrologueRan, Is.True);
                Assert.That(action, Is.EqualTo(ExpectedAction));
                if (ExpectedEpilogModelType is null)
                {
                    Assert.That(entities, Is.Null.Or.Empty);
                }
                else
                {
                    Assert.That(entities, Is.Not.Null);
                    Assert.That(entities!.FirstOrDefault()?.Model, Is.EqualTo(ExpectedEpilogModelType));
                    ExtraEpilogueAssertions?.Invoke([.. entities.NotNull()]);
                }
                EpilogueRan = true;
            }
            return null;
        }
    }
}