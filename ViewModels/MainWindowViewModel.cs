using DBDStudio.Core.Interfaces;

namespace Body_Distribution_Studio.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private int _selectedPageIndex;
    private string _statusMessage = "Ready";

    public MainWindowViewModel(
        ISettingsService settingsService,
        ITexturePackService texturePackService,
        IBodySlideService bodySlideService,
        IRuleService ruleService,
        IConditionRegistryService conditionRegistryService,
        IRuleResolutionService ruleResolutionService,
        IFormDatabaseService formDatabaseService,
        ILoadOrderService loadOrderService,
        IRaceMenuPresetService raceMenuPresetService)
    {
        Settings = new SettingsViewModel(settingsService);
        TexturePacks = new TexturePacksViewModel(texturePackService);
        BodySlide = new BodySlideViewModel(bodySlideService);
        RaceMenuPresets = new RaceMenuPresetsViewModel(raceMenuPresetService);
        Rules = new RulesViewModel(ruleService, raceMenuPresetService, conditionRegistryService, ruleResolutionService);
        Preview = new PreviewViewModel();
        RulePreview = new RulePreviewViewModel(formDatabaseService, ruleService, ruleResolutionService);
        LoadOrderExplorer = new LoadOrderExplorerViewModel(loadOrderService);
    }

    public SettingsViewModel Settings { get; }
    public TexturePacksViewModel TexturePacks { get; }
    public BodySlideViewModel BodySlide { get; }
    public RaceMenuPresetsViewModel RaceMenuPresets { get; }
    public RulesViewModel Rules { get; }
    public PreviewViewModel Preview { get; }
    public RulePreviewViewModel RulePreview { get; }
    public LoadOrderExplorerViewModel LoadOrderExplorer { get; }

    public int SelectedPageIndex
    {
        get => _selectedPageIndex;
        set
        {
            if (SetField(ref _selectedPageIndex, value))
                OnPropertyChanged(nameof(CurrentPage));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
    }

    public ViewModelBase CurrentPage => SelectedPageIndex switch
    {
        0 => Settings,
        1 => TexturePacks,
        2 => BodySlide,
        3 => RaceMenuPresets,
        4 => Rules,
        5 => RulePreview,
        6 => LoadOrderExplorer,
        7 => Preview,
        _ => Settings
    };
}
