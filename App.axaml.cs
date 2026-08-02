using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Body_Distribution_Studio.ViewModels;
using Body_Distribution_Studio.Views;

namespace Body_Distribution_Studio;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var wizardVm = new SetupWizardViewModel();
            var wizardWindow = new SetupWizardWindow { DataContext = wizardVm };

            // When the wizard completes, open the main application window.
            wizardVm.Finished += () =>
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                wizardWindow.Close();
            };

            desktop.MainWindow = wizardWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
