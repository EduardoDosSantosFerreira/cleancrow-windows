using System;
using System.Globalization;
using System.Windows.Data;

namespace CleanCrow.Converters
{
    /// <summary>
    /// Converte o progresso (0-100) para o contador de operações (0-45)
    /// </summary>
    public class ProgressToOperationsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int progress)
            {
                var operations = (int)(progress / 100.0 * 45);
                return $"{operations}/45";
            }
            return "0/45";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}