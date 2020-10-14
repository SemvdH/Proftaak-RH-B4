using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace DoctorApp.Utils
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
