using System.Linq.Expressions;
using TheXDS.MCART.Types.Base;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.JsonLocalStore.Services;

internal record JsonRecord(string TypeId, object? Data);

internal class JsonLocalStorageTransaction : AsyncDisposable, ICrudReadWriteTransaction
{
    public QueryServiceResult<TModel> All<TModel>() where TModel : Model
    {
        throw new NotImplementedException();
    }

    public QueryServiceResult<Model> All(Type model)
    {
        throw new NotImplementedException();
    }

    public ServiceResult Commit()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult> CommitAsync()
    {
        throw new NotImplementedException();
    }

    public ServiceResult Create(params Model[] entities)
    {
        throw new NotImplementedException();
    }

    public ServiceResult Delete<TModel>(params TModel[] entities) where TModel : Model
    {
        throw new NotImplementedException();
    }

    public ServiceResult Delete(params Model[] entities)
    {
        throw new NotImplementedException();
    }

    public ServiceResult Delete<TModel, TKey>(params TKey[] keys)
        where TModel : Model<TKey>, new()
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        throw new NotImplementedException();
    }

    public ServiceResult Delete<TModel>(params string[] stringKeys) where TModel : Model, new()
    {
        throw new NotImplementedException();
    }

    public ServiceResult Discard()
    {
        throw new NotImplementedException();
    }

    public ServiceResult<TModel?> Read<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        throw new NotImplementedException();
    }

    public ServiceResult Read<TModel, TKey>(TKey key, out TModel? entity)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        throw new NotImplementedException();
    }

    public ServiceResult<TModel?> Read<TModel>(object key) where TModel : Model, new()
    {
        throw new NotImplementedException();
    }

    public ServiceResult<Model?> Read(Type model, object key)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<TModel?>> ReadAsync<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<TModel?>> ReadAsync<TModel>(object key) where TModel : Model, new()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<Model?>> ReadAsync(Type model, object key)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<TModel[]?>> SearchAsync<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : Model
    {
        throw new NotImplementedException();
    }

    public ServiceResult Update<TModel>(params TModel[] entities) where TModel : Model
    {
        throw new NotImplementedException();
    }

    public ServiceResult Update(params Model[] entities)
    {
        throw new NotImplementedException();
    }

    protected override void OnDispose()
    {
        throw new NotImplementedException();
    }

    protected override ValueTask OnDisposeAsync()
    {
        throw new NotImplementedException();
    }
}
