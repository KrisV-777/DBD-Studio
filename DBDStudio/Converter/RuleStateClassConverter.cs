using System.Diagnostics;
using System.Globalization;
using Avalonia.Data.Converters;
using DBDStudio.Interfaces;
using DBDStudio.Models.Rules;

namespace DBDStudio.Converters
{
    public sealed class RuleStateClassConverter : IValueConverter
    {
        private static RuleState ResolveState(object? value)
        {
            if (value is null) {
                return RuleState.None;
            }

            if (value is RuleState state) {
                return state;
            }

            if (value is RenderedRuleData renderedRule) {
                return renderedRule.State;
            }

            Debug.WriteLine($"[RuleStateClassConverter] Unrecognized value type: {value.GetType().FullName}");
            return RuleState.None;
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
                    RuleState.Ephemeral => "Ephemeral Rule",
                    RuleState.Modified => "Modified Primordial Rule",
                    RuleState.Primordial => "Primordial Rule",
                    _ => "No Rule"
                };
            }

            if (string.Equals(mode, "is-ephemeral", StringComparison.OrdinalIgnoreCase)) {
                return (state == RuleState.Ephemeral) ^ invert;
            }

            if (string.Equals(mode, "is-primordial-edited", StringComparison.OrdinalIgnoreCase)) {
                return (state == RuleState.Modified) ^ invert;
            }

            if (string.Equals(mode, "is-primordial", StringComparison.OrdinalIgnoreCase)) {
                return (state == RuleState.Primordial) ^ invert;
            }

            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
