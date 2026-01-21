using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Component;

public interface IExecutionContextInfo<TResult> where TResult : notnull, ServiceResult, new()
{
    /// <summary>
    /// Represents the CRUD action to perform. Determines the context for
    /// prologue and epilogue processing.
    /// </summary>
    CrudAction Action { get; }

    /// <summary>
    /// Gets or sets the collection of change tracker items representing the prologue data.
    /// </summary>
    IEnumerable<ChangeTrackerItem>? PrologueData { get; }

    /// <summary>
    /// Gets or sets a delegate that generates a collection of change tracking items based on the specified service
    /// result.
    /// </summary>
    /// <remarks>The delegate can be null if no epilogue data is required. When set, the function is invoked
    /// with the result of the service operation to produce additional change tracking information. The returned
    /// collection may be <see langword="null"/> if there are no changes to track.</remarks>
    Func<ServiceResult?, IEnumerable<ChangeTrackerItem>?>? EpilogueData { get; }

    Task<TResult> InvokeOperationAsync();
}

public class SimpleAsyncExecutionContextInfo<TResult>(CrudAction action, Func<Task<TResult?>> operation) : IExecutionContextInfo<TResult> where TResult : notnull, ServiceResult, new()
{
    public CrudAction Action { get; set; } = action;

    public IEnumerable<ChangeTrackerItem>? PrologueData { get; set; } = null;

    public Func<ServiceResult?, IEnumerable<ChangeTrackerItem>?>? EpilogueData { get; set; } = null;

    public Func<Task<TResult?>> Operation { get; } = operation;

    public Task<TResult?> InvokeOperationAsync() => Operation.Invoke();

    public SimpleAsyncExecutionContextInfo(CrudAction action, Func<TResult?> operation) : this(action, () => Task.FromResult(operation.Invoke()))
    {
    }
}

public class ChangeTrackerPassthroughExecutionContextInfo<TResult>(
    CrudAction action,
    IEnumerable<ChangeTrackerItem> changeTrackerItems,
    Func<IEnumerable<ChangeTrackerItem>, Task<TResult?>> operation)
    : IExecutionContextInfo<TResult> where TResult : notnull, ServiceResult, new()
{
    private readonly IEnumerable<ChangeTrackerItem> _changeTrackerItems = changeTrackerItems;

    public ChangeTrackerPassthroughExecutionContextInfo(
        CrudAction action,
        IEnumerable<ChangeTrackerItem> changeTrackerItems,
        Func<IEnumerable<ChangeTrackerItem>, TResult?> operation)
        : this(action, changeTrackerItems, e => Task.FromResult(operation.Invoke(e)))
    { }

    public CrudAction Action { get; } = action;

    public IEnumerable<ChangeTrackerItem>? PrologueData => _changeTrackerItems;

    public Func<ServiceResult?, IEnumerable<ChangeTrackerItem>?>? EpilogueData => _ => _changeTrackerItems;

    public Func<Task<TResult?>> Operation => () => operation.Invoke(_changeTrackerItems);

    public Task<TResult?> InvokeOperationAsync() => Operation.Invoke();
}
