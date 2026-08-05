using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DBDStudio.Core.Models;
using DBDStudio.ViewModels;

namespace DBDStudio.Views.Pages;

public partial class TexturePacksPage : UserControl
{
    public TexturePacksPage()
    {
        InitializeComponent();
    }

    private async void OnAddPackFromFolderClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not TexturePacksViewModel viewModel) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Texture Root Folder",
            AllowMultiple = false
        });

        if (result.Count > 0)
        {
            var selectedFolder = result[0].Path.LocalPath;
            viewModel.PopulatePackFromFolder(selectedFolder);
        }
    }

    private async void OnBrowseTextureClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not TexturePacksViewModel viewModel) return;
        if (sender is not Button button) return;
        if (button.Tag is not TextureMapping mapping) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var result = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Replacement Texture",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Texture Files")
                {
                    Patterns = new[] { "*.dds", "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.tga" }
                },
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } }
            }
        });

        if (result.Count > 0)
        {
            var selectedFile = result[0].Path.LocalPath;
            mapping.ReplacementTexture = "textures/dbd/" + System.IO.Path.GetFileName(selectedFile).Replace("\\", "/");
            mapping.SourcePath = selectedFile;
        }
    }

    private void OnDeleteMappingClick(object? sender, RoutedEventArgs e)
    {
        if (this.DataContext is not TexturePacksViewModel viewModel) return;
        if (sender is not Button button) return;
        if (button.Tag is not TextureMapping mapping) return;

        viewModel.SelectedMapping = mapping;
        viewModel.DeleteMappingCommand.Execute(null);
    }
}

