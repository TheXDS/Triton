using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.EFCore.Services.Base;
using TheXDS.Triton.Services;
using static TheXDS.Triton.Services.FailureReason;

namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// A class that describes a transaction that allows write operations
/// on a data context.
/// </summary>
/// <typeparam name="T">
/// Type of data context to use within the transaction.
/// </typeparam>
public class CrudWriteTransaction<T> : CrudTransactionBase<T>, ICrudWriteTransaction where T : DbContext
{
    /// <summary>
    /// Initializes a new instance of the class
    /// <see cref="CrudWriteTransaction{T}"/>.
    /// </summary>
    /// <param name="configuration">
    /// Configuration to use for the transaction.
    /// </param>
    /// <param name="options">
    /// Options to use when calling the data context constructor.
    /// Set this parameter to <see langword="null"/> to use the parameterless public constructor.
    /// </param>
    public CrudWriteTransaction(IMiddlewareRunner configuration, DbContextOptions? options = null) : base(configuration, options)
    {
    }

    internal CrudWriteTransaction(IMiddlewareRunner configuration, T context) : base(configuration, context)
    {
    }

    /// <summary>
    /// Executes cleanup tasks before disposing of this transaction.
    /// </summary>
    protected override void OnDispose()
    {
        if (_context.ChangeTracker.Entries().Any(p=>p.State != EntityState.Unchanged))
        {
            Commit();
        }
        base.OnDispose();
    }

    /// <summary>
    /// Saves all pending changes of the current transaction.
    /// </summary>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public ServiceResult Commit()
    {
        return TryCall(CrudAction.Commit, (Func<int>)_context.SaveChanges, null) ?? ServiceResult.Ok;
    }

    /// <summary>
    /// Saves all changes made asynchronously.
    /// </summary>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public Task<ServiceResult> CommitAsync()
    {
        return TryCallAsync<Model>(CrudAction.Commit, _context.SaveChangesAsync(), null) ?? Task.FromResult(ServiceResult.Ok);
    }

    /// <inheritdoc/>
    public ServiceResult Create<TModel>(params TModel[] entities) where TModel : Model
    {
        return TryCall(CrudAction.Write, _context.AddRange, entities);
    }

    /// <summary>
    /// Deletes an entity from the database.
    /// </summary>
    /// <typeparam name="TModel">
    /// Type of the entity model to delete.
    /// </typeparam>
    /// <param name="entities">
    /// Entity that should be deleted from the database.
    /// </param>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public ServiceResult Delete<TModel>(params TModel[] entities) where TModel : Model
    {
        return TryCall(CrudAction.Write, _context.RemoveRange, entities);
    }

    /// <summary>
    /// Deletes an entity from the database.
    /// </summary>
    /// <typeparam name="TModel">
    /// Type of the entity model to delete.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// Type of the key field that identifies the entity.
    /// </typeparam>
    /// <param name="key">
    /// Key of the entity that should be deleted from the database.
    /// </param>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public ServiceResult Delete<TModel, TKey>(TKey key)
        where TModel : Model<TKey>
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        var e = _context.Find<TModel>(key);
        if (e is null) return NotFound;
        return Delete(e);
    }

    /// <inheritdoc/>
    public ServiceResult Update<TModel>(params TModel[] entities) where TModel : Model
    {
        return TryCall(CrudAction.Write, _context.UpdateRange, entities);
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel, TKey>(params TKey[] keys)
        where TModel : Model<TKey>, new()
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        var entities = keys.Select(k => _context.Find<TModel>(k)).ToArray();
        if (entities.Any(p => p is null)) return NotFound;

        foreach (TModel entity in entities.NotNull())
        {
            if (Perform(CrudAction.Write, _context.Remove, entity) is { } error) return error;
        }
        return ServiceResult.Ok;
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel>(params string[] stringKeys) where TModel : Model, new()
    {
        var entities = stringKeys.Select(k => _context.Find<TModel>(k)).ToArray();
        if (entities.Any(p => p is null)) return NotFound;

        foreach (TModel entity in entities.NotNull())
        {
            if (Perform(CrudAction.Write, _context.Remove, entity) is { } error) return error;
        }
        return ServiceResult.Ok;
    }

    /// <inheritdoc/>
    public ServiceResult Discard()
    {
        return TryCall(CrudAction.Discard, _context.ChangeTracker.Clear, null) ?? ServiceResult.Ok;
    }

    /// <inheritdoc/>
    public ServiceResult Create(params Model[] entities)
    {
        return TryCall(CrudAction.Write, _context.AddRange, entities);
    }

    /// <inheritdoc/>
    public ServiceResult Update(params Model[] entities)
    {
        return TryCall(CrudAction.Write, _context.UpdateRange, entities);
    }

    /// <inheritdoc/>
    public ServiceResult Delete(params Model[] entities)
    {
        return TryCall(CrudAction.Write, _context.RemoveRange, entities);
    }
}