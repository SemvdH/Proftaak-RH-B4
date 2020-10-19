using DoctorApp.Models;
using DoctorApp.Utils;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Util;

namespace DoctorApp.ViewModels
{

    class ClientInfoViewModel : ObservableObject
    {
        public PatientInfo PatientInfo { get; set; }

        private string _mySelectedItem;
        public string MySelectedItem
        {
            get { return _mySelectedItem; }
            set
            {
                Chart.Clear();
                _mySelectedItem = value;
            }
        }

        public ICommand StartSession { get; set; }

        public ICommand StopSession { get; set; }

        public ICommand Chat { get; set; }

        public ICommand ChatToAll { get; set; }

        public ICommand ClientInfo { get; set; }

        public ICommand SetResistance { get; set; }

        public MainWindowViewModel MainWindowViewModel { get; set; }
        private Client client;

        public Chart Chart { get; set; }

        public ClientInfoViewModel(MainWindowViewModel mainWindowViewModel, string username)
        {
            MainWindowViewModel = mainWindowViewModel;
            this.PatientInfo = new PatientInfo() { Username = username, Status = "Waiting to start" };
            this.Chart = new Chart(this.PatientInfo);
            PatientInfo.ChatLog = new ObservableCollection<string>();
            client = mainWindowViewModel.client;

            StartSession = new RelayCommand(() =>
            {
                client.sendMessage(DataParser.getStartSessionJson(PatientInfo.Username));
                PatientInfo.Status = "Session started";
            });

            StopSession = new RelayCommand(() =>
            {
                client.sendMessage(DataParser.getStopSessionJson(PatientInfo.Username));
                PatientInfo.Status = "Session stopped, waiting to start again.";
            });

            Chat = new RelayCommand<object>((parameter) =>
            {
                client.sendMessage(DataParser.getChatJson(PatientInfo.Username, ((TextBox)parameter).Text));
                PatientInfo.ChatLog.Add(DateTime.Now + ": " + ((TextBox)parameter).Text);
            });

            //TODO RelayCommand ChatToAll

            SetResistance = new RelayCommand<object>((parameter) =>
            {
                Debug.WriteLine("resistance");
                //client.sendMessage(DataParser.getSetResistanceJson(PatientInfo.Username, float.Parse(((TextBox)parameter).Text)));
                PatientInfo.Resistance = float.Parse(((TextBox)parameter).Text);
            });

        }

        public void BPMData(byte [] bytes)
        {
            //TODO
            //Parsen van de data you fuck
            if(bytes[0] == 0x00)
            {

            }
            else
            {
                PatientInfo.BPM = bytes[1];
                if (MySelectedItem == "BPM")
                    {
                    Chart.NewValue(PatientInfo.BPM);
                }

            }
            
            
        }

        public void BikeData(byte[] bytes)
        {
            //TODO
            //Parsen van de data you fuck
            switch (bytes[0])
            {
                case 0x10:
                    if (bytes[1] != 25)
                    {
                        throw new Exception();
                    }
                    PatientInfo.Distance = bytes[3];
                    PatientInfo.Speed = (bytes[4] | (bytes[5] << 8)) * 0.01;
                    if (MySelectedItem == "Speed")
                    {
                        Chart.NewValue(PatientInfo.Speed);
                    }
                    break;
                case 0x19:
                    PatientInfo.Acc_Power = bytes[3] | (bytes[4] << 8);
                    PatientInfo.Curr_Power = (bytes[5]) | (bytes[6] & 0b00001111) << 8;
                    break;
                default:
                    Debug.WriteLine("rip");
                    break;
            }
        }


    }
}
