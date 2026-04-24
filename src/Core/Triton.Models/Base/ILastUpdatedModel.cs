namespace TheXDS.Triton.Models.Base;

/// <summary>
/// Defines a contract for entities that track the date and time they were last updated.
/// </summary>
public interface ILastUpdatedModel
{
    /// <summary>
    /// Gets or sets a last updated timestamp associated with this entity.
    /// </summary>
    DateTime LastUpdated { get; set; }
}