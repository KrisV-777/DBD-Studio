using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models.Textures;
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
            if (DataContext is not TexturePacksViewModel viewModel)
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
                return;

            var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
                Title = "Select Texture Root Folder",
                AllowMultiple = false
            });

            if (result.Count > 0) {
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
            if (DataContext is not TexturePacksViewModel viewModel)
                return;
            if (sender is not Button button)
                return;
            if (button.Tag is not TextureMapping mapping)
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
                return;

            var result = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
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
            if (DataContext is not TexturePacksViewModel viewModel)
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
                return;

            var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
                Title = "Select Texture Root Folder",
                AllowMultiple = false
            });

            if (result.Count > 0) {
                var selectedFolder = result[0].Path.LocalPath;
                viewModel.PopulatePackFromFolder(selectedFolder, viewModel.SelectedPack);
            }
        }

        private async void OnAddMappingClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TexturePacksViewModel viewModel)
                return;
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
            if (DataContext is not TexturePacksViewModel viewModel)
                return;
            if (viewModel.SelectedPack is null || viewModel.SelectedPack.NumMappings == 0)
                return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
                return;

            var suggestedFileName = string.IsNullOrWhiteSpace(viewModel.SelectedPack.Name)
                ? "TexturePack.zip"
                : $"{viewModel.SelectedPack.Name}.zip";

            var saveFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
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
            if (DataContext is not TexturePacksViewModel viewModel)
                return;
            if (sender is not Button button)
                return;
            if (button.Tag is not TextureMapping mapping)
                return;

            viewModel.SelectedMapping = mapping;
            viewModel.RemoveMappingCommand.Execute(null);
        }

        private async void OnDeleteSelectedPackClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TexturePacksViewModel viewModel)
                return;
            var selectedPack = viewModel.SelectedPack;

            System.Diagnostics.Debug.Assert(selectedPack is not null);
            System.Diagnostics.Debug.Assert(selectedPack.Is(TexturePackState.Ephemeral));

            if (selectedPack.NumMappings > 0) {
                var shouldDelete = await ShowConfirmationDialogAsync(
                    "Delete Texture Pack",
                    $"'{selectedPack.Name}' contains {selectedPack.NumMappings} mapping(s).\n\nDelete this workspace pack anyway?",
                    "Delete",
                    "Cancel");

                if (!shouldDelete) {
                    return;
                }
            }

            viewModel.DeletePackCommand.Execute(null);
        }

        private void OnResetSelectedPackClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not TexturePacksViewModel viewModel)
                return;
            System.Diagnostics.Debug.Assert(viewModel.SelectedPack is not null);
            System.Diagnostics.Debug.Assert(viewModel.SelectedPack.Is(TexturePackState.Modified));

            viewModel.ResetPackCommand.Execute(null);
        }

        private async Task<bool> ShowConfirmationDialogAsync(
            string title,
            string message,
            string confirmText,
            string cancelText)
        {
            if (TopLevel.GetTopLevel(this) is not Window owner)
                return false;

            var dialog = new Window {
                Title = title,

                // Give the dialog a sensible width, but let the height
                // be determined naturally by its content.
                Width = 400,
                SizeToContent = SizeToContent.Height,

                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false,

                // Let Windows/Avalonia provide the normal window frame.
                WindowDecorations = WindowDecorations.Full
            };

            var cancelButton = new Button {
                Content = cancelText,
                MinWidth = 90,
                IsCancel = true
            };

            var confirmButton = new Button {
                Content = confirmText,
                MinWidth = 90,
                IsDefault = true
            };

            cancelButton.Click += (_, _) => dialog.Close(false);
            confirmButton.Click += (_, _) => dialog.Close(true);

            dialog.Content = new StackPanel {
                Spacing = 18,
                Margin = new Thickness(24),
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14
                    },

                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 8,
                        Children =
                        {
                            cancelButton,
                            confirmButton
                        }
                    }
                }
            };

            return await dialog.ShowDialog<bool>(owner);
        }
    }

}
