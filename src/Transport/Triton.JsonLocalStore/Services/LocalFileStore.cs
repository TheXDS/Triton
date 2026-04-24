namespace TheXDS.Triton.JsonLocalStore.Services;

/// <summary>
/// Implements a data store that reads and writes data onto a file on the local
/// filesystem.
/// </summary>
/// <param name="fileName">File name to use as the data store.</param>
public class LocalFileStore(string fileName) : IDataStore
{
    private readonly string fileName = fileName;

    /// <inheritdoc/>
    public bool CanOpenStream()
    {
        return File.Exists(fileName);
    }

    /// <inheritdoc/>
    public Stream GetReadStream()
    {
        return new FileStream(fileName, FileMode.Open);
    }

    /// <inheritdoc/>
    public Stream GetWriteStream()
    {
        return new FileStream(fileName, FileMode.Create);
    }
}
