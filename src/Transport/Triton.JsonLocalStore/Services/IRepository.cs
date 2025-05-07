namespace TheXDS.Triton.JsonLocalStore.Services;

/// <summary>
/// Defines a set of members to be implemented by a type that loads and saves
/// data records as defined in the specified type.
/// </summary>
/// <typeparam name="T">
/// Type that contains the desired data schema.
/// </typeparam>
public interface IRepository<T> where T : notnull
{
    /// <summary>
    /// Attempts to load the data from this repository.
    /// </summary>
    /// <returns>
    /// A task that upon completion returns a new instance of the
    /// <typeparamref name="T"/> class with all its property values loaded from
    /// this repository or <see langword="null"/> if the data could
    /// not be loaded after completing the task.
    /// </returns>
    Task<T?> Load();

    /// <summary>
    /// Saves the specified configuration into this data repository.
    /// </summary>
    /// <param name="data">Data to save.</param>
    /// <returns>
    /// A task that can be used to await the async operation.
    /// </returns>
    Task Save(T data);
}
