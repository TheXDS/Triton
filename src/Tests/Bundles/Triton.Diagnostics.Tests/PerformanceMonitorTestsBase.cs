using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.ComponentModel;
using TheXDS.Triton.Diagnostics.Middleware;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Tests.Diagnostics;

internal abstract class PerformanceMonitorTestsBase<T> : MiddlewareTestsBase<T> where T : PerformanceMonitorBase, new()
{
    protected const int EventsEpsilon = 200;

    protected virtual IEnumerable<string> ExtraTelemetryNpcProps() => [];

    protected static void RunCrudAction(IMiddlewareRunner runner, int sleep)
    {
        runner.RunPrologue(CrudAction.Commit, null);
        Thread.Sleep(sleep);
        runner.RunEpilogue(CrudAction.Commit, null);
    }

    protected static RangeConstraint IsAround(int meanMs, int epsilon = EventsEpsilon)
    {
        return Is.InRange(meanMs - epsilon, meanMs + epsilon);
    }

    [Test]
    public void Monitor_registers_event()
    {
        (var runner, var perfMon) = Build();
        PerformanceMonitorTestsBase<T>.RunCrudAction(runner, 1000);
        Assert.That(perfMon.EventCount, Is.EqualTo(1));
        Assert.That(perfMon.AverageMs, PerformanceMonitorTestsBase<T>.IsAround(1000));
    }

    [Test]
    public void Monitor_registers_multiple_events()
    {
        (var runner, var perfMon) = Build();
        PerformanceMonitorTestsBase<T>.RunCrudAction(runner, 500);
        Assert.That(perfMon.EventCount, Is.EqualTo(1));
        Assert.That(perfMon.AverageMs, PerformanceMonitorTestsBase<T>.IsAround(500));
        PerformanceMonitorTestsBase<T>.RunCrudAction(runner, 1500);
        Assert.That(perfMon.EventCount, Is.EqualTo(2));
        Assert.That(perfMon.AverageMs, PerformanceMonitorTestsBase<T>.IsAround(1000));

    }
    [Test]
    public void Monitor_includes_commits()
    {
        (var runner, var perfMon) = Build();
        RunCrudAction(runner, CrudAction.Commit);
        Assert.That(perfMon.EventCount, Is.EqualTo(1));
    }

    [Test]
    public void Monitor_includes_reads()
    {
        (var runner, var perfMon) = Build();
        RunCrudAction(runner, CrudAction.Read);
        Assert.That(perfMon.EventCount, Is.EqualTo(1));
    }

    [Test]
    public void Monitor_skips_create()
    {
        (var runner, var perfMon) = Build();
        RunCrudAction(runner, CrudAction.Write);
        Assert.That(perfMon.EventCount, Is.Zero);
    }

    [Test]
    public void Monitor_skips_update()
    {
        (var runner, var perfMon) = Build();
        RunCrudAction(runner, CrudAction.Write);
        Assert.That(perfMon.EventCount, Is.Zero);
    }

    [Test]
    public void Monitor_skips_delete()
    {
        (var runner, var perfMon) = Build();
        RunCrudAction(runner, CrudAction.Write);
        Assert.That(perfMon.EventCount, Is.Zero);
    }

    [Test]
    public void Monitor_data_is_initially_NaN()
    {
        var perfMon = new T();
        Assert.That(perfMon.AverageMs, Is.NaN);
        Assert.That(perfMon.MinMs, Is.NaN);
        Assert.That(perfMon.MaxMs, Is.NaN);
    }

    [Test]
    public void Monitor_initially_does_not_include_any_event()
    {
        var perfMon = new T();
        Assert.That(perfMon.EventCount, Is.Zero);
    }

    [Test]
    public void Monitor_has_full_telemetry()
    {
        (var runner, var perfMon) = Build();
        PerformanceMonitorTestsBase<T>.RunCrudAction(runner, 2000);
        PerformanceMonitorTestsBase<T>.RunCrudAction(runner, 1000);
        Assert.That(perfMon.MinMs, PerformanceMonitorTestsBase<T>.IsAround(1000));
        Assert.That(perfMon.MaxMs, PerformanceMonitorTestsBase<T>.IsAround(2000));
    }

    [Test]
    public void Monitor_supports_reset()
    {
        (var runner, var perfMon) = Build();
        PerformanceMonitorTestsBase<T>.RunCrudAction(runner, 1000);
        perfMon.Reset();
        Assert.That(perfMon.EventCount, Is.Zero);
        Assert.That(perfMon.AverageMs, Is.NaN);
        Assert.That(perfMon.MinMs, Is.NaN);
        Assert.That(perfMon.MaxMs, Is.NaN);
    }

    [Test]
    public void Monitor_fires_npc_events_for_telemetry()
    {
        (var runner, var perfMon) = Build();
        List<string> pendingProps = new(
        [
            nameof(perfMon.EventCount),
            nameof(perfMon.AverageMs),
            nameof(perfMon.MinMs),
            nameof(perfMon.MaxMs),
            ..ExtraTelemetryNpcProps()
        ]);
        void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Assert.That(sender, Is.SameAs(perfMon));
            Assert.That(pendingProps.Remove(e.PropertyName!));
        }
        perfMon.PropertyChanged += OnPropertyChanged;
        RunCrudAction(runner);
        perfMon.PropertyChanged -= OnPropertyChanged;
        Assert.That(pendingProps, Is.Empty);
    }
}