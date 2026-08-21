using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DBDStudio.Models;
using DBDStudio.ViewModels;

namespace DBDStudio.Views.Pages
{
    public partial class RulesPage : UserControl
    {
        public RulesPage()
        {
            InitializeComponent();
        }

        private async void OnSaveRuleClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not RulesViewModel viewModel || viewModel.SelectedRenderedRule is null)
                return;

            if (viewModel.SelectedRenderedRule.Is(ConstructState.Modified)) {
                viewModel.SaveRuleCommand.Execute(null);
                return;
            }

            if (viewModel.SelectedRenderedRule.Is(ConstructState.Ephemeral)) {
                await SaveAsAsync(viewModel);
            }
        }

        private async void OnSaveRuleAsClick(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not RulesViewModel viewModel || viewModel.SelectedRenderedRule is null)
                return;

            await SaveAsAsync(viewModel);
        }

        private async Task SaveAsAsync(RulesViewModel viewModel)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
                return;

            var selectedRule = viewModel.SelectedRule;
            if (selectedRule is null)
                return;

            var suggestedFileName = BuildSuggestedFileName(selectedRule.Name);
            var saveFile = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions {
                Title = "Save Rule As",
                SuggestedFileName = suggestedFileName,
                DefaultExtension = "json",
                FileTypeChoices =
                [
                    new FilePickerFileType("JSON File")
                    {
                        Patterns = ["*.json"]
                    }
                ]
            });

            if (saveFile is null)
                return;

            viewModel.SaveRuleAs(saveFile.Path.LocalPath);
        }

        private static string BuildSuggestedFileName(string ruleName)
        {
            var fallback = "rule";
            var fileName = string.IsNullOrWhiteSpace(ruleName) ? fallback : ruleName;

            foreach (var invalid in Path.GetInvalidFileNameChars()) {
                fileName = fileName.Replace(invalid, '_');
            }

            return fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? fileName
                : fileName + ".json";
        }
    }
}
