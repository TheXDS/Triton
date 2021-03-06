﻿using Microsoft.EntityFrameworkCore;
using TheXDS.Triton.Services.Base;

namespace TheXDS.Triton.Services
{
    /// <summary>
    /// Fábrica de transacciones que permite conectarse a un contexto de datos
    /// de Entity Framework Core.
    /// </summary>
    /// <typeparam name="T">
    /// Tipo de contexto de datos al cual conectarse.
    /// </typeparam>
    public class EfCoreTransFactory<T> : ITransactionFactory where T : DbContext, new()
    {
        /// <summary>
        /// Construye un nuevo servicio simple utilizando un 
        /// <see cref="EfCoreTransFactory{T}"/> para conectarse a un contexto
        /// de datos.
        /// </summary>
        /// <param name="configuration">
        /// Configuración de transacciones a aplicar al nuevo servicio.
        /// </param>
        /// <returns>
        /// Un nuevo servicio con la configuración especificada, utilizando un
        /// <see cref="EfCoreTransFactory{T}"/> para conectarse a un contexto
        /// de datos.
        /// </returns>
        public static IService BuildService(TransactionConfiguration configuration)
        {
            return new Service(configuration, new EfCoreTransFactory<T>());
        }

        /// <summary>
        /// Construye un nuevo servicio simple utilizando un 
        /// <see cref="EfCoreTransFactory{T}"/> para conectarse a un contexto
        /// de datos.
        /// </summary>
        /// <returns>
        /// Un nuevo servicio con una configuración de transacciones
        /// predetermianda y utilizando un <see cref="EfCoreTransFactory{T}"/>
        /// para conectarse a un contexto de datos.
        /// </returns>
        public static IService BuildService()
        {
            return new Service(new EfCoreTransFactory<T>());
        }

        /// <summary>
        /// Obtiene una transacción que permite leer información desde el
        /// contexto de datos.
        /// </summary>
        /// <param name="configuration">
        /// Confiuración de transacción a utilizar.
        /// </param>
        /// <returns>
        /// Una transacción que permite leer información desde el contexto de
        /// datos.
        /// </returns>
        public ICrudReadTransaction GetReadTransaction(TransactionConfiguration configuration)
        {
            return new CrudReadTransaction<T>(configuration);
        }

        /// <summary>
        /// Obtiene una transacción que permite escribir información desde el
        /// contexto de datos.
        /// </summary>
        /// <param name="configuration">
        /// Confiuración de transacción a utilizar.
        /// </param>
        /// <returns>
        /// Una transacción que permite escribir información desde el contexto
        /// de datos.
        /// </returns>
        public ICrudWriteTransaction GetWriteTransaction(TransactionConfiguration configuration)
        {
            return new CrudWriteTransaction<T>(configuration);
        }

        /// <summary>
        /// Obtiene una transacción que permite leer y escribir información
        /// desde el contexto de datos.
        /// </summary>
        /// <param name="configuration">
        /// Confiuración de transacción a utilizar.
        /// </param>
        /// <returns>
        /// Una transacción que permite leer y escribir información desde el 
        /// contexto de datos.
        /// </returns>
        public ICrudReadWriteTransaction GetTransaction(TransactionConfiguration configuration)
        {
            return new CrudTransaction<T>(configuration);
        }
    }
}