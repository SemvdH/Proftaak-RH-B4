using ClientApp.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientApp.ViewModels
{
    class MainViewModel : ObservableObject
    {

        public ObservableObject SelectedViewModel { get; set; }

        public MainViewModel()
        {
            SelectedViewModel = new LoginViewModel();
        }
    }
}
