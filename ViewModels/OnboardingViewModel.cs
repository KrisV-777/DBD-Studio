using System;
using System.Windows.Input;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace Body_Distribution_Studio.ViewModels;

public sealed class OnboardingViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;

    public OnboardingViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        ContinueCommand = new RelayCommand(Continue);
        SkipCommand = new RelayCommand(Skip);
    }

    public event Action? Completed;
    public event Action? Skipped;

    public ICommand ContinueCommand { get; }
    public ICommand SkipCommand { get; }

    public string SkyrimDataFolder
    {
        get => _settingsService.Settings.SkyrimDataFolder;
        set
        {
            _settingsService.Settings.SkyrimDataFolder = value;
            OnPropertyChanged();
        }
    }

    public string ModsFolder
    {
        get => _settingsService.Settings.ModsFolder;
        set
        {
            _settingsService.Settings.ModsFolder = value;
            OnPropertyChanged();
        }
    }

    public string BodySlidePresetsFolder
    {
        get => _settingsService.Settings.BodySlidePresetsFolder;
        set
        {
            _settingsService.Settings.BodySlidePresetsFolder = value;
            OnPropertyChanged();
        }
    }

    public string RaceMenuPresetsFolder
    {
        get => _settingsService.Settings.RaceMenuPresetsFolder;
        set
        {
            _settingsService.Settings.RaceMenuPresetsFolder = value;
            OnPropertyChanged();
        }
    }

    private void Continue()
    {
        _settingsService.Save();
        Completed?.Invoke();
    }

    private void Skip() => Skipped?.Invoke();
}
