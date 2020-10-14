using ClientApp.Models;
using ClientApp.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ClientApp.ViewModels
{
    class MainWindowViewModel : ObservableObject
    {
        public Info InfoModel { get; set; }

        public ObservableObject SelectedViewModel { get; set; }
        public Client client { get; }

        LoginViewModel loginViewModel;

        public MainWindowViewModel(Client client)
        {
            this.InfoModel = new Info();
            this.client = client;
            loginViewModel = new LoginViewModel(this);
            SelectedViewModel = loginViewModel;
            this.client.SetLoginViewModel(loginViewModel);
            App.Current.MainWindow.Closing += new CancelEventHandler(MainWindow_Closing);
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            this.client.sendMessage(DataParser.getDisconnectJson(loginViewModel.Username));

        }

    }
}
