using System.Linq.Expressions;
using System.Reflection;
using TheXDS.MCART.Exceptions;
using TheXDS.MCART.Helpers;
using TheXDS.MCART.Types.Base;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Middleware;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;
using System.Linq;
using System;

namespace TheXDS.Triton.InMemory.Services;

/// <summary>
/// Represents a transaction for a dataset that lives in memory, whose contents
/// will only persist through the lifespan of the application.
/// </summary>
/// <param name="runner">Middleware runner to use.</param>
/// <param name="store">
/// Temporary collection where the data is stored.
/// </param>
public class InMemoryCrudTransaction(IMiddlewareRunner runner, ICollection<Model> store) : AsyncDisposable, ICrudReadWriteTransaction
{
    private static readonly object _syncLock = new();
    private readonly IMiddlewareRunner runner = runner;
    private readonly ICollection<Model> _store = store;
    private readonly ICollection<ChangeTrackerItem> _temp = [];

    private static ChangeTrackerChangeType Map(CrudAction action)
    {
        return action switch
        {
            CrudAction.Create => ChangeTrackerChangeType.Create,
            CrudAction.Update => ChangeTrackerChangeType.Update,
            CrudAction.Delete => ChangeTrackerChangeType.Delete,
            _ => ChangeTrackerChangeType.NoChange
        };
    }

    private TResult Execute<TResult>(CrudAction action, Func<TResult> operation, IEnumerable<Model>? middlewareData = null)
        where TResult : ServiceResult, new()
    {
        return Execute(action, () => (operation.Invoke(), middlewareData), (middlewareData ?? []).Select(p => new ChangeTrackerItem(Map(action), p)));
    }

    private TResult Execute2<TResult, TModel>(CrudAction action, Func<(TResult, IEnumerable<TModel>?)> operation, IEnumerable<TModel>? middlewareData = null)
        where TResult : ServiceResult, new()
        where TModel : Model
    {
        return Execute(action, operation.Invoke, (middlewareData ?? []).Select(p => new ChangeTrackerItem(Map(action), p)));
    }

    private TResult Execute<TResult>(CrudAction action, Func<(TResult, IEnumerable<Model>?)> operation, IEnumerable<ChangeTrackerItem>? prologueData = null)
        where TResult : ServiceResult, new()
    {
        lock (_syncLock)
        {
            if (runner.RunPrologue(action, prologueData) is { } failure) return failure.CastUp<TResult>();
            var (result, epilogData) = operation.Invoke();
            return (result.Success ? runner.RunEpilogue(action, (epilogData ?? []).Select(p => new ChangeTrackerItem(Map(action), p)))?.CastUp<TResult>() : null) ?? result;
        }
    }

    private TResult Execute<TResult, TModel>(CrudAction action, Func<(TResult, IEnumerable<TModel>?)> operation, IEnumerable<ChangeTrackerItem>? prologueData = null)
        where TResult : ServiceResult, new()
        where TModel : Model
    {
        lock (_syncLock)
        {
            if (runner.RunPrologue(action, prologueData) is { } failure) return failure.CastUp<TResult>();
            var (result, epilogData) = operation.Invoke();
            return (result.Success ? runner.RunEpilogue(action, (epilogData ?? []).Select(p => new ChangeTrackerItem(Map(action), p)))?.CastUp<TResult>() : null) ?? result;
        }
    }
    
    private TModel? ReadInternal<TModel, TKey>(TKey key) where TModel : Model<TKey>, new() where TKey : notnull, IComparable<TKey>, IEquatable<TKey> => FullSet().OfType<TModel>().FirstOrDefault(p => p.Id.Equals(key));

    private TModel? ReadInternal<TModel>(object key) where TModel : Model, new() => FullSet().OfType<TModel>().FirstOrDefault(p => p.IdAsString == key.ToString());

    private IEnumerable<Model> FullSet() => _store.Concat(_temp.Select(p => p.NewEntity)).NotNull();

    private ServiceResult? StoreDelete(IEnumerable<ChangeTrackerItem> j)
    {
        foreach (var l in j.ToArray())
        {
            if ((FindOnStore(l) ?? l.OldEntity) is not { } oldEntity) return FailureReason.NotFound;
            _ = _store.Remove(oldEntity) || _temp.Remove(l);
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

    private Model? FindOnStore(ChangeTrackerItem item)
    {
        return _store.OfType(item.Model).FirstOrDefault(p => p.IdAsString == (item.OldEntity?.IdAsString ?? throw new InvalidOperationException()).ToString());
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
        return Execute(CrudAction.Query, () =>
        {
            var r = new QueryServiceResult<TModel>(FullSet().OfType<TModel>().AsQueryable());
            return (r, r);
        });
    }

    /// <inheritdoc/>
    public QueryServiceResult<Model> All(Type model)
    {
        return Execute(CrudAction.Query, () =>
        {
            var r = new QueryServiceResult<Model>(FullSet().OfType(model).AsQueryable());
            return (r, r);
        });
    }

    /// <inheritdoc/>
    public ServiceResult Commit()
    {
        return Execute(CrudAction.Commit, () =>
        {
            var callbacks = new Dictionary<ChangeTrackerChangeType, Func<IEnumerable<ChangeTrackerItem>, ServiceResult?>>
            {
                { ChangeTrackerChangeType.Create, StoreNewEntities },
                { ChangeTrackerChangeType.Update, StoreUpdates },
                { ChangeTrackerChangeType.Delete, StoreDelete }
            };
            foreach (var j in _temp)
            {
                if (callbacks.TryGetValue(j.ChangeType, out var callback) && callback.Invoke(_temp.Where(p => p.ChangeType == j.ChangeType)) is { } fail) return fail;
            }
            _temp.Clear();
            return ServiceResult.Ok;
        });
    }

    /// <inheritdoc/>
    public Task<ServiceResult> CommitAsync()
    {
        return Task.Run(Commit);
    }

    /// <inheritdoc/>
    public ServiceResult Create(params Model[] entities)
    {
        return Execute(CrudAction.Create, () =>
        {
            foreach (var entity in entities)
            {
                if (FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return FailureReason.EntityDuplication;
                }
            }
            _temp.AddRange(entities.Select(p => new ChangeTrackerItem(Map(CrudAction.Create), p)));
            return ServiceResult.Ok;
        }, entities);
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel>(params TModel[] entities) where TModel : Model
    {
        return Execute(CrudAction.Delete, () =>
        {
            if (!FullSet().Any()) return FailureReason.NotFound;
            foreach (var entity in entities)
            {
                if (!FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return FailureReason.NotFound;
                }
            }
            _temp.AddRange(entities.Select(p => new ChangeTrackerItem(Map(CrudAction.Delete), p)));
            return ServiceResult.Ok;
        });
    }

    /// <inheritdoc/>
    public ServiceResult Delete(params Model[] entities)
    {
        return Execute(CrudAction.Delete, () =>
        {
            if (!FullSet().Any()) return FailureReason.NotFound;
            foreach (var entity in entities)
            {
                if (!FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return FailureReason.NotFound;
                }
            }
            _temp.AddRange(entities.Select(p => new ChangeTrackerItem(Map(CrudAction.Delete), p)));
            return ServiceResult.Ok;
        });
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel, TKey>(params TKey[] keys)
        where TModel : Model<TKey>, new()
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        return Execute(CrudAction.Delete, () =>
        {
            if (!FullSet().Any()) return FailureReason.NotFound;
            foreach (var key in keys)
            {
                if (!FullSet().OfType<TModel>().Any(p => p.Id.Equals(key) && p.GetType().IsAssignableTo(typeof(TModel))))
                {
                    return FailureReason.NotFound;
                }
            }
            _temp.AddRange(keys.Select(p => new ChangeTrackerItem(Map(CrudAction.Delete), ReadInternal<TModel, TKey>(p))));
            return ServiceResult.Ok;
        }, keys.Select(ReadInternal<TModel, TKey>).NotNull());
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel>(params string[] stringKeys) where TModel : Model, new()
    {
        return Execute(CrudAction.Delete, () =>
        {
            if (!FullSet().Any()) return FailureReason.NotFound;
            foreach (var key in stringKeys)
            {
                if (!FullSet().Any(p => p.IdAsString == key && p.GetType().IsAssignableTo(typeof(TModel))))
                {
                    return FailureReason.NotFound;
                }
            }
            _temp.AddRange(stringKeys.Select(p => new ChangeTrackerItem(Map(CrudAction.Delete), Read<TModel>(p))));
            return ServiceResult.Ok;
        });
    }

    /// <inheritdoc/>
    public ServiceResult Discard()
    {
        return Execute(CrudAction.Discard, () =>
        {
            _temp.Clear();
            return ServiceResult.Ok;
        });
    }

    /// <inheritdoc/>
    public ServiceResult<TModel?> Read<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        return Execute(CrudAction.Read, () =>
        {
            var result = ReadInternal<TModel, TKey>(key);
            return (new ServiceResult<TModel?>(result), ((IEnumerable<TModel?>)[result]).NotNull());
        });
    }

    /// <inheritdoc/>
    public ServiceResult Read<TModel, TKey>(TKey key, out TModel? entity)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        entity = Execute(CrudAction.Read, () => new ServiceResult<TModel?>(ReadInternal<TModel, TKey>(key)));
        return entity is not null ? ServiceResult.Ok : FailureReason.NotFound;
    }

    /// <inheritdoc/>
    public ServiceResult<TModel?> Read<TModel>(object key) where TModel : Model, new()
    {
        return Execute(CrudAction.Read, () => new ServiceResult<TModel?>(ReadInternal<TModel>(key)));
    }

    /// <inheritdoc/>
    public ServiceResult<Model?> Read(Type model, object key)
    {
        return Execute(CrudAction.Read, () => new ServiceResult<Model?>(FullSet().OfType(model).FirstOrDefault(p => p.IdAsString == key.ToString())));
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel?>> ReadAsync<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        return Task.FromResult(Read<TModel, TKey>(key));
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel?>> ReadAsync<TModel>(object key) where TModel : Model, new()
    {
        return Task.FromResult(Read<TModel>(key));
    }

    /// <inheritdoc/>
    public Task<ServiceResult<Model?>> ReadAsync(Type model, object key)
    {
        return Task.FromResult(Read(model, key));
    }

    /// <inheritdoc/>
    public Task<ServiceResult<TModel[]?>> SearchAsync<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : Model
    {
        return Task.FromResult(Execute(CrudAction.Query, () => new ServiceResult<TModel[]?>([.. FullSet().OfType<TModel>().Where(predicate.Compile())])));
    }

    /// <inheritdoc/>
    public ServiceResult Update<TModel>(params TModel[] entities) where TModel : Model
    {
        return Execute2(CrudAction.Update, () =>
        {
            if (!FullSet().Any()) return (FailureReason.NotFound, null);
            foreach (var entity in entities)
            {
                if (!FullSet().Any(p => p.IdAsString == entity.IdAsString && entity.GetType() == p.GetType()))
                {
                    return (FailureReason.NotFound, null);
                }
            }
            _temp.AddRange(entities.Select(p => new ChangeTrackerItem(Map(CrudAction.Update), p)));
            return (ServiceResult.Ok, entities);
        }, entities);
    }

    /// <inheritdoc/>
    public ServiceResult Update(params Model[] entities)
    {
        return Execute(CrudAction.Update, () =>
        {
            _temp.AddRange(entities.Select(p => new ChangeTrackerItem(Map(CrudAction.Update), p)));
            return ServiceResult.Ok;
        });
    }

    /// <summary>
    /// Libera los recursos desechables utilizados por esta instancia.
    /// </summary>
    protected override void OnDispose()
    {
        if (_temp.Count != 0) ((ICrudWriteTransaction)this).CommitAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Libera de forma asíncrona los recursos desechables utilizados por
    /// esta instancia.
    /// </summary>
    /// <returns>
    /// Un objeto que puede ser utilizado para monitorear el estado de la
    /// tarea.
    /// </returns>
    protected override async ValueTask OnDisposeAsync()
    {
        if (_temp.Count != 0) await ((ICrudWriteTransaction)this).CommitAsync().ConfigureAwait(false);
    }
}
