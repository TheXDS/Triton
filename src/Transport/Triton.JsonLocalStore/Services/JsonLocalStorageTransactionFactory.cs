using System.Text.Json;
using System.Text.Json.Serialization;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.JsonLocalStore.Services;

internal class JsonLocalStorageTransactionFactory(string path) : ITransactionFactory
{
    private readonly string _path = path;

    public ICrudReadWriteTransaction GetTransaction(IMiddlewareRunner runner)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Defines a configuration repository that loads and saves data in Json
/// format.
/// </summary>
/// <typeparam name="T">Type of data store.</typeparam>
/// <param name="store">Data store to use.</param>
/// <param name="jsonOptions">Json serialization options to use.</param>
public class JsonRepository<T>(IDataStore store, JsonSerializerOptions jsonOptions) : IRepository<T> where T : notnull
{
    private static JsonSerializerOptions GetDefaultSerializationOptions() => new() { Converters = { new JsonStringEnumConverter() } };
    private readonly IDataStore store = store;
    private readonly JsonSerializerOptions jsonOptions = jsonOptions;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="JsonRepository{T}"/> class, specifying the
    /// configuration store to use and leaving all Json serialization options
    /// to their defaults.
    /// </summary>
    /// <param name="store">Configuration store to use.</param>
    public JsonRepository(IDataStore store) : this(store, GetDefaultSerializationOptions()) { }

    async Task<T?> IRepository<T>.Load()
    {
        if (store.CanOpenStream())
        {
            using var fs = store.GetReadStream();
            return await JsonSerializer.DeserializeAsync<T>(fs, jsonOptions);
        }
        return default;
    }

    Task IRepository<T>.Save(T configuration)
    {
        using var fs = store.GetWriteStream();
        return JsonSerializer.SerializeAsync(fs, configuration, jsonOptions);
    }
}
