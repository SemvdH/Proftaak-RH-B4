using System;
using System.Collections.Generic;
using System.Text;
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
    /// Interaction logic for UserControlForTab.xaml
    /// </summary>
    public partial class UserControlForTab : UserControl
    {
        public UserControlForTab()
        {
            InitializeComponent();
            Username_Label.Content = "Bob";
            Status_Label.Content = "Status: Dead";
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChatBox.Items.Add(textBox_Chat.Text);
        }

        private void StartSession_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StopSession_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetResistance_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClientInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("firstname:\tBob\n" +
                "surname:\t\tde Bouwer");
        }

    }
}
