using ClientApp.Models;
using ClientApp.Utils;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace ClientApp.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public Info infoModel { get; set; }
        public ICommand RetryServerCommand { get; set; }
        public ICommand RetryVREngineCommand { get; set; }

        public MainViewModel()
        {
            this.infoModel = new Info();
            this.RetryServerCommand = new RelayCommand(() =>
            {
                //try connect server
                this.infoModel.ConnectedToServer = true;
            });
            this.RetryVREngineCommand = new RelayCommand(() =>
            {
                //try connect vr-engine
                this.infoModel.ConnectedToVREngine = true;
            });
        }
    }
}
