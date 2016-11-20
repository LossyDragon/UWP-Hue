using System;
using Windows.UI.Xaml.Data;

namespace UWP_Hue
{
    /// <summary>
    /// Utility class for converting between byte (for the Hue bridge) and double (for this app).
    /// </summary>
    internal class ByteToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            System.Convert.ToDouble((byte)value);

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            System.Convert.ToByte((double)value);
    }

    /// <summary>
    /// Utility class for converting between ushort (for the Hue bridge) and double (for this app). 
    /// </summary>
    internal class UShortToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            System.Convert.ToDouble((ushort)value);

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            System.Convert.ToUInt16((double)value);
    }
}
