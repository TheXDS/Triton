namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// Base class that defines an object that retrieves configuration to use
/// when instantiating a <see cref="DbContext"/> that accepts a
/// <see cref="DbContextOptions"/> parameter.
/// </summary>
/// <typeparam name="TBuilder">
/// Type of object to use for building configuration to instantiate a
/// <see cref="DbContext"/>.
/// </typeparam>
/// <typeparam name="TOptions">
/// Type of object that contains configuration to use when instantiating a
/// <see cref="DbContext"/>.
/// </typeparam>
public abstract class DbContextOptionsSourceBase<TBuilder, TOptions> : IDbContextOptionsSource where TBuilder : DbContextOptionsBuilder, new() where TOptions : DbContextOptions
{
    private readonly Action<TBuilder>? builderCallback;
    private readonly TOptions? options;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DbContextOptionsSourceBase{TBuilder, TOptions}"/> class.
    /// </summary>
    /// <param name="builderCallback">
    /// Method to invoke to configure a
    /// <see cref="DbContextOptionsBuilder"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <see cref="builderCallback"/> is <see langword="null"/>.
    /// </exception>
    protected DbContextOptionsSourceBase(Action<TBuilder> builderCallback)
    {
        this.builderCallback = builderCallback ?? throw new ArgumentNullException(nameof(builderCallback));
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DbContextOptionsSourceBase{TBuilder, TOptions}"/> class.
    /// </summary>
    /// <param name="options">
    /// Static options object to return when a
    /// <see cref="DbContext"/> needs to be instantiated.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <see cref="options"/> is <see langword="null"/>.
    /// </exception>
    protected DbContextOptionsSourceBase(TOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DbContextOptionsSourceBase{TBuilder, TOptions}"/> class,
    /// explicitly omitting both a configuration method and static configuration.
    /// </summary>
    protected DbContextOptionsSourceBase()
    {
    }

    DbContextOptions? IDbContextOptionsSource.GetOptions()
    {
        if (options is not null) return options;
        if (builderCallback is null) return null;
        var b = new TBuilder();
        builderCallback.Invoke(b);
        return b.Options;
    }
}
