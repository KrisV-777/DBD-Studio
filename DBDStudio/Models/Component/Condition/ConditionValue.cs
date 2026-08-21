using DBDStudio.Models.Mutagen;
using DBDStudio.Interfaces.Mutagen;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace DBDStudio.Models.Component.Condition
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(String), "string")]
    [JsonDerivedType(typeof(Integer), "integer")]
    [JsonDerivedType(typeof(Float), "float")]
    [JsonDerivedType(typeof(Boolean), "boolean")]
    [JsonDerivedType(typeof(Sex), "sex")]
    [JsonDerivedType(typeof(Form), "form")]
    public abstract class ConditionValue : ModelBase
    {
        public sealed class String : ConditionValue
        {
            private string _value = string.Empty;
            public string Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }
        }

        public sealed class Integer : ConditionValue
        {
            private int _value;
            public int Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }
        }

        public sealed class Float : ConditionValue
        {
            private float _value;
            public float Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }
        }

        public sealed class Boolean : ConditionValue
        {
            private bool _value;
            public bool Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }
        }

        public sealed class Sex : ConditionValue
        {
            private static readonly IReadOnlyList<string> ChoicesInternal = Models.Sex.Sexes;

            private bool _value;
            public bool Value
            {
                get => _value;
                set
                {
                    if (SetProperty(ref _value, value)) {
                        OnPropertyChanged(nameof(SelectedSex));
                    }
                }
            }

            public static IReadOnlyList<string> Choices => ChoicesInternal;

            public string SelectedSex
            {
                get => Value ? Models.Sex.Male : Models.Sex.Female;
                set => Value = string.Equals(value, Models.Sex.Male, StringComparison.OrdinalIgnoreCase);
            }
        }

        public sealed class Form : ConditionValue
        {
            private FormRecord? _value;
            public FormRecord? Value
            {
                get => _value;
                set => SetProperty(ref _value, value);
            }
            public FormType FilteredFormType { get; set; } = FormType.None;
        }

        public ConditionValue DeepClone()
        {
            return this switch {
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
