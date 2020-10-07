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
            System.Diagnostics.Debug.WriteLine("derp1");

            Client client = new Client();
            System.Diagnostics.Debug.WriteLine("derp2");


            InitializeComponent();
            DataContext = new MainWindowViewModel();
            System.Diagnostics.Debug.WriteLine("derp3");


            //BLEHandler bLEHandler = new BLEHandler(client);

            //bLEHandler.Connect();

            //client.setHandler(bLEHandler);


            BikeSimulator bikeSimulator = new BikeSimulator(client);
            System.Diagnostics.Debug.WriteLine("derp4");

            Thread newThread = new Thread(new ThreadStart(bikeSimulator.StartSimulation));
            newThread.Start();
            //bikeSimulator.StartSimulation();
            System.Diagnostics.Debug.WriteLine("derp5");


            client.setHandler(bikeSimulator);
            System.Diagnostics.Debug.WriteLine("derp6");


        }
    }
}
