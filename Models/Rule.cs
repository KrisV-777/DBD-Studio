using System.Collections.ObjectModel;
using Body_Distribution_Studio.ViewModels;

namespace Body_Distribution_Studio.Models;

public sealed class RuleCondition : ViewModelBase
{
    private string _type = string.Empty;
    private string _operator = string.Empty;
    private string _value = string.Empty;

    public string Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }

    public string Operator
    {
        get => _operator;
        set => SetField(ref _operator, value);
    }

    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }
}

public sealed class Rule : ViewModelBase
{
    private string _name = string.Empty;
    private string _texturePack = string.Empty;
    private string _bodySlidePreset = string.Empty;
    private string _priorityPreview = "Generic Match";

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public string TexturePack
    {
        get => _texturePack;
        set => SetField(ref _texturePack, value);
    }

    public string BodySlidePreset
    {
        get => _bodySlidePreset;
        set => SetField(ref _bodySlidePreset, value);
    }

    public string PriorityPreview
    {
        get => _priorityPreview;
        set => SetField(ref _priorityPreview, value);
    }

    public ObservableCollection<RuleCondition> Conditions { get; } = [];
}
