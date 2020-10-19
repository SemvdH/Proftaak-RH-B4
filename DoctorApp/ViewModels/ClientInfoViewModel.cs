using DoctorApp.Models;
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
        public PatientInfo PatientInfo { get; set; }
        public ObservableCollection<string> ChatLog { get; set; }

        public ICommand StartSession { get; set; }

        public ICommand StopSession { get; set; }

        public ICommand Chat { get; set; }

        public ICommand ChatToAll { get; set; }

        public ICommand ClientInfo { get; set; }

        public ICommand SetResistance { get; set; }

        public MainWindowViewModel MainWindowViewModel { get; set; }
        private Client client;

        public ClientInfoViewModel(MainWindowViewModel mainWindowViewModel, string username)
        {
            MainWindowViewModel = mainWindowViewModel;
            this.PatientInfo = new PatientInfo() { Username = username, Status = "Waiting to start" };
            PatientInfo.ChatLog = new ObservableCollection<string>();
            ChatLog = new ObservableCollection<string>();
            client = mainWindowViewModel.client;

            StartSession = new RelayCommand(() =>
            {
                client.sendMessage(DataParser.getStartSessionJson(PatientInfo.Username));
                PatientInfo.Status = "Session started";
                System.Diagnostics.Debug.WriteLine("patient info status" + PatientInfo.Status);
            });

            StopSession = new RelayCommand(() =>
            {
                client.sendMessage(DataParser.getStopSessionJson(PatientInfo.Username));
                PatientInfo.Status = "Session stopped, waiting to start again.";
                System.Diagnostics.Debug.WriteLine("patient info status" + PatientInfo.Status);
            });

            Chat = new RelayCommand<object>((parameter) =>
            {
                client.sendMessage(DataParser.getChatJson(PatientInfo.Username, ((TextBox)parameter).Text));
                PatientInfo.ChatLog.Add(DateTime.Now + ": " + ((TextBox)parameter).Text);
                ChatLog.Add(DateTime.Now + ":derp: " + ((TextBox)parameter).Text);
            });

            //TODO RelayCommand ChatToAll

            SetResistance = new RelayCommand<object>((parameter) =>
            {
                client.sendMessage(DataParser.getSetResistanceJson(PatientInfo.Username, float.Parse(((TextBox)parameter).Text)));
            });

        }


    }
}
