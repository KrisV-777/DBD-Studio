using System.Collections.ObjectModel;
using System.ComponentModel;
using DBDStudio.Interfaces.Rules;

namespace DBDStudio.Models.Rules
{
    public sealed class Condition : ModelBase, ICondition
    {
        private ConditionType _type = ConditionType.IsReference;
        private Operator _operator = Operator.Equals;
        private Conjunction _conjunction = Conjunction.And;

        public ConditionType ConditionType
        {
            get => _type;
            set
            {
                if (!SetProperty(ref _type, value))
                    return;

                Values.Clear();
                foreach (var it in ConditionDefinition.GetValuesForType(value)) {
                    Values.Add(it);
                }
            }
        }

        public Operator Operator
        {
            get => _operator;
            set => SetProperty(ref _operator, value);
        }

        public ObservableCollection<ConditionValue> Values { get; } = [new ConditionValue.Form()];

        public Conjunction Conjunction
        {
            get => _conjunction;
            set => SetProperty(ref _conjunction, value);
        }
    }
}
