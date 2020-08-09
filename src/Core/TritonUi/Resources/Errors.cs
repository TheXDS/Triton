using System;
using System.Collections.Generic;
using System.Text;
using TheXDS.Triton.Ui.Exceptions;
using TheXDS.Triton.Ui.ViewModels;
using St = TheXDS.Triton.Ui.Resources.ErrorStrings;

namespace TheXDS.Triton.Ui.Resources
{
    internal static class Errors
    {
        public static Exception UnresolvableViewModel(PageViewModel vm) => new UnresolvableViewModelException(string.Format(St.UnresolvableViewModel,vm.GetType().Name));
    }
}
