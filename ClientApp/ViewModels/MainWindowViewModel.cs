using ClientApp.Models;
using ClientApp.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientApp.ViewModels
{
    class MainWindowViewModel : ObservableObject
    {
        public Info InfoModel { get; set; }

        public ObservableObject SelectedViewModel { get; set; }
        public Client client { get; }

        public MainWindowViewModel(Client client)
        {
            this.InfoModel = new Info();
            this.client = client;
            LoginViewModel loginViewModel = new LoginViewModel(this);
            SelectedViewModel = loginViewModel;
            this.client.SetLoginViewModel(loginViewModel);
        }

    }
}
