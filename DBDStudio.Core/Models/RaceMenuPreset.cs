namespace DBDStudio.Core.Models
{
    public sealed class RaceMenuPreset
    {
        public string Name { get; set; } = string.Empty;
        public string JsSlotFile { get; set; } = string.Empty;
        public string Sex { get; set; } = "Male";
        public string? NifFile { get; set; }
        public string? DdsFile { get; set; }
    }
}
