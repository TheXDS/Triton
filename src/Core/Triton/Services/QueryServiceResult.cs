﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TheXDS.Triton.Models.Base;

namespace TheXDS.Triton.Services
{
    /// <summary>
    ///     Representa el resultado de una operación de servicio que incluye
    ///     una consulta de tipo <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">
    ///     Tipo de entidades a devolver.
    /// </typeparam>
    public class QueryServiceResult<T> : ServiceResult, IQueryable<T> where T: Model
    {
        private IQueryable<T>? _result;

        public Type ElementType => _result?.ElementType ?? throw new InvalidOperationException();

        public Expression Expression => _result?.Expression ?? throw new InvalidOperationException();

        public IQueryProvider Provider => _result?.Provider ?? throw new InvalidOperationException();

        public IEnumerator<T> GetEnumerator() => _result?.GetEnumerator() ?? throw new InvalidOperationException();

        IEnumerator IEnumerable.GetEnumerator() => _result?.GetEnumerator() ?? throw new InvalidOperationException();

        public QueryServiceResult()
        {
        }

        public QueryServiceResult(IQueryable<T> query)
        {
            _result = query;
        }


        /// <summary>
        ///     Inicializa una nueva instancia de la clase
        ///     <see cref="QueryServiceResult{T}"/>, especificando el motivo por el 
        ///     cual la operación ha fallado.
        /// </summary>
        /// <param name="reason">
        ///     Motivo por el cual la operación ha fallado.
        /// </param>
        public QueryServiceResult(in FailureReason reason) : base(reason)
        {
        }

        /// <summary>
        ///     Inicializa una nueva instancia de la clase
        ///     <see cref="QueryServiceResult{T}"/>, especificando el motivo por el 
        ///     cual la operación ha fallado, además de un mensaje descriptivo
        ///     del resultado.
        /// </summary>
        /// <param name="reason">
        ///     Motivo por el cual la operación ha fallado.
        /// </param>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public QueryServiceResult(in FailureReason reason, string message) : base(reason, message)
        {
        }

        /// <summary>
        ///     Inicializa una nueva instancia de la clase
        ///     <see cref="QueryServiceResult{T}"/> para una operación fallida,
        ///     extrayendo la información relevante a partir de la excepción
        ///     especificada.
        /// </summary>
        /// <param name="ex">
        ///     Excepción desde la cual obtener el mensaje y un código de error
        ///     asociado.
        /// </param>
        public QueryServiceResult(Exception ex) : base(ex)
        {
        }

        /// <summary>
        ///     Inicializa una nueva instancia de la clase
        ///     <see cref="QueryServiceResult{T}"/>, indicando un mensaje de estado 
        ///     personalizado a mostrar.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public QueryServiceResult(string message) : base(FailureReason.Unknown, message)
        {
        }

        /// <summary>
        ///     Convierte implícitamente un <see cref="Exception"/> en un
        ///     <see cref="ServiceResult{T}"/>.
        /// </summary>
        /// <param name="ex">
        ///     Excepción desde la cual obtener el mensaje y un código de error
        ///     asociado.
        /// </param>
        public static implicit operator QueryServiceResult<T>(Exception ex) => new QueryServiceResult<T>(ex);

        /// <summary>
        ///     Convierte implícitamente un <see cref="string"/> en un
        ///     <see cref="ServiceResult{T}"/>.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public static implicit operator QueryServiceResult<T>(string message) => new QueryServiceResult<T>(message);

        /// <summary>
        ///     Convierte implícitamente un <see cref="FailureReason"/> en un
        ///     <see cref="ServiceResult{T}"/>.
        /// </summary>
        /// <param name="reason">
        ///     Motivo por el cual la operación ha fallado.
        /// </param>
        public static implicit operator QueryServiceResult<T>(in FailureReason reason) => new QueryServiceResult<T>(reason);
    }
}