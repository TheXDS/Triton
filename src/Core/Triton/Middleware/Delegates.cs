﻿using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services;

namespace TheXDS.Triton.Middleware;

/// <summary>
/// Describe un método definido dentro de un <see cref="CrudAction"/> y un
/// <see cref="ITransactionMiddleware"/> que acepta un
/// conjunto de objetos de tipo <see cref="Model"/> como entrada, realiza una
/// operación de servicio y devuelve un <see cref="ServiceResult"/> a la hora
/// de ejecutar acciones Crud.
/// </summary>
/// <param name="crudAction">
/// Acción Crud ejecutada sobre las entidades.
/// </param>
/// <param name="entities">
/// Entidades sobre las cuales se ha ejecutado una acción Crud.
/// </param>
/// <returns>
/// <see langword="null"/> si la acción ha sido satisfactoria y la
/// operación Crud puede continuar normalmente, o un
/// <see cref="ServiceResult"/> que describe el error que ha ocurrido en la
/// acción.
/// </returns>
public delegate ServiceResult? MiddlewareAction(CrudAction crudAction, IEnumerable<Model>? entities);