using System.Globalization;
using Avalonia.Data.Converters;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace DBDStudio.Converters
{
    public sealed class TexturePackStateClassConverter : IValueConverter
    {
        private static TexturePackState ResolveState(object? value)
        {
            if (value is TexturePackState texturePackState) {
                return texturePackState;
            }

            if (value is TexturePack pack && Avalonia.Application.Current is App app && app.Services is not null) {
                var texturePackService = app.Services.GetRequiredService<ITexturePackService>();
                return texturePackService.GetTexturePackState(pack);
            }

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
                    TexturePackState.Ephemeral => "Workspace Pack",
                    TexturePackState.DiskEdited => "Edited Pack",
                    TexturePackState.Disk => "File Pack",
                    _ => "No Pack"
                };
            }

            if (string.Equals(mode, "is-ephemeral", StringComparison.OrdinalIgnoreCase)) {
                return (state == TexturePackState.Ephemeral) ^ invert;
            }

            if (string.Equals(mode, "is-disk-edited", StringComparison.OrdinalIgnoreCase)) {
                return (state == TexturePackState.DiskEdited) ^ invert;
            }

            if (string.Equals(mode, "is-disk", StringComparison.OrdinalIgnoreCase)) {
                return (state == TexturePackState.Disk) ^ invert;
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
