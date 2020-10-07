using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DokterApp
{
    public interface ITab
    {
        string Name { get; set; }
        ICommand CloseCommand { get; }
        event EventHandler CloseRequested;
    }

    public abstract class Tab : ITab
    {
        public string Name { get; set; }
        public ICommand CloseCommand { get; }
        public event EventHandler CloseRequested;

        public Tab()
        {
            //CloseCommand = 
        }

    }

}
