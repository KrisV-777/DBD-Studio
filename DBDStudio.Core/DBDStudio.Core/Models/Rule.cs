using System.Collections.Generic;

namespace DBDStudio.Core.Models;

public sealed class Rule
{
    public string Name { get; set; } = string.Empty;
    public Assignment? TextureAssignment { get; set; }
    public Assignment? BodySlideAssignment { get; set; }
    public Assignment? RaceMenuAssignment { get; set; }

    public string TexturePack
    {
        get => TextureAssignment?.Value ?? string.Empty;
        set => TextureAssignment = string.IsNullOrWhiteSpace(value)
            ? null
            : new Assignment { Category = AssignmentCategory.Texture, Value = value };
    }

    public string BodySlidePreset
    {
        get => BodySlideAssignment?.Value ?? string.Empty;
        set => BodySlideAssignment = string.IsNullOrWhiteSpace(value)
            ? null
            : new Assignment { Category = AssignmentCategory.BodySlide, Value = value };
    }

    public string RaceMenuPreset
    {
        get => RaceMenuAssignment?.Value ?? string.Empty;
        set => RaceMenuAssignment = string.IsNullOrWhiteSpace(value)
            ? null
            : new Assignment { Category = AssignmentCategory.RaceMenu, Value = value };
    }

    public string PriorityPreview { get; set; } = "Generic Match";
    public List<Condition> Conditions { get; } = [];
}
