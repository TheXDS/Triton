using TheXDS.Triton.Services;

namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// A transaction factory that connects to an Entity Framework Core data context.
/// </summary>
/// <typeparam name="T">
/// Type of data context to connect to.
/// </typeparam>
public class EfCoreTransFactory<T> : ITransactionFactory where T : DbContext
{
    private readonly IDbContextOptionsSource options;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="EfCoreTransFactory{T}"/> class.
    /// </summary>
    /// <param name="options">
    /// Context options to use when creating instances of
    /// <see cref="DbContext"/>.
    /// </param>
    public EfCoreTransFactory(IDbContextOptionsSource options)
    {
        this.options = options;
    }

    /// <summary>
    /// Gets a transaction that allows reading data from the data context.
    /// </summary>
    /// <param name="runner">
    /// Transaction configuration to use.
    /// </param>
    /// <returns>
    /// A transaction that allows reading data from the data context.
    /// </returns>
    public ICrudReadTransaction GetReadTransaction(IMiddlewareRunner runner)
    {
        return new CrudReadTransaction<T>(runner, options.GetOptions());
    }

    /// <summary>
    /// Gets a transaction that allows writing data to the data context.
    /// </summary>
    /// <param name="runner">
    /// Transaction configuration to use.
    /// </param>
    /// <returns>
    /// A transaction that allows writing data to the data context.
    /// </returns>
    public ICrudWriteTransaction GetWriteTransaction(IMiddlewareRunner runner)
    {
        return new CrudWriteTransaction<T>(runner, options.GetOptions());
    }

    /// <summary>
    /// Gets a transaction that allows reading and writing data from the data context.
    /// </summary>
    /// <param name="runner">
    /// Transaction configuration to use.
    /// </param>
    /// <returns>
    /// A transaction that allows reading and writing data from the data context.
    /// </returns>
    public ICrudReadWriteTransaction GetTransaction(IMiddlewareRunner runner)
    {
        return new CrudTransaction<T>(runner, options.GetOptions());
    }
}
