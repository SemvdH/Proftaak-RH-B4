
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
namespace DokterApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Del handler;
        Client client;
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Login_Click_1(object sender, RoutedEventArgs e)
        {
            WindowTabs windowTabs = new WindowTabs();
            handler = windowTabs.NewTab;

            this.Label.Content = "Waiting";
            this.client = new Client("localhost", 5555, this.Username.Text, this.Password.Password, handler);
            while (!client.IsConnected())
            {
            }

            windowTabs.Show();
            this.Close();
        }

    }

    public delegate void Del(string message);
}
