using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DBDStudio.ViewModels;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using DBDStudio.Core.Models;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistable<T>(
     this IServiceCollection services)
     where T : class, IPersistable
    {
        services.AddSingleton<T>();
        services.AddSingleton<IPersistable>(sp => sp.GetRequiredService<T>());

        return services;
    }

    public static IServiceCollection AddPersistable<TService, TImplementation>(
        this IServiceCollection services)
        where TImplementation : class, TService, IPersistable
        where TService : class
    {
        services.AddSingleton<TService, TImplementation>();
        services.AddSingleton(sp => (IPersistable)sp.GetRequiredService<TService>());

        return services;
    }
}

namespace DBDStudio
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; } = null!;

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();
            services.AddPersistable<ApplicationSettings>();
            services.AddSingleton<PersistenceManager>();
            services.AddPersistable<ITexturePackService, TexturePackService>();
            services.AddPersistable<IBodySlideService, BodySlideService>();
            services.AddPersistable<IRaceMenuPresetService, MockRaceMenuPresetService>();
            services.AddPersistable<IRuleService, MockRuleService>();
            services.AddSingleton<IConditionRegistryService, ConditionRegistryService>();
            services.AddSingleton<IRuleResolutionService, RuleResolutionService>();
            services.AddSingleton<MutagenSkyrimService>();
            services.AddSingleton<IFormDatabaseService>(sp => sp.GetRequiredService<MutagenSkyrimService>());
            services.AddSingleton<ILoadOrderService>(sp => sp.GetRequiredService<MutagenSkyrimService>());
            services.AddTransient<MainWindowViewModel>();
            Services = services.BuildServiceProvider();

            var settings = Services.GetRequiredService<ApplicationSettings>();
            var persistenceManager = Services.GetRequiredService<PersistenceManager>();
            persistenceManager.Load();
            Services.GetRequiredService<ILoadOrderService>().Initialize(settings.SkyrimDataFolder);

            // Apply saved font sizes to application resources
            var baseFontSize = settings.BaseFontSize;
            Resources["FontSize"] = baseFontSize;
            Resources["H1FontSize"] = baseFontSize * 1.6;
            Resources["H2FontSize"] = baseFontSize * 1.3;
            Resources["CaptionFontSize"] = baseFontSize * 0.85;
            Resources["TinyFontSize"] = baseFontSize * 0.7;

            // Apply saved theme
            var theme = settings.Theme;
            var themeVariant = theme switch
            {
                "Light" => Avalonia.Styling.ThemeVariant.Light,
                "Dark" => Avalonia.Styling.ThemeVariant.Dark,
                _ => Avalonia.Styling.ThemeVariant.Default
            };
            RequestedThemeVariant = themeVariant;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Exit += (_, _) => persistenceManager.Save();
                var mainWindowViewModel = Services.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = new MainWindow(mainWindowViewModel);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
