namespace TheXDS.Triton.Services;

/// <summary>
/// Enumerates existing CRUD operations.
/// </summary>
public enum CrudAction : byte
{
    /// <summary>
    /// Commit all queued write operations to the database.
    /// </summary>
    Commit,
    /// <summary>
    /// Read an entity.
    /// </summary>
    Read,
    /// <summary>
    /// Queues a write operation. Includes create, update and delete operations.
    /// </summary>
    Write,
    /// <summary>
    /// Discard pending changes to write in the database.
    /// </summary>
    Discard,
    /// <summary>
    /// Enumerate entities directly according to a filter function.
    /// </summary>
    Query,
}