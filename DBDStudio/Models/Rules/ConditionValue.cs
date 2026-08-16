using DBDStudio.Models.Mutagen;

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

        public sealed class Form : ConditionValue
        {
            public FormRecord? Value { get; set; }
        }
    }
}
