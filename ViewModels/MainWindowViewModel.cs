namespace Body_Distribution_Studio.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private int _selectedPageIndex;
    private string _statusMessage = "Ready";

    public SettingsViewModel Settings { get; } = new();
    public TexturePacksViewModel TexturePacks { get; } = new();
    public BodySlideViewModel BodySlide { get; } = new();
    public RulesViewModel Rules { get; } = new();
    public PreviewViewModel Preview { get; } = new();

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
        3 => Rules,
        4 => Preview,
        _ => Settings
    };
}
