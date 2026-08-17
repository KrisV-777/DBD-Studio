using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Text.Json.Serialization;
using DBDStudio.Interfaces.Rules;
using System.Diagnostics;

namespace DBDStudio.Models.Rules
{
    public sealed class Condition : ModelBase, ICondition
    {
        private static readonly ReadOnlyCollection<string> OperatorSymbolsInternal =
            new(["<", "<=", "==", ">=", ">", "!="]);

        private static readonly ReadOnlyCollection<string> ConjunctionLabelsInternal =
            new(["AND", "OR"]);

        private ConditionType _type = ConditionType.IsReference;
        private Operator _operator = Operator.Equals;
        private Conjunction _conjunction = Conjunction.And;

        public Condition()
        {
            SyncValuesForType(_type, preserveCompatibleValues: false);
        }

        public static IReadOnlyList<string> OperatorSymbols => OperatorSymbolsInternal;
        public static IReadOnlyList<string> ConjunctionLabels => ConjunctionLabelsInternal;

        public ConditionType ConditionType
        {
            get => _type;
            set
            {
                if (!SetProperty(ref _type, value))
                    return;

                SyncValuesForType(value, preserveCompatibleValues: true);
            }
        }

        public Operator Operator
        {
            get => _operator;
            set
            {
                if (SetProperty(ref _operator, value))
                    OnPropertyChanged(nameof(OperatorSymbol));
            }
        }

        /// <summary>
        /// Gets or sets the list of values for this condition. The number and type of values depends on the ConditionType.
        /// </summary>
        /// <remarks>
        /// TODO: Should be treated as Read-Only but serialization requires a setter to populate the collection.
        /// Using JsonObjectCreationHandling.Populate causes the values to double, Id need a "ReplaceOneByOne" behavior
        /// instead of "ClearAndAddAll" which is not supported by System.Text.Json. Will have to look for a better
        /// solution in the future.
        /// </remarks>
        public ObservableCollection<ConditionValue> Values { get; set; } = [new ConditionValue.Form()];

        public string OperatorSymbol
        {
            get => _operator switch {
                Operator.Equals => "==",
                Operator.NotEquals => "!=",
                Operator.GreaterThan => ">",
                Operator.LessThan => "<",
                Operator.GreaterThanOrEqual => ">=",
                Operator.LessThanOrEqual => "<=",
                _ => "=="
            };
            set
            {
                var parsed = value switch {
                    "==" => Operator.Equals,
                    "!=" => Operator.NotEquals,
                    ">" => Operator.GreaterThan,
                    "<" => Operator.LessThan,
                    ">=" => Operator.GreaterThanOrEqual,
                    "<=" => Operator.LessThanOrEqual,
                    _ => _operator
                };

                Operator = parsed;
            }
        }

        public Conjunction Conjunction
        {
            get => _conjunction;
            set
            {
                if (SetProperty(ref _conjunction, value))
                    OnPropertyChanged(nameof(ConjunctionLabel));
            }
        }

        public string ConjunctionLabel
        {
            get => _conjunction switch {
                Conjunction.And => "AND",
                Conjunction.Or => "OR",
                _ => "AND"
            };
            set
            {
                var parsed = value?.ToUpperInvariant() switch {
                    "AND" => Conjunction.And,
                    "OR" => Conjunction.Or,
                    _ => _conjunction
                };

                Conjunction = parsed;
            }
        }

        public Condition DeepClone()
        {
            var clone = new Condition {
                _type = _type,
                _operator = _operator,
                _conjunction = _conjunction
            };

            clone.Values.Clear();
            foreach (var value in Values)
                clone.Values.Add(value.DeepClone());

            return clone;
        }

        public void SyncValuesForType(ConditionType type, bool preserveCompatibleValues)
        {
            var nextValues = ConditionDefinition.GetValuesForType(type).ToArray();
            var formTypes = ConditionDefinition.GetFormTypeForType(type).ToArray();

            var currentValues = preserveCompatibleValues ? Values.ToArray() : [];

            for (var i = 0; i < nextValues.Length; i++) {
                if (nextValues[i] is ConditionValue.Form formValue && i < formTypes.Length) {
                    formValue.FilteredFormType = formTypes[i];
                }

                if (!preserveCompatibleValues || i >= currentValues.Length)
                    continue;

                TryCopyValue(currentValues[i], nextValues[i]);
            }

            Values.Clear();
            foreach (var value in nextValues)
                Values.Add(value);
        }

        private static void TryCopyValue(ConditionValue source, ConditionValue target)
        {
            switch (source) {
            case ConditionValue.String from when target is ConditionValue.String to:
                to.Value = from.Value;
                break;
            case ConditionValue.Integer from when target is ConditionValue.Integer to:
                to.Value = from.Value;
                break;
            case ConditionValue.Float from when target is ConditionValue.Float to:
                to.Value = from.Value;
                break;
            case ConditionValue.Boolean from when target is ConditionValue.Boolean to:
                to.Value = from.Value;
                break;
            case ConditionValue.Form from when target is ConditionValue.Form to:
                if (from.FilteredFormType == to.FilteredFormType)
                    to.Value = from.Value;
                break;
            case ConditionValue.Sex from when target is ConditionValue.Sex to:
                to.Value = from.Value;
                break;
            }
        }
    }
}
