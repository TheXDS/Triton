using TheXDS.MCART.Math;

namespace TheXDS.Triton.Diagnostics.Middleware;

/// <summary>
/// Middleware that allows obtaining specific information about the time it
/// takes to execute CRUD actions.
/// </summary>
public class PerformanceMonitor : PerformanceMonitorBase
{
    private int _eventCount;
    private double _averageMs;
    private double _minMs;
    private double _maxMs;

    /// <inheritdoc/>
    public override int EventCount => _eventCount;

    /// <inheritdoc/>
    public override double AverageMs => _averageMs;

    /// <inheritdoc/>
    public override double MinMs => _minMs;

    /// <inheritdoc/>
    public override double MaxMs => _maxMs;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMonitor"/>
    /// class.
    /// </summary>
    public PerformanceMonitor()
    {
        Reset();
    }

    /// <inheritdoc/>
    protected override void OnReset()
    {
        _averageMs = double.NaN;
        _eventCount = 0;
        _minMs = double.NaN;
        _maxMs = double.NaN;
    }

    /// <inheritdoc/>
    protected override void RegisterEvent(double milliseconds)
    {
        if (milliseconds > _maxMs || !_maxMs.IsValid()) _maxMs = milliseconds;
        if (milliseconds < _minMs || !_minMs.IsValid()) _minMs = milliseconds;
        if (_averageMs.IsValid())
        {
            _averageMs += (milliseconds - _averageMs)/(++_eventCount);
        }
        else
        {
            _averageMs = milliseconds;
            _eventCount = 1;
        }
    }
}