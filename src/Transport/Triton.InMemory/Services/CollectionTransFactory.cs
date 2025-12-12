using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.InMemory.Services;

/// <summary>
/// Transaction factory that creates transactions that execute data
/// actions on an object of type <see cref="ICollection{T}"/>.
/// </summary>
/// <param name="store">
/// The collection to be managed by this instance.
/// </param>
public class CollectionTransFactory(ICollection<Model> store) : ITransactionFactory
{
    private readonly ICollection<Model> _store = store;

    /// <summary>
    /// Creates a transaction connected to a volatile store without
    /// persistence in the application's memory.
    /// </summary>
    /// <param name="runner">
    /// Transaction configuration to use.
    /// </param>
    /// <returns>
    /// A transaction connected to a volatile store without persistence
    /// in the application's memory.
    /// </returns>
    public ICrudReadWriteTransaction GetTransaction(IMiddlewareRunner runner)
    {
        return new InMemoryCrudTransaction(runner, _store);
    }
}