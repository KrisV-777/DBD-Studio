using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Body_Distribution_Studio.ViewModels;

namespace Body_Distribution_Studio.Views.Pages;

public partial class SettingsPage : UserControl
{
    private SettingsViewModel ViewModel => (SettingsViewModel)DataContext!;

    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnBrowseSkyrimClicked(object? sender, RoutedEventArgs e)
        => await BrowseFolderAsync(path => ViewModel.SkyrimDataFolder = path);

    private async void OnBrowseModsClicked(object? sender, RoutedEventArgs e)
        => await BrowseFolderAsync(path => ViewModel.ModsFolder = path);

    private async void OnBrowseBodySlideClicked(object? sender, RoutedEventArgs e)
        => await BrowseFolderAsync(path => ViewModel.BodySlidePresetsFolder = path);

    private async Task BrowseFolderAsync(Action<string> setter)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = "Select Folder", AllowMultiple = false });

        if (result.Count > 0)
            setter(result[0].Path.LocalPath);
    }
}
