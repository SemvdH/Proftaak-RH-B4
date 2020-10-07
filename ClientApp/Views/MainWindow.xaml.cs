using ClientApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClientApp.Utils;
using Hardware.Simulators;
using System.Threading;

namespace ClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Client client = new Client();


            InitializeComponent();
            DataContext = new MainWindowViewModel(client);


            //BLEHandler bLEHandler = new BLEHandler(client);

            //bLEHandler.Connect();

            //client.setHandler(bLEHandler);


            BikeSimulator bikeSimulator = new BikeSimulator(client);

            Thread newThread = new Thread(new ThreadStart(bikeSimulator.StartSimulation));
            newThread.Start();


            client.SetHandler(bikeSimulator);


        }
    }
}
