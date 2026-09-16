using TheXDS.Triton.Models.Base;

namespace TheXDS.Triton.Services;

/// <summary>
/// Defines the contract for types that allows performing read and write
/// operations based on transaction over a database.
/// </summary>
public interface ICrudReadWriteTransaction : ICrudReadTransaction, ICrudWriteTransaction
{
    /// <summary>
    /// Creates or updates a set of entities in the database.
    /// </summary>
    /// <typeparam name="TModel">
    /// Model type of the entities.
    /// </typeparam>
    /// <param name="entities">
    /// A collection of entities to be created or updated in the database.
    /// </param>
    /// <returns>
    /// The result reported by the underlying service for the operation
    /// that has been executed.
    /// </returns>
    ServiceResult CreateOrUpdate<TModel>(params TModel[] entities) where TModel : Model => CreateOrUpdate([.. entities.Cast<Model>()]);

    /// <summary>
    /// Creates or updates a set of entities in the database.
    /// </summary>
    /// <param name="entities">
    /// A collection of entities to be created or updated in the database.
    /// </param>
    /// <returns>
    /// The result reported by the underlying service for the operation
    /// that has been executed.
    /// </returns>
    ServiceResult CreateOrUpdate(params Model[] entities);
}
