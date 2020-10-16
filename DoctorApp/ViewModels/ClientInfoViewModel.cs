using DoctorApp.Utils;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Util;

namespace DoctorApp.ViewModels
{
    
    class ClientInfoViewModel : ObservableObject
    {
        public ObservableCollection<string> ChatLog { get; set; }
        public string Username { get; set; }

        public string Status { get; set; }

        public int Speed { get; set; }

        public int BPM { get; set; }

        public int Resistance { get; set; }
        public int Acc_Power { get; set; }

        public int Curr_Power { get; set; }

        public int Distance { get; set; }

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
            ChatLog = new ObservableCollection<string>();
            client = mainWindowViewModel.client;

            StartSession = new RelayCommand(()=>{
                client.sendMessage(DataParser.getStartSessionJson(Username));
                Status = "Session started";
            });

            StopSession = new RelayCommand(() => {
                client.sendMessage(DataParser.getStopSessionJson(Username));
                Status = "Session stopped, waiting to start again.";
            });

            Chat = new RelayCommand<object>((parameter) =>
            {
                client.sendMessage(DataParser.getChatJson(Username, ((TextBox)parameter).Text));
                ChatLog.Add(DateTime.Now+": "+ ((TextBox)parameter).Text);
            });

            //TODO RelayCommand ChatToAll

            ClientInfo = new RelayCommand(() =>
            {
                //TODO POPUP
            });

            SetResistance = new RelayCommand<object>((parameter) =>
            {
                client.sendMessage(DataParser.getSetResistanceJson(Username, float.Parse(((TextBox)parameter).Text)));
            });

        }


    }
}
