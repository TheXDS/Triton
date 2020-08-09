using TheXDS.Triton.Models.Base;
using TheXDS.MCART.Types.Extensions;

namespace TheXDS.Triton.CrudGen
{
    /// <summary>
    /// Define una serie de miembros a implementar por un tipo que exponga
    /// valores de configuración a utilizar para generar vistas de Crud.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public interface ICrudBuilderSettings<TModel> where TModel : Model
    {
        /// <summary>
        /// Obtiene el nombre amigable con el cual identificar al modelo
        /// descrito.
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        /// Obtiene un método que determina si la descripción del modelo
        /// permite crear una nueva entidad.
        /// </summary>
        /// <value>
        /// Un delegado que comprobará si es posible crear una nueva entidad, o
        /// <see langword="null"/> para permitir crear entidades de forma 
        /// predeterminada.
        /// </value>
        EntityCrudActionCheck? CanCreate { get; }
        
        /// <summary>
        /// Obtiene un método que determina si la descripción del modelo
        /// permite editar una entidad.
        /// </summary>
        EntityCrudActionCheck<TModel>? CanEdit { get; }
        
        /// <summary>
        /// Obtiene un método que determina si la descripción del modelo
        /// permite eliminar una entidad.
        /// </summary>
        EntityCrudActionCheck<TModel>? CanDelete { get; }

        /// <summary>
        /// Obtiene la cantidad de elementos a cargar por página del Widget
        /// utilizado para explorar y seleccionar entidades.
        /// </summary>
        /// <value>
        /// Un <see cref="int"/> que indica la cantidad de elementos a mostrar
        /// por página, o <see langword="null"/> si no debe utilizarse un
        /// Pager.
        /// </value>
        int? PagerItems { get; }

        /// <summary>
        /// Obtiene un <see cref="ILayoutBuilder"/> alternativo a utilizar para
        /// generar la vista de Crud.
        /// </summary>
        /// <value>
        /// <see cref="ILayoutBuilder"/> a utilizar para generar la vista de
        /// Crud, o <see langword="null"/> para utilizar el
        /// <see cref="ILayoutBuilder"/> predeterminado de la aplicación.
        /// </value>
        ILayoutBuilder CustomLayout { get; }
    }
}
