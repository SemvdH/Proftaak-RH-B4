using DoctorApp.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace DoctorApp.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public MainWindowViewModel MainWindowViewModel { get; set; }

        public MainViewModel(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
        }
    }
}
