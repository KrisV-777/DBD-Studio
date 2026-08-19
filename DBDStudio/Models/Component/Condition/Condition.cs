using System.Collections.ObjectModel;
using DBDStudio.Interfaces.Rules;

namespace DBDStudio.Models.Component.Condition
{
    public sealed class Condition : ModelBase, ICondition
    {
        private static readonly ReadOnlyCollection<KeyValuePair<Operator, string>> OperatorSymbolsInternal =
            new([
                new KeyValuePair<Operator, string>(Operator.LessThan, "<"),
                new KeyValuePair<Operator, string>(Operator.LessThanOrEqual, "<="),
                new KeyValuePair<Operator, string>(Operator.Equals, "=="),
                new KeyValuePair<Operator, string>(Operator.GreaterThanOrEqual, ">="),
                new KeyValuePair<Operator, string>(Operator.GreaterThan, ">"),
                new KeyValuePair<Operator, string>(Operator.NotEquals, "!=")
            ]);

        private static readonly ReadOnlyCollection<KeyValuePair<Conjunction, string>> ConjunctionLabelsInternal =
            new([
                new KeyValuePair<Conjunction, string>(Conjunction.And, "AND"),
                new KeyValuePair<Conjunction, string>(Conjunction.Or, "OR")
            ]);

        #region Fields

        private ConditionType _type = ConditionType.IsReference;
        private Operator _operator = Operator.Equals;
        private Conjunction _conjunction = Conjunction.And;

        #endregion

        #region Properties

        public static IReadOnlyList<string> OperatorSymbols => OperatorSymbolsInternal.Aggregate(
            new List<string>(), (list, pair) => { list.Add(pair.Value); return list; });
        public static IReadOnlyList<string> ConjunctionLabels => ConjunctionLabelsInternal.Aggregate(
            new List<string>(), (list, pair) => { list.Add(pair.Value); return list; });

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

        public Operator Operator
        {
            get => _operator;
            set
            {
                if (SetProperty(ref _operator, value))
                    OnPropertyChanged(nameof(OperatorSymbol));
            }
        }

        public string OperatorSymbol
        {
            get => OperatorSymbolsInternal.FirstOrDefault(pair => pair.Key == _operator).Value ?? "==";
            set => Operator = OperatorSymbolsInternal.FirstOrDefault(pair => pair.Value == value).Key;
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
            get => ConjunctionLabelsInternal.FirstOrDefault(pair => pair.Key == _conjunction).Value ?? "AND";
            set => Conjunction = ConjunctionLabelsInternal.FirstOrDefault(pair => pair.Value == value).Key;
        }

        #endregion

        #region Constructors

        public Condition()
        {
            SyncValuesForType(_type, preserveCompatibleValues: false);
        }

        public ICondition DeepClone()
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

        #endregion

        #region Private Methods

        private void SyncValuesForType(ConditionType type, bool preserveCompatibleValues)
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

        #endregion
    }
}
