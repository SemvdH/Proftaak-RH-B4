﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ClientApp.Utils;
using GalaSoft.MvvmLight.Command;
using Util;

namespace ClientApp.ViewModels
{
    class LoginViewModel : ObservableObject
    {
        public string Username { get; set; }
        public ICommand LoginCommand { get; set; }

        public bool LoginStatus { get; set; }

        public bool InvertedLoginStatus { get; set; }

        private MainWindowViewModel MainWindowViewModel;
        public LoginViewModel(MainWindowViewModel mainWindowViewModel)
        {
            this.MainWindowViewModel = mainWindowViewModel;
            LoginCommand = new RelayCommand<object>((parameter) =>
            {
                //TODO send username and password to server
                this.MainWindowViewModel.client.tryLogin(Username, ((PasswordBox)parameter).Password);
            });
        }



        internal void setLoginStatus(bool status)
        {
            this.MainWindowViewModel.InfoModel.ConnectedToServer = status;
            this.InvertedLoginStatus = !status;
            if (status)
            {
                this.MainWindowViewModel.SelectedViewModel = new MainViewModel(MainWindowViewModel);
            }
        }
    }
}
