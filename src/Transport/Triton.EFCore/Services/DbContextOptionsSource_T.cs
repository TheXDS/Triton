namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// Implements an <see cref="IDbContextOptionsSource"/> for configurations
/// that require precisely knowing the type of context to configure.
/// </summary>
public class DbContextOptionsSource<T> : DbContextOptionsSourceBase<DbContextOptionsBuilder<T>, DbContextOptions<T>> where T : DbContext
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DbContextOptionsSource"/> class.
    /// </summary>
    /// <param name="options">
    /// Static options instance to use when instantiating a
    /// <see cref="DbContext"/>.
    /// </param>
    public DbContextOptionsSource(DbContextOptions<T> options) : base(options)
    {
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DbContextOptionsSource"/> class.
    /// </summary>
    /// <param name="builder">
    /// Method to invoke for configuring options to use when instantiating
    /// <see cref="DbContext"/> objects.
    /// </param>
    public DbContextOptionsSource(Action<DbContextOptionsBuilder<T>> builder) : base(builder)
    {
    }
}