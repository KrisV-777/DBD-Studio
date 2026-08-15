using DBDStudio.Interfaces;
using DBDStudio.Models;

namespace DBDStudio.Services
{
    public sealed class ConditionRegistryService : IConditionRegistryService
    {
        private static readonly List<ConditionDefinition> Definitions =
        [
            new() { Name = "Race", DisplayName = "Race", Priority = 0, ValueType = ConditionValueType.FormReference, EditorType = ConditionEditorType.FormPicker, UsesFormSearch = true },
            new() { Name = "Sex", DisplayName = "Sex", Priority = 0, ValueType = ConditionValueType.Boolean, EditorType = ConditionEditorType.BooleanPicker, UsesFormSearch = false },
            new() { Name = "Level", DisplayName = "Level", Priority = 0, ValueType = ConditionValueType.Integer, EditorType = ConditionEditorType.IntegerEditor, UsesFormSearch = false },
            new() { Name = "Keyword", DisplayName = "Keyword", Priority = 1, ValueType = ConditionValueType.FormReference, EditorType = ConditionEditorType.FormPicker, UsesFormSearch = true },
            new() { Name = "Faction", DisplayName = "Faction", Priority = 2, ValueType = ConditionValueType.FormReference, EditorType = ConditionEditorType.FormPicker, UsesFormSearch = true },
            new() { Name = "FactionRank", DisplayName = "Faction Rank", Priority = 3, ValueType = ConditionValueType.FormAndInteger, EditorType = ConditionEditorType.FormAndIntegerEditor, UsesFormSearch = true },
            new() { Name = "ActorBase", DisplayName = "Actor Base", Priority = 4, ValueType = ConditionValueType.FormReference, EditorType = ConditionEditorType.FormPicker, UsesFormSearch = true },
            new() { Name = "ReferenceID", DisplayName = "Reference ID", Priority = 5, ValueType = ConditionValueType.FormReference, EditorType = ConditionEditorType.FormPicker, UsesFormSearch = true }
        ];

        public IReadOnlyList<ConditionDefinition> GetDefinitions() => Definitions;

        public ConditionDefinition? FindByName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) {
                return null;
            }

            return Definitions.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
