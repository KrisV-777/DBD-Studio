namespace DBDStudio.Core.Models
{
    public enum ConditionValueType
    {
        FormReference,
        Boolean,
        Integer,
        FormAndInteger
    }

    public enum ConditionEditorType
    {
        FormPicker,
        BooleanPicker,
        IntegerEditor,
        FormAndIntegerEditor
    }

    public sealed class ConditionDefinition
    {
        public string Name { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public int Priority { get; init; }
        public ConditionValueType ValueType { get; init; }
        public ConditionEditorType EditorType { get; init; }
        public bool UsesFormSearch { get; init; }
    }
}
