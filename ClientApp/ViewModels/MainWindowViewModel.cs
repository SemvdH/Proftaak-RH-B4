using ClientApp.Models;
using ClientApp.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientApp.ViewModels
{
    class MainWindowViewModel : ObservableObject
    {
        public Info infoModel { get; set; }

        public ObservableObject SelectedViewModel { get; set; }
        public Client client { get; }

        public MainWindowViewModel(Client client)
        {
            this.infoModel = new Info();
            this.client = client;
            LoginViewModel loginViewModel = new LoginViewModel(this);
            SelectedViewModel = loginViewModel;
            this.client.SetLoginViewModel(loginViewModel);
        }

    }
}
