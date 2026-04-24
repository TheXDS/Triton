namespace TheXDS.Triton.JsonLocalStore.Services;

/// <summary>
/// Defines a set of members to be implemented by a type that provides read and
/// write capabilities for data.
/// </summary>
public interface IDataStore
{
    /// <summary>
    /// Indicates if the store is available for read operations.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the store can be opened for read,
    /// <see langword="false"/> otherwise.
    /// </returns>
    bool CanOpenStream();

    /// <summary>
    /// Gets a <see cref="Stream"/> that can be used to read data from.
    /// </summary>
    /// <returns>
    /// A <see cref="Stream"/> that can be used to read data from.
    /// </returns>
    Stream GetReadStream();

    /// <summary>
    /// Gets a <see cref="Stream"/> that can be used to write data onto.
    /// </summary>
    /// <returns>
    /// A <see cref="Stream"/> that can be used to write data onto.
    /// </returns>
    Stream GetWriteStream();
}
