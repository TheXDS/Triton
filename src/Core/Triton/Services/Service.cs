﻿using Microsoft.EntityFrameworkCore;
using System;
using TheXDS.MCART;
using TheXDS.MCART.Exceptions;
using TheXDS.Triton.Services.Base;

namespace TheXDS.Triton.Services
{
    /// <summary>
    ///     Clase base para todos los servicios. Provee de la funcionalidad
    ///     básica de instanciación de contexto de datos y provee acceso a la 
    ///     configuración del servicio.
    /// </summary>
    /// <typeparam name="TContext">
    ///     Tipo de contexto de datos a instanciar.
    /// </typeparam>
    public abstract class Service<TContext> : IService where TContext : DbContext, new()
    {
        /// <summary>
        ///     Obtiene una referencia a la configuración activa para este servicio.
        /// </summary>
        public IServiceConfiguration Configuration { get; }

        /// <summary>
        ///     Inicializa una nueva instancia de la clase 
        ///     <see cref="ServiceBase{TContext}"/>,
        ///     buscando automáticamente la configuración a utilizar.
        /// </summary>
        protected Service() : this(Objects.FindFirstObject<IServiceConfiguration>() ?? throw new MissingTypeException(typeof(IServiceConfiguration)))
        {
        }

        /// <summary>
        ///     Inicializa una nueva instancia de la clase 
        ///     <see cref="ServiceBase{TContext}"/>,
        ///     especificando la configuración a utilizar.
        /// </summary>
        /// <param name="settings">
        ///     Configuración a utilizar para este servicio.
        /// </param>
        protected Service(IServiceConfiguration settings)
        {
            if (settings is null) throw new ArgumentNullException(nameof(settings));
            Configuration = settings;
        }

        /// <summary>
        ///     Obtiene una transacción para lectura de datos.
        /// </summary>
        /// <returns>
        ///     Una transacción para lectura de datos.
        /// </returns>
        public ICrudReadTransaction GetReadTransaction()
        {
            return Configuration.CrudTransactionFactory.ManufactureReadTransaction<TContext>(Configuration.TransactionConfiguration);
        }

        /// <summary>
        ///     Obtiene una transacción para escritura de datos.
        /// </summary>
        /// <returns>
        ///     Una transacción para escritura de datos.
        /// </returns>
        public ICrudWriteTransaction GetWriteTransaction()
        {
            return Configuration.CrudTransactionFactory.ManufactureWriteTransaction<TContext>(Configuration.TransactionConfiguration);
        }

        /// <summary>
        ///     Obtiene una transacción para lectura y escritura de datos.
        /// </summary>
        /// <returns>
        ///     Una transacción para lectura y escritura de datos.
        /// </returns>
        public ICrudReadWriteTransaction<TContext> GetReadWriteTransaction()
        {
            return Configuration.CrudTransactionFactory.ManufactureReadWriteTransaction<TContext>(Configuration.TransactionConfiguration);
        }

        ICrudReadWriteTransaction IService.GetReadWriteTransaction() => GetReadWriteTransaction();
    }
}
