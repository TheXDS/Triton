﻿using TheXDS.Triton.Ui.ViewModels;
using System.Windows.Input;
using TheXDS.MCART.ViewModel;

namespace TheXDS.Triton.Ui.Component
{
    /// <summary>
    /// ViewModel de prueba de generación de UI.
    /// </summary>
    public class TestViewModel : PageViewModel
    {
        private static int _count;
        private string _name = "usuario";
        private int _numberOne;
        private int _numberTwo;
        private int _result;

        /// <summary>
        /// Inicializa una nueva instancia de la clase
        /// <see cref="TestViewModel"/>.
        /// </summary>
        public TestViewModel()
        {
            _count++;
            Title = $"Prueba # {_count}";
            AccentColor = MCART.Resources.Colors.Pick();
            SumCommand = new SimpleCommand(OnSum);
            OkTkxByeCommand = new SimpleCommand(OnOkTkxBye);
        }

        /// <summary>
        /// Obtiene o establece el valor Name.
        /// </summary>
        /// <value>El valor de Name.</value>
        public string Name
        {
            get => _name;
            set => Change(ref _name, value);
        }

        /// <summary>
        /// Obtiene o establece el valor NumberOne.
        /// </summary>
        /// <value>El valor de NumberOne.</value>
        public int NumberOne
        {
            get => _numberOne;
            set => Change(ref _numberOne, value);
        }

        /// <summary>
        /// Obtiene o establece el valor NumberTwo.
        /// </summary>
        /// <value>El valor de NumberTwo.</value>
        public int NumberTwo
        {
            get => _numberTwo;
            set => Change(ref _numberTwo, value);
        }

        /// <summary>
        /// Obtiene o establece el valor Result.
        /// </summary>
        /// <value>El valor de Result.</value>
        public int Result
        {
            get => _result;
            private set => Change(ref _result, value);
        }

        /// <summary>
        /// Obtiene el comando relacionado a la acción Sum.
        /// </summary>
        /// <returns>El comando Sum.</returns>
        public ICommand SumCommand { get; }

        private void OnSum()
        {
            Result = NumberOne + NumberTwo;
        }

        /// <summary>
        /// Okay, Thanks, Bye.
        /// </summary>
        public ICommand OkTkxByeCommand { get; } 

        private void OnOkTkxBye()
        {
            Close();
        }
    }
}