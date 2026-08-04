using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace DBDStudio.Views.Controls;

public partial class FilePathPickerControl : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<FilePathPickerControl, string?>(nameof(Text));

    public static readonly StyledProperty<string> PickerTitleProperty =
        AvaloniaProperty.Register<FilePathPickerControl, string>(nameof(PickerTitle), "Select Folder");

    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<FilePathPickerControl, string>(nameof(PlaceholderText), string.Empty);

    public static readonly StyledProperty<string> BrowseButtonTextProperty =
        AvaloniaProperty.Register<FilePathPickerControl, string>(nameof(BrowseButtonText), "Browse…");

    public FilePathPickerControl()
    {
        InitializeComponent();
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string PickerTitle
    {
        get => GetValue(PickerTitleProperty);
        set => SetValue(PickerTitleProperty, value);
    }

    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public string BrowseButtonText
    {
        get => GetValue(BrowseButtonTextProperty);
        set => SetValue(BrowseButtonTextProperty, value);
    }

    private async void OnBrowseClicked(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = PickerTitle, AllowMultiple = false });

        if (result.Count > 0)
            Text = result[0].Path.LocalPath;
    }
}
