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
        public ObservableCollection<string> ChatLog { get; set; }
        public string Username { get; set; }

        public string Status { get; set; }

        public double Speed { get; set; }

        public int BPM { get; set; }

        public float Resistance { get; set; }
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

        public ClientInfoViewModel(MainWindowViewModel mainWindowViewModel, string username)
        {
            MainWindowViewModel = mainWindowViewModel;
            this.PatientInfo = new PatientInfo() { Username = username, Status = "Waiting to start" };
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
            client.sendMessage(DataParser.getSetResistanceJson(Username, float.Parse(((TextBox)parameter).Text)));
            this.Resistance = float.Parse(((TextBox)parameter).Text);
            });

        }

        public void BPMData(byte [] bytes)
        {
            //TODO
            //Parsen van de data you fuck
            this.BPM = bytes[1];
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
                    this.Distance = bytes[3];
                    this.Speed = (bytes[4] | (bytes[5] << 8)) * 0.01;
                    break;
                case 0x19:
                    this.Acc_Power = bytes[3] | (bytes[4] << 8);
                    this.Curr_Power = (bytes[5]) | (bytes[6] & 0b00001111) << 8;
                    break;
                default:
                    throw new Exception();
            }
            
        }


    }
}
