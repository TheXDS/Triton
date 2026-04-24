using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Component;

/// <summary>
/// Encapsulates the execution context for CRUD operations, handling prologue
/// and epilogue invocation automatically.
/// </summary>
/// <param name="runner">Middleware runner to use.</param>
public sealed class MiddlewareExecutionContext(IMiddlewareRunner runner)
{
    private readonly IMiddlewareRunner _runner = runner;

    /// <summary>
    /// Executes a CRUD operation asynchronously, applying operations to
    /// optionally-specified sets of data, and returns a service result
    /// indicating the outcome.
    /// </summary>
    /// <remarks>
    /// If the prologue or epilogue processing returns a service result, that
    /// result is returned immediately and the main operation or subsequent
    /// steps are not executed. If the operation throws an exception, a failed
    /// service result is returned. If the operation returns
    /// <see langword="null"/>, a default successful service result is
    /// returned.
    /// </remarks>
    /// <typeparam name="TServiceResult">
    /// The type of the service result returned by the operation. Must either
    /// be or inherit from <see cref="ServiceResult"/>, be non-nullable, and
    /// have a parameterless constructor.
    /// </typeparam>
    /// <param name="parameters">
    /// Parameters to use when invoking this execution context.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result
    /// contains a result of type <typeparamref name="TServiceResult"/>,
    /// indicating the outcome of the operation and any prologue or epilogue
    /// processing.
    /// </returns>
    public async Task<TServiceResult> ExecuteAsync<TServiceResult>(IExecutionContextInfo<TServiceResult> parameters) where TServiceResult : notnull, ServiceResult, new()
    {
        TServiceResult? result;
        if (_runner.RunPrologue(parameters.Action, parameters.PrologueData) is { } prologueResult) return prologueResult.CastUp<TServiceResult>();
        try
        {
            result = await parameters.InvokeOperationAsync();
        }
        catch (Exception ex)
        {
            return new ServiceResult(ex).CastUp<TServiceResult>();
        }
        if (result is { IsSuccessful: false }) return result;
        if (_runner.RunEpilogue(parameters.Action, parameters.EpilogueData?.Invoke(result)) is { } epilogueResult) return epilogueResult.CastUp<TServiceResult>();
        return result ?? ServiceResult.Ok.CastUp<TServiceResult>();
    }

    /// <summary>
    /// Executes a service operation synchronously using the specified
    /// execution context parameters.
    /// </summary>
    /// <typeparam name="TServiceResult">
    /// The type of the service result returned by the operation. Must either
    /// be or inherit from <see cref="ServiceResult"/> and have a parameterless
    /// constructor.
    /// </typeparam>
    /// <param name="parameters">
    /// The parameters that define the execution context for the service
    /// operation. Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// A result object of type <typeparamref name="TServiceResult"/>
    /// containing the outcome of the executed service operation.
    /// </returns>
    public TServiceResult Execute<TServiceResult>(IExecutionContextInfo<TServiceResult> parameters)
        where TServiceResult : notnull, ServiceResult, new()
    {
        return ExecuteAsync(parameters).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Returns the entity enumeration as a collection of
    /// <see cref="ChangeTrackerItem"/> objects with the specified change type.
    /// </summary>
    /// <param name="change">
    /// Type of change to indicate by the resulting collection of
    /// <see cref="ChangeTrackerItem"/> objects.
    /// </param>
    /// <param name="entities">
    /// Collection of entities affected by the operation.
    /// </param>
    /// <returns>
    /// A collection of <see cref="ChangeTrackerItem"/> objects with the
    /// specified change type.
    /// </returns>
    public static IEnumerable<ChangeTrackerItem> Map(ChangeTrackerChangeType change, IEnumerable<Model> entities)
    {
        return entities.Select(e => new ChangeTrackerItem(change, e));
    }
}
