using NUnit.Framework;
using System.Collections.Specialized;
using TheXDS.Triton.Diagnostics.Middleware;
using TheXDS.Triton.Middleware;
using TheXDS.Triton.Services;
using TheXDS.Triton.Tests.Models;

namespace TheXDS.Triton.Tests.Diagnostics;

internal class ChangeTrackerObserverMiddlewareTests
{
    [Test]
    public void Instance_test()
    {
        var j = new ChangeTrackerObserverMiddleware();
        Assert.That(j, Is.InstanceOf<ITransactionMiddleware>());
        Assert.That(j.Changes,
            Is.Not.Null
            .And.AssignableTo<IEnumerable<ChangeTrackerItem>>()
            .And.AssignableTo<INotifyCollectionChanged>());
    }

    [Test]
    public void Middleware_tracks_changes_during_transaction()
    {
        IMiddlewareConfigurator r = new TransactionConfiguration();
        ChangeTrackerObserverMiddleware m = new();
        r.Attach(m);
        Assert.That(m.Changes, Is.Empty);
        r.GetRunner().RunPrologue(CrudAction.Write, [new ChangeTrackerItem(ChangeTrackerChangeType.Create, new User())]);
        Assert.That(m.Changes, Is.Not.Empty);
        r.GetRunner().RunEpilogue(CrudAction.Commit, []);
        Assert.That(m.Changes, Is.Empty);
    }

    [Test]
    public void Middleware_clears_changes_on_Discard()
    {
        IMiddlewareConfigurator r = new TransactionConfiguration();
        ChangeTrackerObserverMiddleware m = new();
        r.Attach(m);
        r.GetRunner().RunPrologue(CrudAction.Write, [new ChangeTrackerItem(ChangeTrackerChangeType.Create, new User())]);
        r.GetRunner().RunEpilogue(CrudAction.Discard, []);
        Assert.That(m.Changes, Is.Empty);
    }

    [Test]
    public void Middleware_tracks_multiple_writes_during_transaction()
    {
        IMiddlewareConfigurator r = new TransactionConfiguration();
        ChangeTrackerObserverMiddleware m = new();
        r.Attach(m);
        Assert.That(m.Changes, Is.Empty);
        r.GetRunner().RunPrologue(CrudAction.Write, [new ChangeTrackerItem(ChangeTrackerChangeType.Create, new User())]);
        Assert.That(m.Changes, Has.Count.EqualTo(1));
        r.GetRunner().RunPrologue(CrudAction.Write, [new ChangeTrackerItem(ChangeTrackerChangeType.Update, new User())]);
        Assert.That(m.Changes, Has.Count.EqualTo(2));
        r.GetRunner().RunPrologue(CrudAction.Write, [new ChangeTrackerItem(ChangeTrackerChangeType.Delete, new User())]);
        Assert.That(m.Changes, Has.Count.EqualTo(3));
        r.GetRunner().RunEpilogue(CrudAction.Commit, []);
        Assert.That(m.Changes, Is.Empty);
    }

    [TestCase(CrudAction.Read)]
    [TestCase(CrudAction.Query)]
    [TestCase(CrudAction.Discard)]
    [TestCase(CrudAction.Commit)]

    public void Middleware_ignores_non_writes(CrudAction action)
    {
        IMiddlewareConfigurator r = new TransactionConfiguration();
        ChangeTrackerObserverMiddleware m = new();
        r.Attach(m);
        Assert.That(m.Changes, Is.Empty);
        r.GetRunner().RunPrologue(action, [new ChangeTrackerItem(ChangeTrackerChangeType.NoChange, new User())]);
        Assert.That(m.Changes, Is.Empty);
    }

    [Test]
    public void Middleware_ignores_bogus_empty_writes()
    {
        IMiddlewareConfigurator r = new TransactionConfiguration();
        ChangeTrackerObserverMiddleware m = new();
        r.Attach(m);
        Assert.That(m.Changes, Is.Empty);
        r.GetRunner().RunPrologue(CrudAction.Write, []);
        Assert.That(m.Changes, Is.Empty);
    }

    [Test]
    public void Middleware_ignores_bogus_null_writes()
    {
        IMiddlewareConfigurator r = new TransactionConfiguration();
        ChangeTrackerObserverMiddleware m = new();
        r.Attach(m);
        Assert.That(m.Changes, Is.Empty);
        r.GetRunner().RunPrologue(CrudAction.Write, null);
        Assert.That(m.Changes, Is.Empty);
    }

    [Test]
    public void Middleware_prologue_unconditionally_continues()
    {
        IMiddlewareConfigurator r = new TransactionConfiguration();
        ChangeTrackerObserverMiddleware m = new();
        r.Attach(m);
        Assert.That(r.GetRunner().RunPrologue(CrudAction.Write, []), Is.Null);
    }

    [Test]
    public void Middleware_epilogue_unconditionally_continues()
    {
        IMiddlewareConfigurator r = new TransactionConfiguration();
        ChangeTrackerObserverMiddleware m = new();
        r.Attach(m);
        Assert.That(r.GetRunner().RunEpilogue(CrudAction.Write, []), Is.Null);
    }
}
