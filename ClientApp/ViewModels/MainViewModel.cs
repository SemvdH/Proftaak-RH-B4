using ClientApp.Models;
using ClientApp.Utils;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using System.Windows.Input;

namespace ClientApp.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public ICommand RetryServerCommand { get; set; }
        public ICommand RetryVREngineCommand { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }

        private Client client;

        public MainViewModel(MainWindowViewModel mainWindowViewModel)
        {
            this.MainWindowViewModel = mainWindowViewModel;
            client = this.MainWindowViewModel.client;
            client.engineConnectFailed = retryEngineConnection;
            client.engineConnectSuccess = succesEngineConnection;
            this.RetryServerCommand = new RelayCommand(() =>
            {
                //try connect server
                this.MainWindowViewModel.InfoModel.ConnectedToServer = true;
            });
            this.RetryVREngineCommand = new RelayCommand(() =>
            {
                //try connect vr-engine

                //this.MainWindowViewModel.InfoModel.ConnectedToVREngine = true;
                //this.MainWindowViewModel.InfoModel.CanConnectToVR = false;
                //client.engineConnection.CreateConnection();
                //retryEngineConnection();
                Debug.WriteLine("retry button clicked");

            });
        }

        private void retryEngineConnection()
        {
            this.MainWindowViewModel.InfoModel.ConnectedToVREngine = false;
            this.MainWindowViewModel.InfoModel.CanConnectToVR = true;
            client.engineConnection.CreateConnection();
        }

        private void succesEngineConnection()
        {
            this.MainWindowViewModel.InfoModel.ConnectedToVREngine = true;
            this.MainWindowViewModel.InfoModel.CanConnectToVR = false;

        }


    }
}
