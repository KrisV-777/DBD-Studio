namespace DBDStudio.Models.Mutagen
{
    public sealed class FormRecord : DBDComponent
    {
        public string EditorId { get; init; } = string.Empty;
        public uint FormId { get; init; } = 0;
        public string Plugin { get; init; } = string.Empty;
        public string RecordType { get; init; } = string.Empty;
        public FormReference FormReference => new(Plugin, FormId);

        public bool MatchQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return true;

            query = query.Trim();

            if (query.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                uint.TryParse(query[2..], System.Globalization.NumberStyles.HexNumber, null, out var formId)) {
                return FormId == formId;
            }

            return Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                EditorId.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                FormReference.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                FormId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase);
        }

        internal override DBDComponent Copy()
            => throw new NotSupportedException("Copying is not supported for FormRecord.");
        internal override void Import(DBDComponent source)
            => throw new NotSupportedException("Importing is not supported for FormRecord.");
    }
}
