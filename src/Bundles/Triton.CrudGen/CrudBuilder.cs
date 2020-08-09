using System;
using System.Collections.Generic;
using System.Text;
using TheXDS.Triton.Ui.ViewModels;
using TheXDS.Triton.CrudGen.Base;
using TheXDS.Triton.Models.Base;
using TheXDS.MCART.ViewModel;
using System.Linq.Expressions;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.CrudGen.ViewModels;
using TheXDS.Triton.Ui.Component;

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
    /// <returns>
    /// <see langword="true"/> si la acción Crud puede ejecutarse en el estado
    /// actual, <see langword="false"/> en caso contrario.
    /// </returns>
    public delegate bool EntityCrudActionCheck(CrudViewModel vmContext);

    /// <summary>
    /// Define un delegado para un método que determina si la acción de Crud
    /// solicitada se puede realizar basada en la información de estado
    /// provista por el <typeparamref name="TModel"/> activo.
    /// </summary>
    /// <typeparam name="TModel">
    /// Tipo de modelo que está siendo editado en un
    /// <see cref="CrudViewModel"/> activo.
    /// </typeparam>
    /// <param name="entity">
    /// Entidad que está siendo editada en el <see cref="CrudViewModel"/>
    /// activo.
    /// </param>
    /// <returns>
    /// <see langword="true"/> si la acción Crud puede ejecutarse en el estado
    /// actual, <see langword="false"/> en caso contrario.
    /// </returns>
    public delegate bool EntityCrudActionCheck<TModel>(TModel entity) where TModel : Model;
    
    /// <summary>
    /// Define una serie de miembros a implementar por un tipo que permita
    /// construir un <see cref="PageViewModel"/> utilizando un conjunto de
    /// propiedades configuradas.
    /// </summary>
    public interface ICrudBuilder
    {
        /// <summary>
        /// Construye un <see cref="PageViewModel"/>.
        /// </summary>
        /// <returns></returns>
        PageViewModel Build();
    }

    public abstract class CrudBuilder<TModel> : ICrudBuilder where TModel : Model
    {
        public ICrudBuilderSettings<TModel> Settings { get; }

        /// <summary>
        /// Esctructura que contiene información de configuración que el
        /// constructor de Crud utilizará al generar páginas.
        /// </summary>
        protected class CrudBuilderSettings : ICrudBuilderSettings<TModel>
        {
            public string FriendlyName { get; set; } = typeof(TModel).NameOf();

            public EntityCrudActionCheck? CanCreate { get; set; }

            public EntityCrudActionCheck<TModel>? CanEdit { get; set; }

            public EntityCrudActionCheck<TModel>? CanDelete { get; set; }

            public int? PagerItems { get; set; }

            public ILayoutBuilder? CustomLayout { get; set; }
        }

        public PageViewModel Build()
        {
            return null;
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
        /// Inicializa una nueva instancia de la clase
        /// <see cref="CrudBuilder{TModel}"/>, estableciendo toda la
        /// configuración del constructor de Crud a sus valores
        /// predeterminados.
        /// </summary>
        protected CrudBuilder() : this(new CrudBuilderSettings { FriendlyName = typeof(TModel).NameOf() }) { }

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="CrudBuilder{TModel}"/>, estableciendo la configuración
        /// del constructor de Crud a utilizar.
        /// </summary>
        /// <param name="settings">Configuración de constructor de Crud a utilizar.</param>
        protected CrudBuilder(CrudBuilderSettings settings)
        {
            Settings = settings;
        }
    }
}
