using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;
using DBDStudio.Models;

namespace DBDStudio.Converters
{
    public sealed class ConstructStateClassConverter : IValueConverter
    {
        private static ConstructState ResolveState(object? value)
        {
            if (value is null) {
                return ConstructState.None;
            }

            if (value is ConstructState state) {
                return state;
            }

            var stateProperty = value.GetType().GetProperty("State");
            if (stateProperty?.PropertyType == typeof(ConstructState)) {
                var rawState = stateProperty.GetValue(value);
                if (rawState is ConstructState reflectedState) {
                    return reflectedState;
                }
            }

            Debug.WriteLine($"[ConstructStateClassConverter] Unrecognized value type: {value.GetType().FullName}");
            return ConstructState.None;
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var state = ResolveState(value);
            var mode = parameter as string;
            var invert = mode?.StartsWith("!", StringComparison.OrdinalIgnoreCase) ?? false;
            if (invert) {
                mode = mode?[1..];
            }

            if (string.Equals(mode, "label", StringComparison.OrdinalIgnoreCase)) {
                return state switch {
                    ConstructState.Ephemeral => "Ephemeral",
                    ConstructState.Modified => "Modified",
                    ConstructState.Primordial => "Primordial",
                    _ => "Unknown?"
                };
            }

            if (string.Equals(mode, "is-ephemeral", StringComparison.OrdinalIgnoreCase)) {
                return (state == ConstructState.Ephemeral) ^ invert;
            }

            if (string.Equals(mode, "is-primordial-edited", StringComparison.OrdinalIgnoreCase)) {
                return (state == ConstructState.Modified) ^ invert;
            }

            if (string.Equals(mode, "is-primordial", StringComparison.OrdinalIgnoreCase)) {
                return (state == ConstructState.Primordial) ^ invert;
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
