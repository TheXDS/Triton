namespace TheXDS.Triton.Models.Base;

/// <summary>
/// A base class for all Triton data models.
/// </summary>
public abstract class Model
{
    /// <summary>
    /// Gets the metadata of this model.
    /// </summary>
    public IModelMetadata Metadata => this as IModelMetadata ?? throw new InvalidOperationException("This model does not implement IModelMetadata.");
}
