using System;
using System.Collections.Generic;
using TheXDS.MCART.Events;

namespace TheXDS.Triton.Ui.ViewModels
{
    /// <summary>
    /// Define una serie de miembros a implementar por un tipo que permita
    /// alojar objetos de tipo <see cref="PageViewModel"/> para ser 
    /// presentados de manera visual.
    /// </summary>
    public interface IHostViewModel
    {
        /// <summary>
        /// Enumera las páginas abiertas activas de esta instancia.
        /// </summary>
        IEnumerable<PageViewModel> Pages { get; }

        /// <summary>
        /// Se produce cuando se ha agregado una página a la colección de
        /// páginas de este host.
        /// </summary>
        event EventHandler<ValueEventArgs<PageViewModel>> PageAdded;

        /// <summary>
        /// Se produce cuando se ha cerrado una página en la colección de
        /// páginas de este host.
        /// </summary>
        event EventHandler<ValueEventArgs<PageViewModel>> PageClosed;

        /// <summary>
        /// Agrega una página a esta instancia.
        /// </summary>
        /// <param name="page">
        /// Página a agregar.
        /// </param>
        void AddPage(PageViewModel page);

        /// <summary>
        /// Cierra una página activa en esta instancia.
        /// </summary>
        /// <param name="page">
        /// Página a cerrar.
        /// </param>
        void ClosePage(PageViewModel page);
    }
}