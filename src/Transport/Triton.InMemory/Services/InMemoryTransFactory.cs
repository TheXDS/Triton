using TheXDS.Triton.Models.Base;

namespace TheXDS.Triton.InMemory.Services;

/// <summary>
/// Transaction factory that creates transactions that read and write data to a
/// collection without persistance (in memory).
/// </summary>
public class InMemoryTransFactory : CollectionTransFactory
{
    private static readonly List<Model> _store = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryTransFactory"/>
    /// class.
    /// </summary>
    public InMemoryTransFactory() : base(_store)
    {
    }

    /// <summary>
    /// Clears the entire data store.
    /// </summary>
    public static void Wipe()
    {
        _store.Clear();
    }
}
