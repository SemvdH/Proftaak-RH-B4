using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DokterApp
{
    /// <summary>
    /// Interaction logic for WindowTabs.xaml
    /// </summary>
    public partial class WindowTabs : Window
    {
        public TabControl tbControl;
        public WindowTabs()
        {
            InitializeComponent();
        }

        private void tabControl_Load(object sender, RoutedEventArgs e)
        {
            this.tbControl = (sender as TabControl);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TabItem newTabItem = new TabItem
            {
                Header = "Test",
                Width = 110,
                Height = 40
            };
            newTabItem.Content = new UserControlForTab();
            this.tbControl.Items.Add(newTabItem);
        }
    }
}
