using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DBDStudio.Converters
{
    /// <summary>
    /// Converts a string to a boolean value indicating whether the string is not null or empty.
    /// </summary>
    public class StringNotNullOrEmptyConverter : IValueConverter
    {
        /// <summary>
        /// Converts a string to a boolean value indicating whether the string is not null or empty.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A boolean value indicating whether the string is not null or empty.</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is string s && !string.IsNullOrEmpty(s);

        /// <summary>
        /// Not implemented. Throws a NotSupportedException.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Throws NotSupportedException.</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
