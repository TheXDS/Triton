using System.Linq.Expressions;
using TheXDS.Triton.EFCore.Services.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// Gets a complex transaction that allows read and write operations on a common data context.
/// </summary>
/// <typeparam name="T">
/// Type of data context to use.
/// </typeparam>
public class CrudTransaction<T> : CrudTransactionBase<T>, ICrudReadWriteTransaction<T> where T : DbContext
{
    private readonly CrudReadTransaction<T> _readTransaction;
    private readonly CrudWriteTransaction<T> _writeTransaction;

    /// <summary>
    /// Gets the active context instance in this transaction.
    /// </summary>
    public T Context => _context;

    /// <summary>
    /// Initializes a new instance of the class
    /// <see cref="CrudTransaction{T}"/>.
    /// </summary>
    /// <param name="configuration">
    /// Configuration to pass to the underlying transactions.
    /// </param>
    /// <param name="options">
    /// Options to use when calling the data context constructor.
    /// Set this parameter to <see langword="null"/> to use the parameterless public constructor.
    /// </param>
    public CrudTransaction(IMiddlewareRunner configuration, DbContextOptions? options = null) : base(configuration, options)
    {
        _readTransaction = new CrudReadTransaction<T>(configuration, _context);
        _writeTransaction = new CrudWriteTransaction<T>(configuration, _context);
    }

    /// <summary>
    /// Gets an entity whose key field equals the specified value.
    /// </summary>
    /// <typeparam name="TModel">
    /// Type of the entity model to get.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// Type of the key field of the entity to get.
    /// </typeparam>
    /// <param name="key">
    /// Key of the entity to get.
    /// </param>
    /// <param name="entity">
    /// Output parameter. The entity obtained from the read operation.
    /// If no entity exists with the specified key field, <see langword="null"/> will be returned.
    /// </param>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public ServiceResult Read<TModel, TKey>(TKey key, out TModel? entity) where TModel : Model<TKey>, new() where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        return _readTransaction.Read(key, out entity);
    }

    /// <summary>
    /// Gets an entity whose key field equals the specified value.
    /// </summary>
    /// <typeparam name="TModel">
    /// Type of the entity model to get.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// Type of the key field of the entity to get.
    /// </typeparam>
    /// <param name="key">
    /// Key of the entity to get.
    /// </param>
    /// <returns>
    /// The reported result of the operation executed by the underlying service,
    /// including the entity obtained from the read operation as the result value.
    /// If no entity exists with the specified key field, the result value will be
    /// <see langword="null"/>.
    /// </returns>
    public ServiceResult<TModel?> Read<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        return _readTransaction.Read<TModel,TKey>(key);
    }

    /// <summary>
    /// Creates a new entity in the database.
    /// </summary>
    /// <typeparam name="TModel">
    /// Type of the new entity model.
    /// </typeparam>
    /// <param name="newEntity">
    /// New entity to add to the database.
    /// </param>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public ServiceResult Create<TModel>(params TModel[] newEntity) where TModel : Model
    {
        return _writeTransaction.Create(newEntity);
    }

    /// <summary>
    /// Deletes an entity from the database.
    /// </summary>
    /// <typeparam name="TModel">
    /// Type of the entity model to delete.
    /// </typeparam>
    /// <param name="entity">
    /// Entity that should be deleted from the database.
    /// </param>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public ServiceResult Delete<TModel>(params TModel[] entity) where TModel : Model
    {
        return _writeTransaction.Delete(entity);
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
    public ServiceResult Delete<TModel, TKey>(params TKey[] key)
        where TModel : Model<TKey>, new()
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        return _writeTransaction.Delete<TModel,TKey>(key);
    }

    /// <summary>
    /// Updates the data contained in an entity within the database.
    /// </summary>
    /// <typeparam name="TModel">
    /// Type of the entity model to update.
    /// </typeparam>
    /// <param name="entity">
    /// Entity containing the new information to write.
    /// </param>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public ServiceResult Update<TModel>(params TModel[] entity) where TModel : Model
    {
        return _writeTransaction.Update(entity);
    }

    /// <summary>
    /// Saves all pending synchronous changes of the current transaction.
    /// </summary>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public ServiceResult Commit()
    {
        return _writeTransaction.Commit();
    }

    /// <summary>
    /// Saves all changes made asynchronously.
    /// </summary>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public Task<ServiceResult> CommitAsync()
    {
        return _writeTransaction.CommitAsync();
    }

    /// <summary>
    /// Asynchronously gets an entity whose key field equals the specified value.
    /// </summary>
    /// <typeparam name="TModel">
    /// Type of the entity model to get.
    /// </typeparam>
    /// <typeparam name="TKey">
    /// Type of the key field of the entity to get.
    /// </typeparam>
    /// <param name="key">
    /// Key of the entity to get.
    /// </param>
    /// <returns>
    /// The reported result of the operation executed by the underlying service.
    /// </returns>
    public Task<ServiceResult<TModel?>> ReadAsync<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        return _readTransaction.ReadAsync<TModel, TKey>(key);
    }

    /// <summary>
    /// Gets the complete collection of entities of the specified model
    /// stored in the database.
    /// </summary>
    /// <typeparam name="TModel">
    /// Type of the entity models to get.
    /// </typeparam>
    /// <returns></returns>
    public QueryServiceResult<TModel> All<TModel>() where TModel : Model
    {
        return _readTransaction.All<TModel>();
    }

    /// <summary>
    /// Releases the resources used by this instance.
    /// </summary>
    protected override void OnDispose()
    {
        _writeTransaction.Dispose();
        base.OnDispose();
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel[]?>> SearchAsync<TModel>(Expression<Func<TModel, bool>> query) where TModel : Model
    {
        return _readTransaction.SearchAsync(query);
    }

    /// <inheritdoc/>
    protected override async ValueTask OnDisposeAsync()
    {
        await _writeTransaction.CommitAsync();
        await base.OnDisposeAsync();
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel>(params string[] stringKeys) where TModel : Model, new()
    {
        return _writeTransaction.Delete<TModel>(stringKeys);
    }

    /// <inheritdoc/>
    public ServiceResult Discard()
    {
        return _writeTransaction.Discard();
    }

    /// <inheritdoc/>
    public ServiceResult<TModel?> Read<TModel>(object key) where TModel : Model, new()
    {
        return _readTransaction.Read<TModel>(key);
    }

    /// <inheritdoc/>
    public ServiceResult<Model?> Read(Type model, object key)
    {
        return _readTransaction.Read(model, key);
    }

    /// <inheritdoc/>
    public QueryServiceResult<Model> All(Type model)
    {
        return _readTransaction.All(model);
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel?>> ReadAsync<TModel>(object key) where TModel : Model, new()
    {
        return _readTransaction.ReadAsync<TModel>(key);
    }

    /// <inheritdoc/>
    public Task<ServiceResult<Model?>> ReadAsync(Type model, object key)
    {
        return _readTransaction.ReadAsync(model, key);
    }

    /// <inheritdoc/>
    public ServiceResult Create(params Model[] entities)
    {
        return _writeTransaction.Create(entities);
    }

    /// <inheritdoc/>
    public ServiceResult Update(params Model[] entities)
    {
        return _writeTransaction.Update(entities);
    }

    /// <inheritdoc/>
    public ServiceResult Delete(params Model[] entities)
    {
        return _writeTransaction.Delete(entities);
    }
}