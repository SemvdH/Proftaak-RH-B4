using DoctorApp.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoctorApp.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public MainWindowViewModel MainWindowViewModel { get; set; }

        Client client;

        public MainViewModel(MainWindowViewModel mainWindowViewModel)
        {
            this.MainWindowViewModel = mainWindowViewModel;
            client = this.MainWindowViewModel.client;
        }
    }
}
