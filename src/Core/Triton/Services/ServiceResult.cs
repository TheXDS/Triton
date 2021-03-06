﻿using System;
using System.Diagnostics.CodeAnalysis;
using static TheXDS.MCART.Types.Extensions.EnumExtensions;
using St = TheXDS.Triton.Resources.Strings;

namespace TheXDS.Triton.Services
{
    /// <summary>
    /// Representa el resultado devuelto por un servicio al intentar
    /// realizar una operación.
    /// </summary>
    public class ServiceResult : IEquatable<ServiceResult>, IEquatable<Exception>
    {
        private static string MessageFrom(in FailureReason reason)
        {
            return reason switch
            {
                FailureReason.Unknown => St.FailureUnknown,
                FailureReason.Tamper => St.Tamper,
                FailureReason.Forbidden => St.FailureForbidden,
                FailureReason.ServiceFailure => St.ServiceFailure,
                FailureReason.NetworkFailure => St.NetworkFailure,
                FailureReason.DbFailure => St.DbFailure,
                FailureReason.ValidationError => St.ValidationError,
                FailureReason.ConcurrencyFailure => St.ConcurrencyFailure,
                FailureReason.NotFound => St.EntityNotFound,
                FailureReason.BadQuery => St.BadQuery,
                FailureReason.QueryOverLimit => St.QueryOverLimit,
                _ => reason.NameOf() ?? reason.ToString("X").PadLeft(8, '0'),
            };
        }

        /// <summary>
        /// Obtiene un resultado simple que indica que la operación se ha
        /// completado satisfactoriamente.
        /// </summary>
        public static ServiceResult Ok { get; } = SucceedWith<ServiceResult>(null);

        /// <summary>
        /// Obtiene un <typeparamref name="TServiceResult"/> fallido a
        /// partir de la excepción producida.
        /// </summary>
        /// <typeparam name="TServiceResult">
        /// Tipo de <see cref="ServiceResult"/> a generar.
        /// </typeparam>
        /// <param name="ex">
        /// Excepción producida.
        /// </param>
        /// <returns>
        /// Un <typeparamref name="TServiceResult"/> que representa una
        /// operación fallida.
        /// </returns>
        public static TServiceResult FailWith<TServiceResult>(Exception ex) where TServiceResult : ServiceResult, new()
        {
            return new TServiceResult() 
            {
                Success = false,
                Message = ex.Message,
                Reason = (FailureReason)ex.HResult
            };
        }

        /// <summary>
        /// Obtiene un <typeparamref name="TServiceResult"/> fallido a
        /// partir del fallo informado.
        /// </summary>
        /// <typeparam name="TServiceResult">
        /// Tipo de <see cref="ServiceResult"/> a generar.
        /// </typeparam>
        /// <param name="reason">
        /// Fallo producido durante la operación.
        /// </param>
        /// <returns>
        /// Un <typeparamref name="TServiceResult"/> que representa una
        /// operación fallida.
        /// </returns>
        public static TServiceResult FailWith<TServiceResult>(in FailureReason reason) where TServiceResult : ServiceResult, new()
        {
            return new TServiceResult()
            {
                Success = false,
                Reason = reason,
                Message = MessageFrom(reason)
            };
        }

        /// <summary>
        /// Obtiene un <typeparamref name="TServiceResult"/> fallido a
        /// partir del mensaje de error.
        /// </summary>
        /// <typeparam name="TServiceResult">
        /// Tipo de <see cref="ServiceResult"/> a generar.
        /// </typeparam>
        /// <param name="message">
        /// Mensaje de error producido durante la operación.
        /// </param>
        /// <returns>
        /// Un <typeparamref name="TServiceResult"/> que representa una
        /// operación fallida.
        /// </returns>
        public static TServiceResult FailWith<TServiceResult>(string? message) where TServiceResult : ServiceResult, new()
        {
            return new TServiceResult()
            {
                Success = false,
                Reason = FailureReason.Unknown,
                Message = message ?? St.FailureUnknown
            };
        }

        /// <summary>
        /// Obtiene un <typeparamref name="TServiceResult"/> exitoso a
        /// partir del mensaje.
        /// </summary>
        /// <typeparam name="TServiceResult">
        /// Tipo de <see cref="ServiceResult"/> a generar.
        /// </typeparam>
        /// <param name="message">
        /// Mensaje producido por la operación.
        /// </param>
        /// <returns>
        /// Un <typeparamref name="TServiceResult"/> que representa una
        /// operación exitosa con un mensaje de salida.
        /// </returns>
        public static TServiceResult SucceedWith<TServiceResult>(string? message) where TServiceResult : ServiceResult, new()
        {
            return new TServiceResult()
            {
                Success = true,
                Reason = null,
                Message = message ?? St.OperationCompletedSuccessfully
            };
        }

        /// <summary>
        /// Obtiene un valor que indica si la operación ha sido exitosa.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// Obtiene un mensaje que describe el resultado de la operación.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Obtiene la razón por la cual una operación ha fallado, o
        /// <see langword="null"/> si la operación se completó 
        /// exitosamente.
        /// </summary>
        public FailureReason? Reason { get; private set; } = null;

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>, indicando que la operación se ha
        /// completado satisfactoriamente.
        /// </summary>
        public ServiceResult()
        {
            Success = true;
            Message = St.OperationCompletedSuccessfully;
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>, indicando un mensaje de estado 
        /// personalizado a mostrar.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public ServiceResult(string? message)
        {
            Success = true;
            Message = message ?? St.OperationCompletedSuccessfully;
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>, especificando el motivo por el 
        /// cual la operación ha fallado.
        /// </summary>
        /// <param name="reason">
        /// Motivo por el cual la operación ha fallado.
        /// </param>
        public ServiceResult(in FailureReason reason)
        {
            Success = false;
            Reason = reason;
            Message = MessageFrom(reason);
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>, especificando el motivo por el 
        /// cual la operación ha fallado, además de un mensaje descriptivo
        /// del resultado.
        /// </summary>
        /// <param name="reason">
        /// Motivo por el cual la operación ha fallado.
        /// </param>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public ServiceResult(in FailureReason reason, string message)
        {
            Success = false;
            Reason = reason;
            Message = message;
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/> para una operación fallida,
        /// extrayendo la información relevante a partir de la excepción
        /// especificada.
        /// </summary>
        /// <param name="ex">
        /// Excepción desde la cual obtener el mensaje y un código de error
        /// asociado.
        /// </param>
        public ServiceResult(Exception ex)
        {
            Success = false;
            Message = ex.Message;
            Reason = (FailureReason)ex.HResult;
        }

        /// <summary>
        /// Convierte implícitamente un <see cref="Exception"/> en un
        /// <see cref="ServiceResult"/>.
        /// </summary>
        /// <param name="ex">
        /// Excepción desde la cual obtener el mensaje y un código de error
        /// asociado.
        /// </param>
        public static implicit operator ServiceResult(Exception ex) => FailWith<ServiceResult>(ex);

        /// <summary>
        /// Convierte implícitamente un <see cref="string"/> en un
        /// <see cref="ServiceResult"/>.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public static implicit operator ServiceResult(string message) => FailWith<ServiceResult>(message);

        /// <summary>
        /// Convierte implícitamente un <see cref="FailureReason"/> en un
        /// <see cref="ServiceResult"/>.
        /// </summary>
        /// <param name="reason">
        /// Motivo por el cual la operación ha fallado.
        /// </param>
        public static implicit operator ServiceResult(in FailureReason reason) => FailWith<ServiceResult>(reason);

        /// <summary>
        /// Convierte implícitamente un <see cref="bool"/> en un
        /// <see cref="ServiceResult"/>.
        /// </summary>
        /// <param name="success">
        /// Valor que indica si la operación ha tenido éxito o no.
        /// </param>
        public static implicit operator ServiceResult(in bool success)=> success ? Ok : FailWith<ServiceResult>(FailureReason.Unknown);

        /// <summary>
        /// Permite utilizar un <see cref="ServiceResult"/> en una
        /// expresión booleana.
        /// </summary>
        /// <param name="result">
        /// Resultado desde el cual determinar el valor booleano.
        /// </param>
        public static implicit operator bool(ServiceResult result) => result.Success;

        /// <summary>
        /// Permite utilizar un <see cref="ServiceResult"/> en una
        /// expresión de <see cref="string"/>.
        /// </summary>
        /// <param name="result">
        /// Resultado desde el cual extraer el mensaje.
        /// </param>
        public static implicit operator string(ServiceResult result) => result.ToString();

        /// <summary>
        /// Convierte este objeto en su representación como una cadena.
        /// </summary>
        /// <returns>
        /// Una cadena que representa a este objeto.
        /// </returns>
        public override string ToString()
        {
            return Message;
        }

        /// <summary>
        /// Compara la igualdad entre esta y otra instancia de la clase
        /// <see cref="ServiceResult"/>.
        /// </summary>
        /// <param name="other">
        /// Instancia contra la cual comparar esta instancia.
        /// </param>
        /// <returns>
        /// <see langword="true"/> si ambas instancias son consideradas
        /// iguales o equivalentes, <see langword="false"/> en caso 
        /// contrario.
        /// </returns>
        public bool Equals([AllowNull] ServiceResult other)
        {
            if (other is null) return false;

            return Success == other.Success && other.Reason == FailureReason.Unknown
                ? Message == other.Message
                : Reason == other.Reason;
        }

        /// <summary>
        /// Compara la igualdad entre esta intancia de la clase
        /// <see cref="ServiceResult"/> y un objeto de tipo
        /// <see cref="Exception"/>.
        /// </summary>
        /// <param name="other">
        /// Instancia contra la cual comparar esta instancia.
        /// </param>
        /// <returns>
        /// <see langword="true"/> si ambas instancias son consideradas
        /// iguales o equivalentes, <see langword="false"/> en caso 
        /// contrario.
        /// </returns>
        public bool Equals([AllowNull] Exception other)
        {
            return (int?)Reason == other?.HResult;
        }

        /// <summary>
        /// Convierte un resultado simple en uno de un tipo más específico.
        /// </summary>
        /// <typeparam name="TResult">
        /// Tipo de resultado a obtener. Los valores específicos del
        /// resultado tendrán su valor predeterminado.
        /// </typeparam>
        /// <returns>
        /// Un resultado de tipo <typeparamref name="TResult"/>.
        /// </returns>
        public TResult CastUp<TResult>() where TResult : ServiceResult, new()
        {
            return new TResult()
            {
                Success = Success,
                Reason = Reason,
                Message = Message
            };
        }

        /// <inheritdoc/>
        public override bool Equals([AllowNull] object obj)
        {
            return obj switch
            {
                ServiceResult s => Equals(s),
                Exception ex => Equals(ex),
                _ => false
            };
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Success, Message, Reason);
        }
    }

    /// <summary>
    /// Representa el resultado de una operación de servicio que incluye un
    /// valor devuelto.
    /// </summary>
    /// <typeparam name="T">
    /// Tipo de resultado a devolver.
    /// </typeparam>
    public class ServiceResult<T> : ServiceResult
    {
        /// <summary>
        /// Obtiene el valor a devolver como parte del resultado de la
        /// operación de servicio.
        /// </summary>
        public T ReturnValue { get; } = default!;

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>.
        /// </summary>
        public ServiceResult() : base()
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>, indicando un mensaje de estado 
        /// personalizado a mostrar.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public ServiceResult(string message): base(message)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>.
        /// </summary>
        /// <param name="returnValue">
        /// Objeto relevante retornado por la función.
        /// </param>
        public ServiceResult(T returnValue) : base()
        {
            ReturnValue = returnValue;
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>, indicando un mensaje de estado 
        /// personalizado a mostrar.
        /// </summary>
        /// <param name="returnValue">
        /// Objeto relevante retornado por la función.
        /// </param>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public ServiceResult(T returnValue, string message) : base(message)
        {
            ReturnValue = returnValue;
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>, especificando el motivo por el 
        /// cual la operación ha fallado.
        /// </summary>
        /// <param name="reason">
        /// Motivo por el cual la operación ha fallado.
        /// </param>
        public ServiceResult(in FailureReason reason) : base(reason)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/>, especificando el motivo por el 
        /// cual la operación ha fallado, además de un mensaje descriptivo
        /// del resultado.
        /// </summary>
        /// <param name="reason">
        /// Motivo por el cual la operación ha fallado.
        /// </param>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public ServiceResult(in FailureReason reason, string message) : base(reason, message)
        {
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="ServiceResult"/> para una operación fallida,
        /// extrayendo la información relevante a partir de la excepción
        /// especificada.
        /// </summary>
        /// <param name="ex">
        /// Excepción desde la cual obtener el mensaje y un código de error
        /// asociado.
        /// </param>
        public ServiceResult(Exception ex) : base(ex)
        {
        }

        /// <summary>
        /// Convierte implícitamente un valor <typeparamref name="T"/> en
        /// un <see cref="ServiceResult{T}"/>.
        /// </summary>
        /// <param name="result">
        /// Resultado de la operación.
        /// </param>
        public static implicit operator T(ServiceResult<T> result) => result.ReturnValue;

        /// <summary>
        /// Convierte implícitamente un <see cref="Exception"/> en un
        /// <see cref="ServiceResult{T}"/>.
        /// </summary>
        /// <param name="ex">
        /// Excepción desde la cual obtener el mensaje y un código de error
        /// asociado.
        /// </param>
        public static implicit operator ServiceResult<T>(Exception ex) => new(ex);

        /// <summary>
        /// Convierte implícitamente un <see cref="string"/> en un
        /// <see cref="ServiceResult{T}"/>.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del resultado.</param>
        public static implicit operator ServiceResult<T>(string message) => new(message);

        /// <summary>
        /// Convierte implícitamente un <see cref="FailureReason"/> en un
        /// <see cref="ServiceResult{T}"/>.
        /// </summary>
        /// <param name="reason">
        /// Motivo por el cual la operación ha fallado.
        /// </param>
        public static implicit operator ServiceResult<T>(in FailureReason reason) => new(reason);
    }
}