using ClientApp.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientApp.ViewModels
{
    class MainViewModel : ObservableObject
    {
        public string StatusLabelText { get; set; }

        public MainViewModel()
        {
            StatusLabelText = "Status: not running";
        }
    }
}
