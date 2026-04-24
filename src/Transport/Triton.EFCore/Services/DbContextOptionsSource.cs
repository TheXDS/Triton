namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// Implements an <see cref="IDbContextOptionsSource"/> for configurations
/// that do not require precisely knowing the type of context to configure.
/// </summary>
public class DbContextOptionsSource : DbContextOptionsSourceBase<DbContextOptionsBuilder, DbContextOptions>
{
    /// <summary>
    /// Gets a <see cref="IDbContextOptionsSource"/> instance that represents
    /// an unconfigured source, i.e., a parameterless constructor will be used
    /// to initialize <see cref="DbContext"/> objects.
    /// </summary>
    public static IDbContextOptionsSource None => new DbContextOptionsSource();

    private DbContextOptionsSource()
    { }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="DbContextOptionsSource"/> class.
    /// </summary>
    /// <param name="options">
    /// Static options instance to use when instantiating a
    /// <see cref="DbContext"/>.
    /// </param>
    public DbContextOptionsSource(DbContextOptions options) : base(options)
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
    public DbContextOptionsSource(Action<DbContextOptionsBuilder> builder) : base(builder)
    {
    }
}
