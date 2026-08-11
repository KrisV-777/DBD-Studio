using System.Globalization;
using Avalonia.Data.Converters;
using System.Diagnostics;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models.Textures;

namespace DBDStudio.Converters
{
    public sealed class TexturePackStateClassConverter : IValueConverter
    {
        private static TexturePackState ResolveState(object? value)
        {
            if (value is null) {
                return TexturePackState.None;
            } else if (value is TexturePackState texturePackState) {
                return texturePackState;
            } else if (value is TexturePackData pack) {
                return pack.State;
            }
            Debug.WriteLine($"[TexturePackStateClassConverter] Unrecognized value type: {value?.GetType().FullName ?? "null"}");
            return TexturePackState.None;
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
                    TexturePackState.Ephemeral => "Ephemeral Pack",
                    TexturePackState.Modified => "Modified Primordial Pack",
                    TexturePackState.Primordial => "Primordial Pack",
                    _ => "No Pack"
                };
            }

            if (string.Equals(mode, "is-ephemeral", StringComparison.OrdinalIgnoreCase)) {
                return (state == TexturePackState.Ephemeral) ^ invert;
            }

            if (string.Equals(mode, "is-primordial-edited", StringComparison.OrdinalIgnoreCase)) {
                return (state == TexturePackState.Modified) ^ invert;
            }

            if (string.Equals(mode, "is-primordial", StringComparison.OrdinalIgnoreCase)) {
                return (state == TexturePackState.Primordial) ^ invert;
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
