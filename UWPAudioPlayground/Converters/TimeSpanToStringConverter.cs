using System;
using Windows.UI.Xaml.Data;

namespace UWPAudioPlayground.Converters
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var timeSpan = value as TimeSpan?;
            return timeSpan?.ToString("mm\\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();     
        }
    }
}
