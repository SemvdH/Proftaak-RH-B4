using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ClientApp.ValueConverters
{
    //[ValueConversion(typeof(bool), typeof(BitmapImage))]

    class BoolToMarkConverter : IValueConverter
    {
        public BitmapImage TrueImage { get; set; } = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/CheckMark.png"));
        public BitmapImage FalseImage { get; set; } = new BitmapImage(new Uri("pack://application:,,,/Images/Icons/CrossMark.png"));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool))
            {
                return null;
            }

            bool b = (bool)value;
            if (b)
            {
                return this.TrueImage;
            }
            else
            {
                return this.FalseImage;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
