﻿using TheXDS.MCART.Resources;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Component;
using TheXDS.Triton.Models;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Middleware;

/// <summary>
/// Middleware que permite incluir una capa de seguridad en la capa de
/// acceso a datos cuyas reglas están basadas en la información de acceso
/// del sistema.
/// </summary>
/// <param name="securityActorProvider">
/// Proveedor del objeto de seguridad que intenta ejecutar la acción.
/// </param>
/// <param name="userService"></param>
public class DataLayerSecurityMiddleware(ISecurityActorProvider securityActorProvider, IUserService userService) : ITransactionMiddleware
{
    private readonly IUserService _userService = userService;
    private readonly ISecurityActorProvider _securityActorProvider = securityActorProvider;

    ServiceResult? ITransactionMiddleware.PrologAction(CrudAction action, IEnumerable<Model>? entities)
    {
        if (entities is null) return null;
        if (_securityActorProvider.GetActor() is not { } actor) return FailureReason.Tamper;

        var entityType = entities.GetType();

        return _userService.CheckAccess(actor, GetModelContextString(action, entityType), MapCrudActionToFlags(action)).Result == true
            ? null
            : FailureReason.Forbidden;
    }

    /// <summary>
    /// Obtiene una cadena que puede utilizarse para representar la
    /// operación especificada sobre el modelo en un contexto de seguridad.
    /// </summary>
    /// <param name="action">Acción de Crud a ejecutar.</param>
    /// <param name="model">
    /// Modelo para el cual definir la cadena de contexto de seguridad.
    /// </param>
    /// <returns>
    /// Una cadena que representa la operación Crud sobre el modelo
    /// especificado en un contexto de seguridad.
    /// </returns>
    public static string GetModelContextString(CrudAction action, Type model)
    {
        if (!model.Implements<IEnumerable<Model>>()) throw Errors.UnexpectedType(model, typeof(IEnumerable<Model>));
        return $"{typeof(CrudAction).FullName}.{action};{model.GetCollectionType().CSharpName()}";
    }

    /// <summary>
    /// Obtiene una cadena que puede utilizarse para representar la
    /// operación especificada sobre el modelo en un contexto de seguridad.
    /// </summary>
    /// <param name="action">Acción de Crud a ejecutar.</param>
    /// <typeparam name="TModel">
    /// Modelo para el cual definir la cadena de contexto de seguridad.
    /// </typeparam>
    /// <returns>
    /// Una cadena que representa la operación Crud sobre el modelo
    /// especificado en un contexto de seguridad.
    /// </returns>
    public static string GetModelContextString<TModel>(CrudAction action) where TModel : Model
    {
        return GetModelContextString(action, typeof(TModel[]));
    }

    private static PermissionFlags MapCrudActionToFlags(CrudAction action)
    {
        return action switch
        {
            CrudAction.Create => PermissionFlags.Create,
            CrudAction.Read => PermissionFlags.Read,
            CrudAction.Update => PermissionFlags.Update,
            CrudAction.Delete => PermissionFlags.Delete,
            CrudAction.Commit => PermissionFlags.None,
            _ => PermissionFlags.All
        };
    }
}
