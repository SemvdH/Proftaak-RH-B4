using ClientApp.Models;
using ClientApp.Utils;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace ClientApp.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public ICommand RetryServerCommand { get; set; }
        public ICommand RetryVREngineCommand { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }


        public MainViewModel(MainWindowViewModel mainWindowViewModel)
        {
            this.MainWindowViewModel = mainWindowViewModel;
            this.RetryServerCommand = new RelayCommand(() =>
            {
                //try connect server
                this.MainWindowViewModel.InfoModel.ConnectedToServer = true;
            });
            this.RetryVREngineCommand = new RelayCommand(() =>
            {
                //try connect vr-engine
                this.MainWindowViewModel.InfoModel.ConnectedToVREngine = true;
            });
        }
    }
}
