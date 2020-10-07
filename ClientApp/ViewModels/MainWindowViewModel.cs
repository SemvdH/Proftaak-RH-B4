using ClientApp.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientApp.ViewModels
{
    class MainWindowViewModel : ObservableObject
    {

        public ObservableObject SelectedViewModel { get; set; }

        public MainWindowViewModel()
        {
            SelectedViewModel = new LoginViewModel(this);
        }
    }
}
