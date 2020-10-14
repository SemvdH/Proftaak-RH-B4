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
