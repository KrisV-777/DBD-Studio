using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Body_Distribution_Studio.ViewModels;
using Body_Distribution_Studio.Views;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Body_Distribution_Studio;

public partial class App : Application
{
    public IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ISettingsService, MockSettingsService>();
        services.AddSingleton<ITexturePackService, MockTexturePackService>();
        services.AddSingleton<IBodySlideService, MockBodySlideService>();
        services.AddSingleton<IRuleService, MockRuleService>();
        services.AddSingleton<IRaceMenuPresetService, MockRaceMenuPresetService>();
        services.AddSingleton<MutagenSkyrimService>();
        services.AddSingleton<IFormDatabaseService>(sp => sp.GetRequiredService<MutagenSkyrimService>());
        services.AddSingleton<ILoadOrderService>(sp => sp.GetRequiredService<MutagenSkyrimService>());
        services.AddTransient<OnboardingViewModel>();
        services.AddTransient<MainWindowViewModel>();
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var onboardingViewModel = Services.GetRequiredService<OnboardingViewModel>();
            var onboardingWindow = new OnboardingWindow { DataContext = onboardingViewModel };

            onboardingViewModel.Completed += () =>
            {
                var mainWindow = new MainWindow(Services.GetRequiredService<MainWindowViewModel>());
                mainWindow.Show();
                onboardingWindow.Close();
            };

            onboardingViewModel.Skipped += () =>
            {
                var mainWindow = new MainWindow(Services.GetRequiredService<MainWindowViewModel>());
                mainWindow.Show();
                onboardingWindow.Close();
            };

            desktop.MainWindow = onboardingWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
