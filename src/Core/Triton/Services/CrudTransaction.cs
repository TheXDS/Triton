﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TheXDS.Triton.Models.Base;
using TheXDS.Triton.Services.Base;

namespace TheXDS.Triton.Services
{
    /// <summary>
    ///     Obtiene una transacción compleja que permite operaciones de lectura
    ///     y de escritura sobre un contexto de datos común.
    /// </summary>
    /// <typeparam name="T">
    ///     Tipo de contexto de datos a utilizar.
    /// </typeparam>
    public class CrudTransaction<T> : CrudTransactionBase<T>, ICrudReadWriteTransaction, IAsyncCrudReadWriteTransaction where T : DbContext, new()
    {
        private readonly ICrudReadTransaction _readTransaction;
        private readonly ICrudWriteTransaction _writeTransaction;
        private readonly IAsyncCrudReadTransaction _asyncReadTransaction;
        private readonly IAsyncCrudWriteTransaction _asyncWriteTransaction;

        /// <summary>
        ///     Inicializa una nueva instancia de la clase 
        ///     <see cref="CrudTransaction{T}"/>.
        /// </summary>
        /// <param name="configuration">
        ///     Configuración a pasar a las transacciones subyacentes.
        /// </param>
        public CrudTransaction(IConnectionConfiguration configuration) : base(configuration)
        {
            _readTransaction = new CrudReadTransaction<T>(configuration, _context);
            _writeTransaction = new CrudWriteTransaction<T>(configuration, _context);
            _asyncReadTransaction = new CrudAsyncReadTransaction<T>(configuration, _context);
            _asyncWriteTransaction = new CrudAsyncWriteTransaction<T>(configuration, _context);
        }

        /// <summary>
        ///     Obtiene una entidad cuyo campo llave sea igual al valor
        ///     especificado.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la entidad a obtener.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     Tipo de campo llave de la entidad a obtener.
        /// </typeparam>
        /// <param name="key">
        ///     Llave de la entidad a obtener.
        /// </param>
        /// <param name="entity">
        ///     Parámetro de salida. Entidad obtenida en la operación de
        ///     lectura. Si no existe una entidad con el campo llave
        ///     especificado, se devolverá <see langword="null"/>.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public ServiceResult Read<TModel, TKey>(TKey key, out TModel? entity) where TModel : Model<TKey> where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            return _readTransaction.Read(key, out entity);
        }

        /// <summary>
        ///     Obtiene una entidad cuyo campo llave sea igual al valor
        ///     especificado.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la entidad a obtener.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     Tipo de campo llave de la entidad a obtener.
        /// </typeparam>
        /// <param name="key">
        ///     Llave de la entidad a obtener.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente, incluyendo como valor de resultado a la
        ///     entidad obtenida en la operación de lectura. Si no existe una
        ///     entidad con el campo llave especificado, el valor de resultado
        ///     será <see langword="null"/>.
        /// </returns>
        public ServiceResult<TModel?> Read<TModel, TKey>(TKey key)
            where TModel : Model<TKey>
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            return _readTransaction.Read<TModel,TKey>(key);
        }

        /// <summary>
        ///     Crea una nueva entidad en la base de datos.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la nueva entidad.
        /// </typeparam>
        /// <param name="newEntity">
        ///     Nueva entidad a agregar a la base de datos.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public ServiceResult Create<TModel>(TModel newEntity) where TModel : Model
        {
            return _writeTransaction.Create(newEntity);
        }

        /// <summary>
        ///     Elimina a una entidad de la base de datos.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la entidad a eliminar.
        /// </typeparam>
        /// <param name="entity">
        ///     Entidad que deberá ser eliminada de la base de datos.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public ServiceResult Delete<TModel>(TModel entity) where TModel : Model
        {
            return _writeTransaction.Delete(entity);
        }

        /// <summary>
        ///     Elimina a una entidad de la base de datos.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la entidad a eliminar.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     Tipo del campo llave que identifica a la entidad.
        /// </typeparam>
        /// <param name="key">
        ///     Llave de la entidad que deberá ser eliminada de la base de
        ///     datos.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public ServiceResult Delete<TModel, TKey>(TKey key)
            where TModel : Model<TKey>
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            return _writeTransaction.Delete<TModel,TKey>(key);
        }

        /// <summary>
        ///     Actualiza los datos contenidos en una entidad dentro de la base
        ///     de datos.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la entidad a actualizar.
        /// </typeparam>
        /// <param name="entity">
        ///     Entidad que contiene la nueva información a escribir.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public ServiceResult Update<TModel>(TModel entity) where TModel : Model
        {
            return _writeTransaction.Update(entity);
        }

        /// <summary>
        ///     Guarda todos los cambios síncronos pendientes de la transacción
        ///     actual.
        /// </summary>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public ServiceResult Commit()
        {
            return _writeTransaction.Commit();
        }

        /// <summary>
        ///     Crea una nueva entidad en la base de datos de forma asíncrona.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la nueva entidad.
        /// </typeparam>
        /// <param name="newEntity">
        ///     Nueva entidad a agregar a la base de datos.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public Task<ServiceResult> CreateAsync<TModel>(TModel newEntity) where TModel : Model
        {
            return _asyncWriteTransaction.CreateAsync(newEntity);
        }

        /// <summary>
        ///     Elimina a una entidad de la base de datos de forma asíncrona.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la entidad a eliminar.
        /// </typeparam>
        /// <param name="entity">
        ///     Entidad que deberá ser eliminada de la base de datos.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public Task<ServiceResult> DeleteAsync<TModel>(TModel entity) where TModel : Model
        {
            return _asyncWriteTransaction.DeleteAsync(entity);
        }

        /// <summary>
        ///     Elimina a una entidad de la base de datos de forma asíncrona.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la entidad a eliminar.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     Tipo del campo llave que identifica a la entidad.
        /// </typeparam>
        /// <param name="key">
        ///     Llave de la entidad que deberá ser eliminada de la base de
        ///     datos.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public Task<ServiceResult> DeleteAsync<TModel, TKey>(TKey key)
            where TModel : Model<TKey>
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            return _asyncWriteTransaction.DeleteAsync<TModel,TKey>(key);
        }

        /// <summary>
        ///     Actualiza de forma asíncrona los datos contenidos en una
        ///     entidad dentro de la base de datos.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la entidad a actualizar.
        /// </typeparam>
        /// <param name="entity">
        ///     Entidad que contiene la nueva información a escribir.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public Task<ServiceResult> UpdateAsync<TModel>(TModel entity) where TModel : Model
        {
            return _asyncWriteTransaction.UpdateAsync(entity);
        }

        /// <summary>
        ///     Obtiene de forma asíncrona una entidad cuyo campo llave sea
        ///     igual al valor especificado.
        /// </summary>
        /// <typeparam name="TModel">
        ///     Modelo de la entidad a obtener.
        /// </typeparam>
        /// <typeparam name="TKey">
        ///     Tipo de campo llave de la entidad a obtener.
        /// </typeparam>
        /// <param name="key">
        ///     Llave de la entidad a obtener.
        /// </param>
        /// <returns>
        ///     El resultado reportado de la operación ejecutada por el
        ///     servicio subyacente.
        /// </returns>
        public Task<ServiceResult<TModel?>> ReadAsync<TModel, TKey>(TKey key)
            where TModel : Model<TKey>
            where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            return _asyncReadTransaction.ReadAsync<TModel, TKey>(key);
        }
    }
}