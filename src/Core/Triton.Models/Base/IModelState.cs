namespace TheXDS.Triton.Models.Base;

/// <summary>
/// Defines the contract for models to expose their own internal state
/// information.
/// </summary>
public interface IModelMetadata
{
    /// <summary>
    /// Gets the ID of the entity as a string.
    /// </summary>
    string IdAsString { get; }

    /// <summary>
    /// Gets a value indicating whether the model instance represents a new
    /// (unsaved) entity.
    /// </summary>
    bool IsNew { get; }
}
