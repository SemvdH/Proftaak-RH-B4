using DoctorApp.Utils;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Util;

namespace DoctorApp.ViewModels
{
    
    class ClientInfoViewModel
    {
        public string Username { get; set; }
        public string Status { get; set; }

        public ICommand StartSession { get; set; }

        public ICommand StopSession { get; set; }

        public ICommand Chat { get; set; }

        public ICommand ChatToAll { get; set; }

        public ICommand ClientInfo { get; set; }

        public ICommand SetResistance { get; set; }

        public MainWindowViewModel MainWindowViewModel { get; set; }
        private Client client;



        public ClientInfoViewModel(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
            client = mainWindowViewModel.client;

            StartSession = new RelayCommand(()=>{
                client.sendMessage(DataParser.getStartSessionJson());
            });

            StopSession = new RelayCommand(() => {
                client.sendMessage(DataParser.getStopSessionJson());
            });
            Chat = new RelayCommand<object>((parameter) =>
            {
                /*client.sendMessage(DataParser.)*/
            });

        }


    }
}
