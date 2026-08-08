using DBDStudio.Core.Interfaces;

namespace DBDStudio.ViewModels
{
    public sealed class MainWindowViewModel : ViewModelBase
    {
        private int _selectedPageIndex;
        private string _statusMessage = "Ready";
        private readonly ISettingsService _settingsService;
        private readonly ITexturePackService _texturePackService;
        private readonly IBodySlideService _bodySlideService;
        private readonly IRuleService _ruleService;
        private readonly IConditionRegistryService _conditionRegistryService;
        private readonly IRuleResolutionService _ruleResolutionService;
        private readonly IFormDatabaseService _formDatabaseService;
        private readonly ILoadOrderService _loadOrderService;
        private readonly IRaceMenuPresetService _raceMenuPresetService;

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
            _settingsService = settingsService;
            _texturePackService = texturePackService;
            _bodySlideService = bodySlideService;
            _ruleService = ruleService;
            _conditionRegistryService = conditionRegistryService;
            _ruleResolutionService = ruleResolutionService;
            _formDatabaseService = formDatabaseService;
            _loadOrderService = loadOrderService;
            _raceMenuPresetService = raceMenuPresetService;

            // Create SettingsViewModel with reference to this MainWindowViewModel
            Settings = new SettingsViewModel(settingsService, texturePackService, bodySlideService, raceMenuPresetService, this);
            TexturePacks = new TexturePacksViewModel(texturePackService, this);
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
}
