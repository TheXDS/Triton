using System;
using System.Windows.Controls;
using TheXDS.MCART.Types.Extensions;
using TheXDS.Triton.Pages;
using TheXDS.Triton.Ui.Component;
using TheXDS.Triton.Ui.ViewModels;
using Triton.Workstation.Client.Pages;
using TheXDS.MCART.UI;
using TheXDS.MCART.ViewModel;
using TheXDS.MCART.UI.Base;
using System.Collections.Generic;
using TheXDS.MCART.Events;
using System.Linq;
using TheXDS.MCART.Types;
using System.Windows.Input;
using System.Windows;
using TheXDS.MCART;
using TheXDS.MCART.Dialogs;
using TheXDS.MCART.Component;
using System.Reflection;

namespace TheXDS.Triton.WpfClient.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IHostViewModel, IPageViewModel, IMenuViewModel
    {
        private static Assembly GetEntryAssembly()
        {
            return ReflectionHelpers.GetEntryPoint()?.DeclaringType?.Assembly 
                ?? Assembly.GetEntryAssembly()
                ?? Assembly.GetExecutingAssembly();
        }

        private readonly IVisualBuilder<TabHost> _visualBuilder = CreateBuilder();
        private readonly HostViewModel _host = new HostViewModel();
        private bool _MenuVisible;

        /// <inheritdoc/>
        public IEnumerable<PageViewModel> Pages => ((IHostViewModel)_host).Pages;

        /// <summary>
        /// Enumera los contenedores visuales de los
        /// <see cref="PageViewModel"/> abiertos dentro de esta instancia.
        /// </summary>
        public IEnumerable<TabHost> Children => Pages.Select(_visualBuilder.Build);

        /// <inheritdoc/>
        public Color? AccentColor => null;

        /// <inheritdoc/>
        public bool Closeable => true;

        /// <inheritdoc/>
        public ICommand CloseCommand { get; }

        /// <inheritdoc/>
        public string Title { get; }

        /// <inheritdoc/>
        public IEnumerable<InteractionBase> Menu 
        { 
            get
            {
                yield return new Launcher("Abrir página de prueba", () => AddPage(new TestViewModel()));
                yield return new Launcher("Acerca de...", () => AboutBox.ShowDialog(Application.Current));
            }
        }

        /// <inheritdoc/>
        public bool MenuVisible
        {
            get => _MenuVisible;
            set => Change(ref _MenuVisible, value);
        }






        public MainWindowViewModel()
        {
            CloseCommand = new SimpleCommand(Application.Current.Shutdown);
            Title = new AssemblyInfo(GetEntryAssembly()).Name ?? string.Empty;
            _host.ForwardChange(this);
            RegisterPropertyChangeTrigger(nameof(Children), nameof(_host.Pages));
        }


        /// <inheritdoc/>
        public event EventHandler<ValueEventArgs<PageViewModel>> PageAdded
        {
            add
            {
                ((IHostViewModel)_host).PageAdded += value;
            }

            remove
            {
                ((IHostViewModel)_host).PageAdded -= value;
            }
        }

        /// <inheritdoc/>
        public event EventHandler<ValueEventArgs<PageViewModel>> PageClosed
        {
            add
            {
                ((IHostViewModel)_host).PageClosed += value;
            }

            remove
            {
                ((IHostViewModel)_host).PageClosed -= value;
            }
        }

        /// <inheritdoc/>
        public void AddPage(PageViewModel page)
        {
            ((IHostViewModel)_host).AddPage(page);
            //Notify(nameof(Children));
        }

        /// <inheritdoc/>
        public void ClosePage(PageViewModel page)
        {
            ((IHostViewModel)_host).ClosePage(page);
            //Notify(nameof(Children));
        }

        private static IVisualBuilder<TabHost> CreateBuilder()
        {
            var r = new DictionaryVisualResolver<Page>();
            r.RegisterVisual<TestViewModel, TestPage>();
            return new TabBuilder(new TestFvr(r));
        }
    }

    public class TestFvr : FallbackVisualResolver<Page>
    {
        public TestFvr(IVisualResolver<Page> resolver) : base(resolver)
        {
        }

        /// <inheritdoc/>
        protected override Page FallbackResolve(PageViewModel viewModel, Exception ex)
        {
            return new FallbackErrorPage { Message = $"{ex.GetType().Name}{ex.Message.OrNull(": {0}")}" };
        }
    }
}
