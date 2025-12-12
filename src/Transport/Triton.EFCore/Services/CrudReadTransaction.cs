using System.Linq.Expressions;
using TheXDS.MCART.Exceptions;
using TheXDS.Triton.EFCore.Services.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// Clase que describe una transacción que permite realizar operaciones
/// de lectura sobre un contexto de datos.
/// </summary>
/// <typeparam name="T">
/// Tipo de contexto de datos a utilizar dentro de la transacción.
/// </typeparam>
public class CrudReadTransaction<T> : CrudTransactionBase<T>, ICrudReadTransaction where T : DbContext
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase
    /// <see cref="CrudReadTransaction{T}"/>.
    /// </summary>
    /// <param name="configuration">
    /// Configuración a utilizar para la transacción.
    /// </param>
    /// <param name="options">
    /// Opciones a utilizar para llamar al contructor del contexto de datos.
    /// Establezca este parámetro en <see langword="null"/> para utilizar el
    /// constructor público sin parámetros.
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
        var result = TryCall(CrudAction.Read, setMethod.CreateDelegate(funcDbSetType), out object? dbSet, null)?.CastUp<QueryServiceResult<Model>>();
        return (result?.IsSuccessful ?? false) ? result : FailureReason.DbFailure;
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