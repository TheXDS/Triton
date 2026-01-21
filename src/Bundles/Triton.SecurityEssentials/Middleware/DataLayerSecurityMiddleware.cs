using TheXDS.MCART.Resources;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Component;
using TheXDS.Triton.Models;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Middleware;

/// <summary>
/// Middleware that secures data access by enforcing system-based access rules.
/// </summary>
/// <param name="securityActorProvider">
/// The provider of the security actor attempting to execute an action.
/// </param>
/// <param name="userService">
/// The user service used to authenticate and authorize actions.
/// </param>
public class DataLayerSecurityMiddleware(ISecurityActorProvider securityActorProvider, IUserService userService) : ITransactionMiddleware
{
    private readonly IUserService _userService = userService;
    private readonly ISecurityActorProvider _securityActorProvider = securityActorProvider;

    ServiceResult? ITransactionMiddleware.PrologueAction(in CrudAction action, IEnumerable<ChangeTrackerItem>? entities)
    {
        if (entities is null) return null;
        if (_securityActorProvider.GetCurrentActor() is not { } actor) return FailureReason.Tamper;

        var entityTypes = entities.Select(p => p.Model);

        foreach (var entityType in entityTypes)
        {
            if (_userService.CheckAccess(actor, GetModelContextString(action, entityType), MapCrudActionToFlags(action)).Result != true)
            {
                return FailureReason.Forbidden;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns a security context string for the specified CRUD action on the
    /// given model.
    /// </summary>
    /// <param name="action">The CRUD action to execute.</param>
    /// <param name="model">
    /// The model that defines the security context.
    /// </param>
    /// <returns>
    /// A string representing the operation on the model in a security context.
    /// </returns>
    public static string GetModelContextString(in CrudAction action, Type model)
    {
        if (!(model.Implements<Model>() || model.Implements<IEnumerable<Model>>())) throw Errors.UnexpectedType(model, typeof(Model));
        return $"{typeof(CrudAction).FullName}.{action};{model.CSharpName()}";
    }

    /// <summary>
    /// Returns a security context string for the specified CRUD action on the
    /// given model.
    /// </summary>
    /// <param name="action">The CRUD action to execute.</param>
    /// <typeparam name="TModel">
    /// The type of the model that defines the security context.
    /// </typeparam>
    /// <returns>
    /// A string representing the operation on the model in a security context.
    /// </returns>
    public static string GetModelContextString<TModel>(in CrudAction action) where TModel : Model
    {
        return GetModelContextString(action, typeof(TModel));
    }

    private static PermissionFlags MapCrudActionToFlags(CrudAction action)
    {
        return action switch
        {
            CrudAction.Write => PermissionFlags.Create,
            CrudAction.Read => PermissionFlags.Read,
            CrudAction.Commit => PermissionFlags.None,
            _ => PermissionFlags.All
        };
    }
}
