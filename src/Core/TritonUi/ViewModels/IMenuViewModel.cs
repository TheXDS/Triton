using System.Collections.Generic;
using TheXDS.MCART.UI.Base;

namespace TheXDS.Triton.Ui.ViewModels
{
    /// <summary>
    /// Define una serie de miembros a implementar por un tipo que exponga un
    /// menú de opciones presentables de manera visual.
    /// </summary>
    public interface IMenuViewModel
    {
        /// <summary>
        /// Enumera las opciones de menú disponibles en este
        /// <see cref="IMenuViewModel"/>.
        /// </summary>
        IEnumerable<InteractionBase> Menu { get; }

        /// <summary>
        /// Obtiene o establece un valor que indica si el menú debe o no ser
        /// visible.
        /// </summary>
        /// <value>
        /// <see langword="true"/> si el menú debe ser visible,
        /// <see langword="false"/> en caso contrario.
        /// </value>
        bool MenuVisible { get; set; }
    }
}