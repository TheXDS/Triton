using TheXDS.Triton.Exceptions;
using TheXDS.Triton.Models.Base;

namespace TheXDS.Triton.Services;

/// <summary>
/// Represents an entry in a collection that contains information about changes
/// made during a transaction.
/// </summary>
public class ChangeTrackerItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeTrackerItem"/>
    /// class.
    /// </summary>
    /// <param name="changeType">
    /// Type of change represented by this instance.
    /// </param>
    /// <param name="entity">Entity affected by the change.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the affected entity is <see langword="null"/>, except when
    /// the change type is <see cref="ChangeTrackerChangeType.NoChange"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the <paramref name="changeType"/> is not a valid value.
    /// </exception>
    public ChangeTrackerItem(ChangeTrackerChangeType changeType, Model? entity)
    {
        switch (changeType)
        {
            case ChangeTrackerChangeType.Create:
                NewEntity = entity ?? throw new ArgumentNullException(nameof(entity));
                break;
            case ChangeTrackerChangeType.Update:
                OldEntity = entity ?? throw new ArgumentNullException(nameof(entity));
                NewEntity = entity;
                break;
            case ChangeTrackerChangeType.Delete:
                OldEntity = entity;// ?? throw new ArgumentNullException(nameof(entity));
                break;
            case ChangeTrackerChangeType.NoChange:
                OldEntity = entity;
                NewEntity = entity;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeTrackerItem"/>
    /// class.
    /// </summary>
    /// <param name="oldEntity">Old value of the entity.</param>
    /// <param name="newEntity">New value of the entity.</param>
    public ChangeTrackerItem(Model? oldEntity, Model? newEntity)
    {
        if (oldEntity is not null && newEntity is not null && oldEntity.GetType() != newEntity.GetType())
        {
            throw new ModelTypeMismatchException();
        }
        OldEntity = oldEntity;
        NewEntity = newEntity;
    }

    /// <summary>
    /// Gets a value that infers the change represented by this instance.
    /// </summary>
    public ChangeTrackerChangeType ChangeType => (OldEntity, NewEntity) switch
    {
        (null, null) => ChangeTrackerChangeType.NoChange,
        (null, { }) => ChangeTrackerChangeType.Create,
        ({ }, { }) => ChangeTrackerChangeType.Update,
        ({ }, null) => ChangeTrackerChangeType.Delete,
    };

    /// <summary>
    /// Gets a reference to the model of the entity represented by this
    /// change-tracking entry.
    /// </summary>
    public Type Model => (OldEntity ?? NewEntity)?.GetType() ?? typeof(Model);

    /// <summary>
    /// Gets a reference to the old entity for this change.
    /// </summary>
    /// <remarks>
    /// This value may be <see langword="null"/> when this instance represents
    /// either no change or a new entity.
    /// </remarks>
    public Model? OldEntity { get; }

    /// <summary>
    /// Gets a reference to the new entity for this change.
    /// </summary>
    /// <remarks>
    /// This value may be <see langword="null"/> when this instance represents
    /// either no change or the deletion of an entity.
    /// </remarks>
    public Model? NewEntity { get; }
}