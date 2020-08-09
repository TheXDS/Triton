using System;
using TheXDS.Triton.CrudGen.Base;
using TheXDS.Triton.Models.Base;
using TheXDS.MCART.ViewModel;
using System.Linq.Expressions;
using TheXDS.Triton.Ui.Component;

namespace TheXDS.Triton.CrudGen
{
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
}
