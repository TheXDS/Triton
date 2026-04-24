using System.Collections.ObjectModel;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Middleware;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Diagnostics.Middleware;

/// <summary>
/// Middleware that allows observing the current state of changes to be applied
/// in an active transaction.
/// </summary>
public class ChangeTrackerObserverMiddleware : ITransactionMiddleware
{
    private readonly ObservableCollection<ChangeTrackerItem> _items = [];

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ChangeTrackerObserverMiddleware"/> class.
    /// </summary>
    public ChangeTrackerObserverMiddleware()
    {
        Changes = new(_items);
    }

    /// <summary>
    /// Gets a reference to an observable collection that contains the current
    /// state of changes being applied in the active transaction.
    /// </summary>
    public ReadOnlyObservableCollection<ChangeTrackerItem> Changes { get; }

    ServiceResult? ITransactionMiddleware.PrologueAction(in CrudAction action, IEnumerable<ChangeTrackerItem>? entities)
    {
        switch (action)
        {
            case CrudAction.Write when entities is not null && entities.Any():
                _items.AddRange(entities); break;
        }
        return null;
    }

    ServiceResult? ITransactionMiddleware.EpilogueAction(in CrudAction action, IEnumerable<ChangeTrackerItem>? entities)
    {
        switch (action)
        {
            case CrudAction.Commit:
            case CrudAction.Discard:
                _items.Clear(); break;
        }
        return null;
    }
}
