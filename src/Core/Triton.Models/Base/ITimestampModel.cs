namespace TheXDS.Triton.Models.Base;

/// <summary>
/// Defines a contract for entities that have an associated creation timestamp.
/// </summary>
public interface ITimestampModel
{
    /// <summary>
    /// Gets or sets a creation timestamp associated with this entity.
    /// </summary>
    DateTime? Timestamp { get; set; }
}
