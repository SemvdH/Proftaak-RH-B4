using DoctorApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using Util;

namespace DoctorApp.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public ObservableCollection<object> Tabs { get; set; }
        public int Selected { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }

        Client client;

        public MainViewModel(MainWindowViewModel mainWindowViewModel)
        {
            this.MainWindowViewModel = mainWindowViewModel;
            client = this.MainWindowViewModel.client;
            Tabs = new ObservableCollection<object>();
        }

        public void NewConnectedUser(string username)
        {
            System.Diagnostics.Debug.WriteLine("CREATING TAB FOR " + username);
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Tabs.Add(new ClientInfoViewModel
                {
                    Username = username,
                    TabName = username
                });
            });
        }

        public void DisconnectedUser(string username)
        {

        }
    }


}
