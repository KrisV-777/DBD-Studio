using DBDStudio.Models.Mutagen;
using DBDStudio.Interfaces.Mutagen;

namespace DBDStudio.Models.Rules
{
    public abstract class ConditionValue
    {
        public sealed class String : ConditionValue
        {
            public string Value { get; set; } = string.Empty;
        }

        public sealed class Integer : ConditionValue
        {
            public int Value { get; set; }
        }

        public sealed class Float : ConditionValue
        {
            public float Value { get; set; }
        }

        public sealed class Boolean : ConditionValue
        {
            public bool Value { get; set; }
        }

        public sealed class Sex : ConditionValue
        {
            private static readonly IReadOnlyList<string> ChoicesInternal = ["Male", "Female"];

            public bool Value { get; set; }

            public IReadOnlyList<string> Choices => ChoicesInternal;

            public string SelectedSex
            {
                get => Value ? "Male" : "Female";
                set => Value = string.Equals(value, "Male", StringComparison.OrdinalIgnoreCase);
            }
        }

        public sealed class Form : ConditionValue
        {
            public FormRecord? Value { get; set; }
            public FormType FilteredFormType { get; set; } = FormType.None;
        }

        public ConditionValue DeepClone()
        {
            return this switch
            {
                String it => new String { Value = it.Value },
                Integer it => new Integer { Value = it.Value },
                Float it => new Float { Value = it.Value },
                Boolean it => new Boolean { Value = it.Value },
                Sex it => new Sex { Value = it.Value },
                Form it => new Form { Value = it.Value, FilteredFormType = it.FilteredFormType },
                _ => throw new ArgumentOutOfRangeException(nameof(ConditionValue), GetType(), null)
            };
        }
    }
}
