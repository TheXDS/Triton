using Dapper;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using TheXDS.MCART.Helpers;
using TheXDS.MCART.Resources;
using TheXDS.MCART.Types.Base;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;
using static Dapper.SqlMapper;

namespace TheXDS.Triton.Dapper.Services;

/// <summary>
/// Obtiene una transacción que permite operaciones de lectura y de escritura
/// sobre una base de datos.
/// </summary>
public class DapperTransaction : AsyncDisposable, ICrudReadWriteTransaction
{
    private readonly IDbConnection _connection;
    private readonly IDbTransaction _transaction;
    private readonly IDictionary<Type, DapperModelDescriptor> overrides;
    private readonly IMiddlewareRunner runner;

    internal DapperTransaction(IDbConnection connection, IDictionary<Type, DapperModelDescriptor> overrides, IMiddlewareRunner runner)
    {
        _connection = connection;
        _transaction = _connection.BeginTransaction();
        this.overrides = overrides;
        this.runner = runner;
    }

    /// <inheritdoc/>
    public QueryServiceResult<TModel> All<TModel>() where TModel : Model
    {
        try
        {
            return new QueryServiceResult<TModel>(AllInternal<TModel>());
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public QueryServiceResult<Model> All(Type model)
    {
        try
        {
            return new QueryServiceResult<Model>(AllInternal(model));
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult<TModel?> Read<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        try
        {
            return ReadInternal<TModel, TKey>(key);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult<TModel?> Read<TModel>(object key) where TModel : Model, new()
    {
        try
        {
            return ReadInternal<TModel>(key);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult Read<TModel, TKey>(TKey key, out TModel? entity)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        try
        {
            entity = ReadInternal<TModel, TKey>(key);
            return entity != null ? ServiceResult.Ok : FailureReason.NotFound;
        }
        catch (Exception ex)
        {
            entity = null;
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult<Model?> Read(Type model, object key)
    {
        try
        {
            return ReadInternal(model, key);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<TModel?>> ReadAsync<TModel>(object key) where TModel : Model, new()
    {
        try
        {
            return await ReadInternalAsync<TModel>(key);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<Model?>> ReadAsync(Type model, object key)
    {
        try
        {
            return await ReadInternalAsync(model, key);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<TModel[]?>> SearchAsync<TModel>(Expression<Func<TModel, bool>> predicate) where TModel : Model
    {
        try
        {
            return (await All<TModel>().Where(predicate).ToListAsync()).ToArray();
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> CommitAsync()
    {
        try
        {
            await Task.Run(_transaction.Commit);
            return ServiceResult.Ok;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult Commit()
    {
        try
        {
            _transaction.Commit();
            return ServiceResult.Ok;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult Create<TModel>(params TModel[] newEntities) where TModel : Model
    {
        try
        {
            var tblName = GetTableName<TModel>();
            var columns = string.Concat(",", EnumerateColumns<TModel>());
            var values = string.Concat(",", EnumerateProps<TModel>("@"));
            foreach (var entity in newEntities)
            {
                _connection.Execute($"INSERT INTO {tblName} ({columns}) VALUES ({values});", entity, _transaction);
            }
            return ServiceResult.Ok;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult Create(params Model[] entities)
    {
        foreach (var j in entities.GroupBy(p => p.GetType()))
        {
            try
            {
                var tblName = GetTableName(j.Key);
                var columns = string.Concat(",", EnumerateColumns(j.Key));
                var values = string.Concat(",", EnumerateProps(j.Key, "@"));
                foreach (var entity in j)
                {
                    _connection.Execute($"INSERT INTO {tblName} ({columns}) VALUES ({values});", entity, _transaction);
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        return ServiceResult.Ok;
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel>(params TModel[] entities) where TModel : Model
    {
        try
        {
            var tblName = GetTableName<TModel>();
            var idColumn = GetProp<TModel>("Id");
            foreach (var entity in entities)
            {
                _connection.Execute($"DELETE FROM {tblName} WHERE {idColumn} = @Id;", entity, _transaction);
            }
            return ServiceResult.Ok;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel, TKey>(params TKey[] keys)
        where TModel : Model<TKey>, new()
        where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        return Delete<TModel>(keys.Select(p => p.ToString()).NotNull().ToArray());
    }

    /// <inheritdoc/>
    public ServiceResult Delete<TModel>(params string[] stringKeys) where TModel : Model, new()
    {
        try
        {
            var tblName = GetTableName<TModel>();
            var idColumn = GetProp<TModel>("Id");
            foreach (var key in stringKeys)
            {
                _connection.Execute($"DELETE FROM {tblName} WHERE {idColumn} = @Id;", new { Id = key }, _transaction);
            }
            return ServiceResult.Ok;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult Delete(params Model[] entities)
    {
        foreach (var j in entities.GroupBy(p => p.GetType()))
        {
            try
            {
                var tblName = GetTableName(j.Key);
                var idColumn = GetProp(j.Key, "Id");
                foreach (var entity in j)
                {
                    _connection.Execute($"DELETE FROM {tblName} WHERE {idColumn} = @Id;", entity, _transaction);
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        return ServiceResult.Ok;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<TModel?>> ReadAsync<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        try
        {
            return await _connection.QuerySingleOrDefaultAsync<TModel>($"SELECT * FROM {GetTableName<TModel>()} WHERE {GetProp<TModel>("Id")} = @Id", new { Id = key }, _transaction);
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult CreateOrUpdate<TModel>(params TModel[] entities) where TModel : Model
    {
        try
        {
            var tblName = GetTableName<TModel>();
            var createColumns = string.Concat(",", EnumerateColumns<TModel>());
            var createValues = string.Concat(",", EnumerateProps<TModel>("@"));
            var updateColumns = EnumColEqProp<TModel>();
            var idColumn = GetProp<TModel>("Id");
            foreach (var entity in entities)
            {
                _connection.Execute($@"IF NOT EXISTS ( SELECT TOP 1 FROM {tblName} WHERE {idColumn} = @Id )
INSERT INTO {tblName} ({createColumns}) VALUES ({createValues});
ELSE UPDATE {tblName} SET {updateColumns} WHERE {idColumn} = @Id;", entity, _transaction);
            }
            return ServiceResult.Ok;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult Update<TModel>(params TModel[] entities) where TModel : Model
    {
        try
        {
            var tblName = GetTableName<TModel>();
            var columns = EnumColEqProp<TModel>();
            var idColumn = GetProp<TModel>("Id");
            foreach (var entity in entities)
            {
                _connection.Execute($"UPDATE {tblName} SET {columns} WHERE {idColumn} = @Id;", entity, _transaction);
            }
            return ServiceResult.Ok;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public ServiceResult Update(params Model[] entities)
    {
        foreach (var j in entities.GroupBy(p => p.GetType()))
        {
            try
            {
                var tblName = GetTableName(j.Key);
                var columns = EnumColEqProp(j.Key);
                var idColumn = GetProp(j.Key, "Id");
                foreach (var entity in j)
                {
                    _connection.Execute($"UPDATE {tblName} SET {columns} WHERE {idColumn} = @Id;", entity, _transaction);
                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        return ServiceResult.Ok;
    }

    /// <inheritdoc/>
    public ServiceResult Discard()
    {
        _transaction.Rollback();
        return ServiceResult.Ok;
    }

    /// <inheritdoc/>
    protected override void OnDispose()
    {
        _connection.Dispose();
    }

    /// <inheritdoc/>
    protected override ValueTask OnDisposeAsync()
    {
        _connection.Dispose();
        return ValueTask.CompletedTask;
    }

    private string GetTableName<TModel>() where TModel : Model => GetTableName(typeof(TModel));

    private string GetTableName(Type model)
    {
        return ModelOverriden(model)?.TableName ?? model.Name;
    }

    private IEnumerable<string> EnumerateColumns<TModel>(string prefix = "") => EnumerateColumns(typeof(TModel), prefix);

    private IEnumerable<string> EnumerateColumns(Type model, string prefix = "")
    {
        foreach (var j in EnumerateProps(model))
        {
            yield return $"{prefix}{GetProp(model, j)}";
        }
    }

    private string GetProp<TModel>(string prop) => GetProp(typeof(TModel), prop);

    private string GetProp(Type model, string prop)
    {
        return ModelOverriden(model)?.Properties is { } d && d.TryGetValue(prop, out var k) ? k : prop;
    }

    private static IEnumerable<string> EnumerateProps<TModel>(string prefix = "") => EnumerateProps(typeof(TModel), prefix);

    private static IEnumerable<string> EnumerateProps(Type model, string prefix = "")
    {
        return model.GetProperties().Where(p => p.CanRead && p.CanWrite).Select(p => $"{prefix}{p.Name}");
    }

    private string EnumColEqProp<TModel>() => EnumColEqProp(typeof(TModel));

    private string EnumColEqProp(Type model)
    {
        return string.Join(",", EnumerateColumns(model).Zip(EnumerateProps(model)).Select((p, q) => $"{p} = @{q}"));
    }

    private DapperModelDescriptor? ModelOverriden(Type model)
    {
        return overrides.TryGetValue(model, out var overrideInfo) ? overrideInfo : null;
    }

    private IQueryable<TModel> AllInternal<TModel>() where TModel : Model
    {
        return _connection
            .Query<TModel>($"SELECT * FROM {GetTableName<TModel>()};", transaction: _transaction, buffered: false)
            .AsQueryable();
    }

    private IQueryable<Model> AllInternal(Type model)
    {
        return _connection
            .Query(model, $"SELECT * FROM {GetTableName(model)};", transaction: _transaction, buffered: false)
            .Cast<Model>()
            .AsQueryable();
    }

    private TModel? ReadInternal<TModel, TKey>(TKey key)
        where TModel : Model<TKey>, new()
        where TKey : notnull, IComparable<TKey>, IEquatable<TKey>
    {
        return ReadInternal<TModel>(key);
    }

    private Model? ReadInternal(Type model, object key)
    {
        return (Model?)_connection.QuerySingleOrDefault(GetReadQuery(model), new { Id = key.ToString() }, _transaction);
    }

    private TModel? ReadInternal<TModel>(object key)
        where TModel : Model, new()
    {
        return _connection.QuerySingleOrDefault<TModel>(GetReadQuery<TModel>(), new { Id = key.ToString() }, _transaction);
    }

    private Task<TModel?> ReadInternalAsync<TModel>(object key)
        where TModel : Model, new()
    {
        return _connection.QuerySingleOrDefaultAsync<TModel>(GetReadQuery<TModel>(), new { Id = key.ToString() }, _transaction);
    }

    private async Task<Model?> ReadInternalAsync(Type model, object key)
    {
        return (Model?)await _connection.QuerySingleOrDefaultAsync(model, GetReadQuery(model), new { Id = key.ToString() }, _transaction);
    }

    private string GetReadQuery<TModel>() where TModel : Model => GetReadQuery(typeof(TModel));

    private string GetReadQuery(Type model)
    {
        return $"SELECT * FROM {GetTableName(model)} WHERE {GetProp(model, "Id")} = @Id";
    }
}