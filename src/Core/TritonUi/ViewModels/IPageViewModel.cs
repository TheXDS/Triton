using System.Windows.Input;
using TheXDS.MCART.Types;

namespace TheXDS.Triton.Ui.ViewModels
{
    /// <summary>
    /// Define una serie de miembros a implementar por un tipo que represente a
    /// una página interactiva representable visualmente.
    /// </summary>
    public interface IPageViewModel
    {
        /// <summary>
        /// Obtiene o establece un color decorativo a utilizar para la página.
        /// </summary>
        /// <value>El color decorativo a utilizar.</value>
        Color? AccentColor { get; }

        /// <summary>
        /// Obtiene o establece un valor que indica si esta página puede ser
        /// cerrada.
        /// </summary>
        /// <value>
        /// <see langword="true"/> para indicar que la página puede ser
        /// cerrada, <see langword="false"/> en caso contrario.
        /// </value>
        bool Closeable { get; }

        /// <summary>
        /// Obtiene el comando a ejecutar para cerrar esta página.
        /// </summary>
        ICommand CloseCommand { get; }

        /// <summary>
        /// Obtiene o establece el título de la página.
        /// </summary>
        /// <value>El título de la página.</value>
        string Title { get; }
    }
}