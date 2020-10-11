using System.Collections.Generic;
using TheXDS.Triton.Ui.Resources;
using TheXDS.Triton.Ui.ViewModels;

namespace TheXDS.Triton.Ui.Component
{
    /// <summary>
    /// Colección de <see cref="IVisualResolver{T}"/> que permite resolver un
    /// <see cref="PageViewModel"/> utilizando cualquier elemento registrado.
    /// </summary>
    /// <typeparam name="TVisual">
    /// Tipo de contenedor visual a implementar.
    /// </typeparam>
    public class VisualResolverSet<TVisual> : List<IVisualResolver<TVisual>>, IVisualResolver<TVisual> where TVisual : notnull
    {
        /// <inheritdoc/>
        public TVisual ResolveVisual(PageViewModel viewModel)
        {
            foreach(var j in this)
            {
                try
                {
                    if (j.ResolveVisual(viewModel) is { } v) return v;
                }
                catch { }
            }
            throw Errors.UnresolvableViewModel(viewModel);
        }
    }
}