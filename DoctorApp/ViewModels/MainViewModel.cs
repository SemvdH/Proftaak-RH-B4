using DoctorApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using Util;

namespace DoctorApp.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public ObservableCollection<ClientInfoViewModel> Tabs { get; set; }
        public int Selected { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }

        Client client;

        public MainViewModel(MainWindowViewModel mainWindowViewModel)
        {
            this.MainWindowViewModel = mainWindowViewModel;
            client = this.MainWindowViewModel.client;
            Tabs= new ObservableCollection<ClientInfoViewModel>();
        }

        public void NewConnectedUser(string username)
        {
            System.Diagnostics.Debug.WriteLine("CREATING TAB FOR " + username);
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Tabs.Add(new ClientInfoViewModel(MainWindowViewModel)
                {
                    Username = username,
                    Status = "Waiting to start"
                });
            });
        }

        public void DisconnectedUser(string username)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                foreach (ClientInfoViewModel item in Tabs)
                {
                    if (item.Username == username)
                    {
                        Tabs.Remove(item);
                        break;
                    }
                }
            });
        }

        public void TransferDataToClientBike(byte[] bytes)
        {
            string username = DataParser.getNameFromBytesBike(bytes);
            foreach(ClientInfoViewModel item in Tabs)
            {
                if(item.Username == username)
                {
                    item.BikeData(bytes);
                }
            }
        }

        public void TransferDataToClientBPM(byte[] bytes)
        {
            string username = DataParser.getNameFromBytesBPM(bytes);
            foreach (ClientInfoViewModel item in Tabs)
            {
                if (item.Username == username)
                {
                    item.BikeData(bytes);
                }
            }
        }
    }


}
