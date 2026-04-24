namespace TheXDS.Triton.EFCore.Services;

/// <summary>
/// Defines a set of members to be implemented by a type that allows obtaining
/// the configuration to use when instantiating a <see cref="DbContext"/>
/// that accepts a <see cref="DbContextOptions"/> parameter.
/// </summary>
public interface IDbContextOptionsSource
{
    /// <summary>
    /// Gets the configuration to use for creating a new instance of the
    /// <see cref="DbContext"/> class.
    /// </summary>
    /// <returns>
    /// A <see cref="DbContextOptions"/> with the configuration to use
    /// when instantiating a <see cref="DbContext"/>, or <see langword="null"/>
    /// when the parameterless constructor of the <see cref="DbContext"/> to
    /// instantiate should be called.
    /// </returns>
    DbContextOptions? GetOptions();
}
