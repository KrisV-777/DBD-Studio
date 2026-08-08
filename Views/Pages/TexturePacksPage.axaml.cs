using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DBDStudio.Core.Models;
using DBDStudio.ViewModels;

namespace DBDStudio.Views.Pages
{
    public partial class TexturePacksPage : UserControl
    {
        public TexturePacksPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the click event for adding a new texture pack from a selected folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAddPackFromFolderClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TexturePacksViewModel viewModel) return;

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

        /// <summary>
        /// Handles the click event for browsing and selecting a replacement texture file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnBrowseTextureClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TexturePacksViewModel viewModel) return;
            if (sender is not Button button) return;
            if (button.Tag is not TextureMapping mapping) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null) return;

            var result = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Replacement Texture",
                AllowMultiple = false,
                FileTypeFilter = [
                    new FilePickerFileType("Texture Files")
                    {
                        Patterns = ["*.dds", "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.tga"]
                    },
                    new FilePickerFileType("All Files") { Patterns = ["*"] }
                ]
            });

            if (result.Count <= 0)
                return;

            var selectedFile = result[0].Path.LocalPath;
            viewModel.SetSelectedMappingReplacementTexture(mapping, selectedFile);
        }

        private async void OnAddFolderClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TexturePacksViewModel viewModel) return;

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
                viewModel.PopulatePackFromFolder(selectedFolder, viewModel.SelectedPack);
            }
        }

        private async void OnAddMappingClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TexturePacksViewModel viewModel) return;
            viewModel.AddMappingCommand.Execute(null);

            if (viewModel.SelectedMapping is null)
                return;

            // Wait for the grid to materialize the new row before requesting scroll.
            await Dispatcher.UIThread.InvokeAsync(
                () => MappingsDataGrid.ScrollIntoView(viewModel.SelectedMapping, null),
                DispatcherPriority.Background);
        }

        private async void OnExportPackClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TexturePacksViewModel viewModel) return;
            if (viewModel.SelectedPack is null || viewModel.SelectedPack.Mappings.Count == 0) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null) return;

            var suggestedFileName = string.IsNullOrWhiteSpace(viewModel.SelectedPack.Name)
                ? "TexturePack.zip"
                : $"{viewModel.SelectedPack.Name}.zip";

            var saveFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export Texture Pack",
                SuggestedFileName = suggestedFileName,
                DefaultExtension = "zip",
                FileTypeChoices =
                [
                    new FilePickerFileType("ZIP Archive")
                    {
                        Patterns = ["*.zip"]
                    }
                ]
            });

            if (saveFile is null)
                return;

            viewModel.ExportPack(saveFile.Path.LocalPath);
        }

        /// <summary>
        /// Handles the click event for deleting a texture mapping.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeleteMappingClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TexturePacksViewModel viewModel) return;
            if (sender is not Button button) return;
            if (button.Tag is not TextureMapping mapping) return;

            viewModel.SelectedMapping = mapping;
            viewModel.DeleteMappingCommand.Execute(null);
        }
    }

}
