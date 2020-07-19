﻿using System;
using System.Collections.Generic;
using System.Text;
using TheXDS.Triton.Ui.ViewModels;
using TheXDS.Triton.CrudGen.Base;
using TheXDS.Triton.Models.Base;
using TheXDS.MCART.ViewModel;
using System.Linq.Expressions;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.CrudGen.ViewModels;

namespace TheXDS.Triton.CrudGen
{
    /// <summary>
    /// Define un delegado para un método que determina si la acción de Crud
    /// solicitada se puede realizar basada en la información de estado
    /// provista por el <see cref="CrudViewModel"/> activo.
    /// </summary>
    /// <param name="vmContext">
    /// Contexto en el cual se comprobará la posibilidad de ejecutar una acción
    /// Crud.
    /// </param>
    public delegate bool EntityCrudActionCheck(CrudViewModel vmContext);
    public delegate bool EntityCrudActionCheck<TModel>(TModel entity) where TModel : Model;
    
    
    public interface ICrudBuilder
    {
        PageViewModel Build();
    }

    /// <summary>
    /// Define una serie de miembros a implementar por un tipo que permita 
    /// establecer un contexto de datos para utilizar internamente.
    /// </summary>
    /// <remarks>
    /// Para C#9, esta interfaz es candidato a mudarse a la característica de
    /// "shapes".
    /// </remarks>
    public interface IDataContext
    {
        /// <summary>
        /// Obtiene o establece el contexto de datos utilizado por esta
        /// instancia.
        /// </summary>
        object? DataContext { get; set; }
    }

    /// <summary>
    /// Define una serie de miembros a implementar por un tipo que permita
    /// configurar la generación de una interfaz gráfica con capacidades Crud.
    /// </summary>
    /// <typeparam name="TModel">
    /// Tipo de modelo para el cual se generará un bloque de interfaz gráfica.
    /// </typeparam>
    public interface ICrudDescriptionBuilder<TModel> where TModel : Model
    {
        /// <summary>
        /// Inicia la descripción de una propiedad.
        /// </summary>
        /// <param name="selector">
        /// Selector de la propiedad a describir.
        /// </param>
        /// <typeparam name="TProperty">
        /// Tipo de la propiedad descrita.
        /// </typeparam>
        /// <returns>
        /// Un <see cref="IPropertyDescriptor{TModel,TProperty,TViewModel}"/>
        /// que permite configurar la presentación de la propiedad en el Crud
        /// generado.
        /// </returns>
        IPropertyDescriptor<TModel, TProperty, ViewModel<TModel>> Property<TProperty>(Expression<Func<TModel, TProperty>> selector);
        
        /// <summary>
        /// Agrega un bloque personalizado de UI en el Crud generado,
        /// estableciendo el orígen de contexto de datos a utilizar dentro del
        /// mismo.
        /// </summary>
        /// <param name="dataContext">
        /// Expresión que permite seleccionar un contexto de datos a partir de
        /// la entidad.
        /// </param>
        /// <typeparam name="TUi">
        /// Tipo de elemento visual a colocar. Debe ser instanciable, e
        /// implementar la interfaz <see cref="IDataContext"/>.
        /// </typeparam>
        void CustomBlock<TUi>(Func<TModel, object> dataContext) where TUi : IDataContext, new();
    }

    public abstract class CrudBuilder<TModel> : ICrudBuilder where TModel : Model
    {
        public PageViewModel Build()
        {
            
        }

        /// <summary>
        /// Al invalidarse, provee de objetos descriptores a partir de los
        /// cuales se crearán un editor y un visor para el modelo descrito.
        /// </summary>
        /// <param name="editor">
        /// Objeto que permite describir la construcción de un editor Crud.
        /// </param>
        /// <param name="viewer">
        /// Objeto que permite describir la presentación de un modelo en un
        /// visor Crud.
        /// </param>
        protected abstract void Describe(ICrudDescriptionBuilder<TModel> editor, ICrudDescriptionBuilder<TModel> viewer);

        /// <summary>
        /// Obtiene el nombre amigable con el cual identificar al modelo
        /// descrito.
        /// </summary>
        public string FriendlyName { get; }
        
        /// <summary>
        /// Obtiene un método que determina si la descripción del modelo
        /// permite crear una nueva entidad.
        /// </summary>
        public EntityCrudActionCheck? CanCreate { get; }
        
        /// <summary>
        /// Obtiene un método que determina si la descripción del modelo
        /// permite editar una entidad.
        /// </summary>
        public EntityCrudActionCheck<TModel>? CanEdit { get; }
        
        /// <summary>
        /// Obtiene un método que determina si la descripción del modelo
        /// permite eliminar una entidad.
        /// </summary>
        public EntityCrudActionCheck<TModel>? CanDelete { get; }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="CrudBuilder{TModel}"/>, estableciendo todos los valores
        /// descriptivos simples a sus valores predeterminados.
        /// </summary>
        protected CrudBuilder()
        {
            FriendlyName = typeof(TModel).NameOf();            
        }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="CrudBuilder{TModel}"/>, estableciendo el nombre amigable
        /// a asociar al modelo descrito.
        /// </summary>
        /// <param name="friendlyName">Nombre amigable del modelo.</param>
        protected CrudBuilder(string friendlyName)
        {
            FriendlyName = friendlyName ?? throw new ArgumentNullException(nameof(friendlyName));
        }
    }
}
