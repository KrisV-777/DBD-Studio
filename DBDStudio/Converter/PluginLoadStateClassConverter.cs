using System.Globalization;
using Avalonia.Data.Converters;
using DBDStudio.Interfaces.Mutagen;

namespace DBDStudio.Converters
{
    public sealed class PluginLoadStateClassConverter : IValueConverter
    {
        private static PluginLoadState ResolveState(object? value)
        {
            if (value is PluginLoadState state)
            {
                return state;
            }

            if (value is IPluginData plugin)
            {
                return plugin.LoadState;
            }

            return PluginLoadState.NotLoaded;
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var state = ResolveState(value);
            var mode = parameter as string;
            var invert = mode?.StartsWith("!", StringComparison.OrdinalIgnoreCase) ?? false;
            if (invert)
            {
                mode = mode?[1..];
            }

            if (string.Equals(mode, "label", StringComparison.OrdinalIgnoreCase))
            {
                return state switch
                {
                    PluginLoadState.Loading => "Loading",
                    PluginLoadState.Loaded => "",
                    _ => "Not Loaded"
                };
            }

            if (string.Equals(mode, "is-notloaded", StringComparison.OrdinalIgnoreCase))
            {
                return (state == PluginLoadState.NotLoaded) ^ invert;
            }

            if (string.Equals(mode, "is-loading", StringComparison.OrdinalIgnoreCase))
            {
                return (state == PluginLoadState.Loading) ^ invert;
            }

            if (string.Equals(mode, "is-loaded", StringComparison.OrdinalIgnoreCase))
            {
                return (state == PluginLoadState.Loaded) ^ invert;
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
