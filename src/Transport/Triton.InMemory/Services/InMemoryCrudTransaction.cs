using System.Linq.Expressions;
using TheXDS.MCART.Exceptions;
using TheXDS.MCART.Helpers;
using TheXDS.MCART.Types.Base;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Component;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.InMemory.Services;

/// <summary>
/// Represents a transaction for a dataset that lives in memory, whose contents
/// will only persist through the lifespan of the application.
/// </summary>
/// <param name="runner">Middleware _runner to use.</param>
/// <param name="store">
/// Temporary collection where the data is stored.
/// </param>
public class InMemoryCrudTransaction(IMiddlewareRunner runner, ICollection<Model> store) : AsyncDisposable, ICrudReadWriteTransaction
{
    private static readonly object _syncLock = new();
    private readonly IMiddlewareRunner _runner = runner;
    private readonly ICollection<Model> _store = store;
    private readonly ICollection<ChangeTrackerItem> _changeJournal = [];

    private readonly MiddlewareExecutionContext _executionContext = new(runner);

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryCrudTransaction"/>
    /// class.
    /// </summary>
    /// <param name="store">
    /// Temporary collection where the data is stored.
    /// </param>
    public InMemoryCrudTransaction(ICollection<Model> store) : this(((IMiddlewareConfigurator)new TransactionConfiguration()).GetRunner(), store)
    {
    }

    /// <inheritdoc/>
    public QueryServiceResult<TModel> All<TModel>() where TModel : Model
    {
        return _executionContext.Execute(new SimpleAsyncExecutionContextInfo<QueryServiceResult<TModel>>(CrudAction.Query, OnAll<TModel>));
    }

    /// <inheritdoc/>
    public QueryServiceResult<Model> All(Type model)
    {
        return _executionContext.Execute(new SimpleAsyncExecutionContextInfo<QueryServiceResult<Model>>(CrudAction.Query, () => OnAll(model)));
    }

    /// <inheritdoc/>
    public ServiceResult Commit()
    {
        try
        {
            return _executionContext.Execute(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Commit, _changeJournal, OnCommit));
        }
        finally
        {
            _changeJournal.Clear();
        }
    }

    /// <inheritdoc/>
    public Task<ServiceResult> CommitAsync()
    {
        try
        {
            return _executionContext.ExecuteAsync(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Commit, _changeJournal, OnCommit));
        }
        finally
        {
            _changeJournal.Clear();
        }
    }

    /// <inheritdoc/>
    public ServiceResult Create(params Model[] entities)
    {
        Task<ServiceResult?> OnCreate(IEnumerable<ChangeTrackerItem> entities)
        {
            foreach (var entity in entities.Select(p => p.NewEntity).NotNull())
            {
                if (FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return Task.FromResult<ServiceResult?>(FailureReason.EntityDuplication);
                }
            }
            _changeJournal.AddRange(entities);
            return Task.FromResult<ServiceResult?>(ServiceResult.Ok);
        }
        var changes = entities.Select(p => new ChangeTrackerItem(ChangeTrackerChangeType.Create, p));

        return _executionContext.Execute(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Write, changes, OnCreate));
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel>(params TModel[] entities) where TModel : Model
    {
        Task<ServiceResult?> OnDelete(IEnumerable<ChangeTrackerItem> entities)
        {
            foreach (var entity in entities.Select(p => p.OldEntity).NotNull())
            {
                if (!FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return Task.FromResult<ServiceResult?>(FailureReason.NotFound);
                }
            }
            _changeJournal.AddRange(entities);
            return Task.FromResult<ServiceResult?>(ServiceResult.Ok);
        }
        var changes = entities.Select(p => new ChangeTrackerItem(ChangeTrackerChangeType.Delete, p));
        return _executionContext.Execute(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Write, changes, OnDelete));
    }

    /// <inheritdoc/>
    public ServiceResult Delete(params Model[] entities)
    {
        Task<ServiceResult?> OnDelete(IEnumerable<ChangeTrackerItem> entities)
        {
            foreach (var entity in entities.Select(p => p.OldEntity).NotNull())
            {
                if (!FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return Task.FromResult<ServiceResult?>(FailureReason.NotFound);
                }
            }
            _changeJournal.AddRange(entities);
            return Task.FromResult<ServiceResult?>(ServiceResult.Ok);
        }
        var changes = entities.Select(p => new ChangeTrackerItem(ChangeTrackerChangeType.Delete, p));
        return _executionContext.Execute(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Write, changes, OnDelete));
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel, TKey>(params TKey[] keys)
        where TModel : Model<TKey>, new()
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        Task<ServiceResult?> OnDelete(IEnumerable<ChangeTrackerItem> entities)
        {
            foreach (var entity in entities.Where(p => p.ChangeType == ChangeTrackerChangeType.Delete).Select(p => p.OldEntity))
            {
                if (entity is null || !FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return Task.FromResult<ServiceResult?>(FailureReason.NotFound);
                }
            }
            _changeJournal.AddRange(entities);
            return Task.FromResult<ServiceResult?>(ServiceResult.Ok);
        }
        var changes = keys.Select(p => new ChangeTrackerItem(ChangeTrackerChangeType.Delete, new TModel() { Id = p }));
        return _executionContext.Execute(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Write, changes, OnDelete));
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel>(params string[] stringKeys) where TModel : Model, new()
    {
        Task<ServiceResult?> OnDelete(IEnumerable<ChangeTrackerItem> entities)
        {
            foreach (var entity in entities.Where(p => p.ChangeType == ChangeTrackerChangeType.Delete).Select(p => p.OldEntity))
            {
                if (entity is null || entity.IdAsString.IsEmpty() || !FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return Task.FromResult<ServiceResult?>(FailureReason.NotFound);
                }
            }
            _changeJournal.AddRange(entities);
            return Task.FromResult<ServiceResult?>(ServiceResult.Ok);
        }
        var changes = stringKeys.Select(p => new ChangeTrackerItem(ChangeTrackerChangeType.Delete, ReadInternal<TModel>(p) ?? new TModel() { }));
        return _executionContext.Execute(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Write, changes, OnDelete));
    }

    /// <inheritdoc/>
    public ServiceResult Discard()
    {
        ServiceResult OnDiscard(IEnumerable<ChangeTrackerItem> _)
        {
            _changeJournal.Clear();
            return ServiceResult.Ok;
        }
        try
        {
            return _executionContext.Execute(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Discard, _changeJournal, OnDiscard));
        }
        finally
        {
            _changeJournal.Clear();
        }
    }

    /// <inheritdoc/>
    public ServiceResult<TModel?> Read<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        return _executionContext.Execute(new SimpleAsyncExecutionContextInfo<ServiceResult<TModel?>>(CrudAction.Read, () => ReadInternal<TModel, TKey>(key))
        {
            PrologueData = [new ChangeTrackerItem(ChangeTrackerChangeType.NoChange, new TModel() { Id = key })],
            EpilogueData = (result) => result is ServiceResult<TModel?>{ Result: { } r } ? [new ChangeTrackerItem(ChangeTrackerChangeType.NoChange, r)] : null
        });
    }

    /// <inheritdoc/>
    public ServiceResult Read<TModel, TKey>(TKey key, out TModel? entity)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        var result = Read<TModel, TKey>(key);
        entity = result.Result;
        return result;
    }

    /// <inheritdoc/>
    public ServiceResult<TModel?> Read<TModel>(object key) where TModel : Model, new()
    {
        return _executionContext.Execute(new SimpleAsyncExecutionContextInfo<ServiceResult<TModel?>>(CrudAction.Read, () => ReadInternal<TModel>(key))
        {
            EpilogueData = (result) => result is ServiceResult<TModel?> { Result: { } r } ? [new ChangeTrackerItem(ChangeTrackerChangeType.NoChange, r)] : null
        });
    }

    /// <inheritdoc/>
    public ServiceResult<Model?> Read(Type model, object key)
    {
        return _executionContext.Execute(new SimpleAsyncExecutionContextInfo<ServiceResult<Model?>>(CrudAction.Read, () => ReadInternal<Model>(key))
        {
            EpilogueData = (result) => result is ServiceResult<Model?> { Result: { } r } ? [new ChangeTrackerItem(ChangeTrackerChangeType.NoChange, r)] : null
        });
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel?>> ReadAsync<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        return _executionContext.ExecuteAsync(new SimpleAsyncExecutionContextInfo<ServiceResult<TModel?>>(CrudAction.Read, () => ReadInternal<TModel, TKey>(key))
        {
            PrologueData = [new ChangeTrackerItem(ChangeTrackerChangeType.NoChange, new TModel() { Id = key })],
            EpilogueData = (result) => result is ServiceResult<TModel?> { Result: { } r } ? [new ChangeTrackerItem(ChangeTrackerChangeType.NoChange, r)] : null
        });
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel?>> ReadAsync<TModel>(object key) where TModel : Model, new()
    {
        return _executionContext.ExecuteAsync(new SimpleAsyncExecutionContextInfo<ServiceResult<TModel?>>(CrudAction.Read, () => ReadInternal<TModel>(key))
        {
            EpilogueData = (result) => result is ServiceResult<TModel?> { Result: { } r } ? [new ChangeTrackerItem(ChangeTrackerChangeType.NoChange, r)] : null
        });
    }

    /// <inheritdoc/>
    public Task<ServiceResult<Model?>> ReadAsync(Type model, object key)
    {
        return _executionContext.ExecuteAsync(new SimpleAsyncExecutionContextInfo<ServiceResult<Model?>>(CrudAction.Read, () => ReadInternal<Model>(key))
        {
            EpilogueData = (result) => result is ServiceResult<Model?> { Result: { } r } ? [new ChangeTrackerItem(ChangeTrackerChangeType.NoChange, r)] : null
        });
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel[]?>> SearchAsync<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : Model
    {
        return _executionContext.ExecuteAsync(new SimpleAsyncExecutionContextInfo<ServiceResult<TModel[]?>>(CrudAction.Query,
            () => Task.FromResult<ServiceResult<TModel[]?>?>(new ServiceResult<TModel[]?>([.. FullSet().OfType<TModel>().Where(predicate.Compile())]))));
    }

    /// <inheritdoc/>
    public ServiceResult Update<TModel>(params TModel[] entities) where TModel : Model
    {
        Task<ServiceResult?> OnUpdate(IEnumerable<ChangeTrackerItem> entities)
        {
            foreach (var entity in entities.Select(p => p.NewEntity).NotNull())
            {
                if (!FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return Task.FromResult<ServiceResult?>(FailureReason.NotFound);
                }
            }
            _changeJournal.AddRange(entities);
            return Task.FromResult<ServiceResult?>(ServiceResult.Ok);
        }
        var changes = entities.Select(p => new ChangeTrackerItem(ChangeTrackerChangeType.Update, p));
        return _executionContext.Execute(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Write, changes, OnUpdate));
    }

    /// <inheritdoc/>
    public ServiceResult Update(params Model[] entities)
    {
        Task<ServiceResult?> OnUpdate(IEnumerable<ChangeTrackerItem> entities)
        {
            foreach (var entity in entities.Select(p => p.NewEntity).NotNull())
            {
                if (!FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return Task.FromResult<ServiceResult?>(FailureReason.NotFound);
                }
            }
            _changeJournal.AddRange(entities);
            return Task.FromResult<ServiceResult?>(ServiceResult.Ok);
        }
        var changes = entities.Select(p => new ChangeTrackerItem(ChangeTrackerChangeType.Update, p));
        return _executionContext.Execute(new ChangeTrackerPassthroughExecutionContextInfo<ServiceResult>(CrudAction.Write, changes, OnUpdate));
    }

    /// <summary>
    /// Frees any resources being used by this instance.
    /// </summary>
    protected override void OnDispose()
    {
        if (_changeJournal.Count != 0) Commit();
    }

    /// <summary>
    /// Asynchronously frees any resources being used by this instance.
    /// </summary>
    protected override async ValueTask OnDisposeAsync()
    {
        if (_changeJournal.Count != 0) await CommitAsync().ConfigureAwait(false);
    }

    private TModel? ReadInternal<TModel, TKey>(TKey key) where TModel : Model<TKey> where TKey : notnull, IComparable<TKey>, IEquatable<TKey> => FullSet().OfType<TModel>().FirstOrDefault(p => p.Id.Equals(key));

    private TModel? ReadInternal<TModel>(object key) where TModel : Model => FullSet().OfType<TModel>().FirstOrDefault(p => p.IdAsString == key.ToString());

    private IEnumerable<Model> FullSet()
    {
        var oldEntities = _changeJournal.Where(p => p.OldEntity is not null).Select(FindOnStore).ToArray();
        var newEntities = _changeJournal.Select(p => p.NewEntity).NotNull().ToArray();
        return _store.Except(oldEntities).Concat(newEntities).NotNull();
    }

    private ServiceResult? StoreDelete(IEnumerable<ChangeTrackerItem> j)
    {
        foreach (var l in j.ToArray())
        {
            if ((FindOnStore(l) ?? l.OldEntity) is not { } oldEntity) return FailureReason.NotFound;
            _ = _store.Remove(oldEntity) || _changeJournal.Remove(l);
        }
        return null;
    }

    private ServiceResult? StoreUpdates(IEnumerable<ChangeTrackerItem> j)
    {
        foreach (var l in j)
        {
            if (FindOnStore(l) is not { } oldEntity) return FailureReason.NotFound;
            (l.NewEntity ?? throw new TamperException()).ShallowCopyTo(oldEntity, l.Model);
        }
        return null;
    }

    private ServiceResult? StoreNewEntities(IEnumerable<ChangeTrackerItem> j)
    {
        foreach (var k in j.GroupBy(p => p.Model))
        {
            var storeKeys = _store.OfType(k.Key).Select(q => q.IdAsString).ToArray();
            if (j.Any(p => storeKeys.Contains(p.NewEntity!.IdAsString)))
            {
                return FailureReason.EntityDuplication;
            }
        }
        _store.AddRange(j.Select(p => p.NewEntity).NotNull());
        return null;
    }

    private Model? FindOnStore(ChangeTrackerItem item)
    {
        return _store.OfType(item.Model).FirstOrDefault(p => p.IdAsString == (item.OldEntity?.IdAsString ?? throw new InvalidOperationException()).ToString());
    }

    private QueryServiceResult<TModel> OnAll<TModel>() where TModel : Model
    {
        return new(FullSet().OfType<TModel>().AsQueryable());
    }

    private QueryServiceResult<Model> OnAll(Type model)
    {
        return new(FullSet().OfType(model).AsQueryable());
    }

    private ServiceResult? OnCommit(IEnumerable<ChangeTrackerItem> _)
    {
        var callbacks = new Dictionary<ChangeTrackerChangeType, Func<IEnumerable<ChangeTrackerItem>, ServiceResult?>>
        {
            { ChangeTrackerChangeType.Create, StoreNewEntities },
            { ChangeTrackerChangeType.Update, StoreUpdates },
            { ChangeTrackerChangeType.Delete, StoreDelete }
        };
        foreach (var j in _changeJournal)
        {
            if (callbacks.TryGetValue(j.ChangeType, out var callback) && callback.Invoke(_changeJournal.Where(p => p.ChangeType == j.ChangeType)) is { } fail) return fail;
        }
        return ServiceResult.Ok;
    }
}
