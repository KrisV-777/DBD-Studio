namespace DBDStudio.Core.Models
{
    public enum AssignmentCategory
    {
        Texture,
        BodySlide,
        RaceMenu
    }

    public sealed class Assignment
    {
        public AssignmentCategory Category { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
