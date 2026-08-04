using System;
using System.Windows.Input;

namespace DBDStudio.ViewModels;

public sealed class SetupWizardViewModel : ViewModelBase
{
    private int _currentStep;
    private string _skyrimDataFolder = string.Empty;
    private string _modsFolder = string.Empty;
    private string _bodySlidePresetsFolder = string.Empty;

    public event Action? Finished;

    public SetupWizardViewModel()
    {
        NextCommand = new RelayCommand(GoNext, () => CanGoNext);
        BackCommand = new RelayCommand(GoBack, () => CanGoBack);
        FinishCommand = new RelayCommand(() => Finished?.Invoke());
    }

    public ICommand NextCommand { get; }
    public ICommand BackCommand { get; }
    public ICommand FinishCommand { get; }

    public int CurrentStep
    {
        get => _currentStep;
        private set
        {
            if (!SetField(ref _currentStep, value))
                return;

            OnPropertyChanged(nameof(IsStep0));
            OnPropertyChanged(nameof(IsStep1));
            OnPropertyChanged(nameof(IsStep2));
            OnPropertyChanged(nameof(IsStep3));
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(StepTitle));
            ((RelayCommand)NextCommand).RaiseCanExecuteChanged();
            ((RelayCommand)BackCommand).RaiseCanExecuteChanged();
        }
    }

    public string SkyrimDataFolder
    {
        get => _skyrimDataFolder;
        set => SetField(ref _skyrimDataFolder, value);
    }

    public string ModsFolder
    {
        get => _modsFolder;
        set => SetField(ref _modsFolder, value);
    }

    public string BodySlidePresetsFolder
    {
        get => _bodySlidePresetsFolder;
        set => SetField(ref _bodySlidePresetsFolder, value);
    }

    public string RaceMenuPresetsFolder { get; set; } = string.Empty;

    public bool IsStep0 => _currentStep == 0;
    public bool IsStep1 => _currentStep == 1;
    public bool IsStep2 => _currentStep == 2;
    public bool IsStep3 => _currentStep == 3;
    public bool CanGoBack => _currentStep > 0;
    public bool CanGoNext => _currentStep < 3;

    public string StepTitle => _currentStep switch
    {
        0 => "Step 1 of 4 — Skyrim Data Folder",
        1 => "Step 2 of 4 — Mods Folder",
        2 => "Step 3 of 4 — BodySlide Presets Folder",
        3 => "Step 4 of 4 — Finish Setup",
        _ => string.Empty
    };

    private void GoNext() => CurrentStep++;
    private void GoBack() => CurrentStep--;
}
