using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DBDStudio.ViewModels;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DBDStudio;

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
        services.AddSingleton<IWorkspaceService, InMemoryWorkspaceService>();
        services.AddSingleton<ISettingsService, MockSettingsService>();
        services.AddSingleton<ITexturePackService, MockTexturePackService>();
        services.AddSingleton<IBodySlideService, MockBodySlideService>();
        services.AddSingleton<IRuleService, MockRuleService>();
        services.AddSingleton<IRaceMenuPresetService, MockRaceMenuPresetService>();
        services.AddSingleton<IConditionRegistryService, ConditionRegistryService>();
        services.AddSingleton<IRuleResolutionService, RuleResolutionService>();
        services.AddSingleton<MutagenSkyrimService>();
        services.AddSingleton<IFormDatabaseService>(sp => sp.GetRequiredService<MutagenSkyrimService>());
        services.AddSingleton<ILoadOrderService>(sp => sp.GetRequiredService<MutagenSkyrimService>());
        services.AddTransient<MainWindowViewModel>();
        Services = services.BuildServiceProvider();
        
        var settingsService = Services.GetRequiredService<ISettingsService>();
        settingsService.Load();
        Services.GetRequiredService<ILoadOrderService>().Initialize(settingsService.Settings.SkyrimDataFolder);

        // Apply saved font sizes to application resources
        var baseFontSize = settingsService.Settings.BaseFontSize;
        Resources["FontSize"] = baseFontSize;
        Resources["H1FontSize"] = baseFontSize * 1.6;
        Resources["H2FontSize"] = baseFontSize * 1.3;
        Resources["CaptionFontSize"] = baseFontSize * 0.85;
        Resources["TinyFontSize"] = baseFontSize * 0.7;

        // Apply saved theme
        var theme = settingsService.Settings.Theme;
        var themeVariant = theme switch
        {
            "Light" => Avalonia.Styling.ThemeVariant.Light,
            "Dark" => Avalonia.Styling.ThemeVariant.Dark,
            _ => Avalonia.Styling.ThemeVariant.Default
        };
        RequestedThemeVariant = themeVariant;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow(mainWindowViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
