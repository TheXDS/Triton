using TheXDS.Triton.Services;

namespace TheXDS.Triton.EFCore.Services.Base;

/// <summary>
/// Defines a set of members to be implemented by a type that allows
/// performing read and write transaction-based operations on a database.
/// </summary>
public interface ICrudReadWriteTransaction<TContext> : ICrudReadWriteTransaction where TContext : DbContext
{
    /// <summary>
    /// Gets the active context instance in this transaction.
    /// </summary>
    TContext Context { get; }
}
