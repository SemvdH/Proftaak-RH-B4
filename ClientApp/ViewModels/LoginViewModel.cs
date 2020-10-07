using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using ClientApp.Utils;
using GalaSoft.MvvmLight.Command;

namespace ClientApp.ViewModels
{
    class LoginViewModel : ObservableObject
    {
        public string Username { get; set; }
        //private MainWindowViewModel mainWindowViewModel;
        public LoginViewModel(MainWindowViewModel mainWindowViewModel)
        {
            //this.mainWindowViewModel = mainWindowViewModel;
            LoginCommand = new RelayCommand<object>((parameter) =>
            {
                Debug.WriteLine($"username {Username} password {((PasswordBox)parameter).Password}");
                //TODO send username and password to server
                mainWindowViewModel.SelectedViewModel = new MainViewModel();
            });
        }

        public ICommand LoginCommand { get; set; }
    }
}
