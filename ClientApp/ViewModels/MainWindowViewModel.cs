using ClientApp.Models;
using ClientApp.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ClientApp.ViewModels
{
    class MainWindowViewModel : ObservableObject
    {
        #region private members

        private Window mWindow;

        private int mOuterMarginSize = 10;
        private int mWindowRadius = 10;

        #endregion

        #region public properties
        public Info InfoModel { get; set; }

        public ObservableObject SelectedViewModel { get; set; }

        public Client client { get; }

        /// <summary>
        /// size of the resize border around the window
        /// </summary>
        public int ResizeBorder { get; set; } = 6;

        public Thickness ResizeBorderThickness { get { return new Thickness(ResizeBorder + OuterMarginSize); } }

        public Thickness OuterMarginThickness { get { return new Thickness(OuterMarginSize); } }

        public CornerRadius WindowCornerRadius { get { return new CornerRadius(WindowRadius); } }

        public int OuterMarginSize
        {
            get
            {
                return mWindow.WindowState == WindowState.Maximized ? 0 : mOuterMarginSize;
            }
            set
            {
                mOuterMarginSize = value;
            }
        }

        public int WindowRadius
        {
            get
            {
                return mWindow.WindowState == WindowState.Maximized ? 0 : mWindowRadius;
            }
            set
            {
                mWindowRadius = value;
            }
        }

        public int TitleHeight { get; set; } = 42;

        public GridLength TitleHeightGridLegth { get { return new GridLength(TitleHeight + ResizeBorder); } }

        #endregion

        public MainWindowViewModel(Window window, Client client)
        {
            this.mWindow = window;

            this.mWindow.StateChanged += (sender, e) =>
            {
                OnPropertyChanged(nameof(ResizeBorderThickness));
                OnPropertyChanged(nameof(OuterMarginThickness));
                OnPropertyChanged(nameof(WindowCornerRadius));
                OnPropertyChanged(nameof(OuterMarginSize));
                OnPropertyChanged(nameof(WindowRadius));
            };

            this.InfoModel = new Info();
            this.client = client;
            LoginViewModel loginViewModel = new LoginViewModel(this);
            SelectedViewModel = loginViewModel;
            this.client.SetLoginViewModel(loginViewModel);
        }

    }
}
