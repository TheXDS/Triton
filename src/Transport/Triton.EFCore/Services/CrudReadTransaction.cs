using System.Linq.Expressions;
using TheXDS.MCART.Exceptions;
using TheXDS.Triton.EFCore.Services.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// A class that describes a transaction that allows read operations
/// on a data context.
/// </summary>
/// <typeparam name="T">
/// Type of data context to use within the transaction.
/// </typeparam>
public class CrudReadTransaction<T> : CrudTransactionBase<T>, ICrudReadTransaction where T : DbContext
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="CrudReadTransaction{T}"/> class.
    /// </summary>
    /// <param name="configuration">
    /// Configuration to use for the transaction.
    /// </param>
    /// <param name="options">
    /// Options to use when calling the data context constructor.
    /// Set this parameter to <see langword="null"/> to use the
    /// parameterless public constructor.
    /// </param>
    public CrudReadTransaction(IMiddlewareRunner configuration, DbContextOptions? options = null) : base(configuration, options)
    {
    }

    internal CrudReadTransaction(IMiddlewareRunner configuration, T context) : base(configuration, context)
    {
    }

    /// <inheritdoc/>
    public QueryServiceResult<TModel> All<TModel>() where TModel : Model
    {
        var result = TryCall(CrudAction.Read, (Func<DbSet<TModel>>)_context.Set<TModel>, out DbSet<TModel>? dbSet, null)?.CastUp<QueryServiceResult<TModel>>();
        return dbSet is null ? result ?? FailureReason.DbFailure : new QueryServiceResult<TModel>(dbSet);
    }

    /// <inheritdoc/>
    public QueryServiceResult<Model> All(Type model)
    {
        var setMethod = (typeof(T).GetMethod(nameof(DbContext.Set), 1, []) ?? throw new TamperException()).MakeGenericMethod(model);
        var dbSetType = typeof(DbSet<>).MakeGenericType(model);
        var funcDbSetType = typeof(Func<>).MakeGenericType(dbSetType);
        var result = TryCall(CrudAction.Read, setMethod, _context, out IQueryable<Model> dbSet, null);
        return (result?.IsSuccessful ?? true) ? new QueryServiceResult<Model>(dbSet) : FailureReason.DbFailure;
    }

    /// <inheritdoc/>
    public ServiceResult<TModel?> Read<TModel, TKey>(TKey key) where TModel : Model<TKey>, new() where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        var result = TryCall(CrudAction.Read, _context.Find<TModel>, out TModel? entity, [new object[] { key }])?.CastUp<ServiceResult<TModel?>>();
        return  entity is null ? result ?? FailureReason.NotFound : entity;
    }

    /// <inheritdoc/>
    public ServiceResult Read<TModel, TKey>(TKey key, out TModel? entity)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        var result = TryCall(CrudAction.Read, _context.Find<TModel>, out entity, [new object[] { key }])?.CastUp<ServiceResult<TModel?>>();
        return entity is null ? result ?? FailureReason.NotFound : ServiceResult.Ok;
    }

    /// <inheritdoc/>
    public ServiceResult<TModel?> Read<TModel>(object key) where TModel : Model, new()
    {
        var result = TryCall(CrudAction.Read, _context.Find<TModel>, out TModel? entity, [new object[] { key }])?.CastUp<ServiceResult<TModel?>>();
        return entity is null ? result ?? FailureReason.NotFound : entity;
    }

    /// <inheritdoc/>
    public ServiceResult<Model?> Read(Type model, object key)
    {
        var result = TryCall(CrudAction.Read, (Func<Type, object?[]?, object?>)_context.Find, out Model? entity, [model, new object[] { key }])?.CastUp<ServiceResult<Model?>>();
        return entity is null ? result ?? FailureReason.NotFound : entity;
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel?>> ReadAsync<TModel, TKey>(TKey key) where TModel : Model<TKey>, new() where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        return TryCallAsync(CrudAction.Read, _context.FindAsync<TModel>([key]));
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel?>> ReadAsync<TModel>(object key) where TModel : Model, new()
    {
        return TryCallAsync(CrudAction.Read, _context.FindAsync<TModel>(key), null);
    }

    /// <inheritdoc/>
    public Task<ServiceResult<Model?>> ReadAsync(Type model, object key)
    {
        return TryCallAsync(CrudAction.Read, _context.FindAsync(model, key).AsTask(), null);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<TModel[]?>> SearchAsync<TModel>(Expression<Func<TModel, bool>> query) where TModel : Model
    {
        try
        {
            var a = All<TModel>();
            if (!a.IsSuccessful) return a.Reason!.Value;
            return await a.Where(query).ToArrayAsync();
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}