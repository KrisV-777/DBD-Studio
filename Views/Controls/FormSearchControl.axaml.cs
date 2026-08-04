using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.Views.Controls;

public partial class FormSearchControl : UserControl
{
    public static readonly StyledProperty<IFormDatabaseService?> FormDatabaseServiceProperty =
        AvaloniaProperty.Register<FormSearchControl, IFormDatabaseService?>(nameof(FormDatabaseService));

    public static readonly StyledProperty<FormReference?> SelectedFormReferenceProperty =
        AvaloniaProperty.Register<FormSearchControl, FormReference?>(nameof(SelectedFormReference));

    public static readonly StyledProperty<string> QueryTextProperty =
        AvaloniaProperty.Register<FormSearchControl, string>(nameof(QueryText), string.Empty);

    public FormSearchControl()
    {
        InitializeComponent();
    }

    public IFormDatabaseService? FormDatabaseService
    {
        get => GetValue(FormDatabaseServiceProperty);
        set => SetValue(FormDatabaseServiceProperty, value);
    }

    public FormReference? SelectedFormReference
    {
        get => GetValue(SelectedFormReferenceProperty);
        set => SetValue(SelectedFormReferenceProperty, value);
    }

    public string QueryText
    {
        get => GetValue(QueryTextProperty);
        set => SetValue(QueryTextProperty, value);
    }

    public ObservableCollection<FormRecord> SearchResults { get; } = [];

    public FormRecord? SelectedFormRecord
    {
        get => _selectedFormRecord;
        set
        {
            _selectedFormRecord = value;
            SelectedFormReference = value?.FormReference;
        }
    }

    private FormRecord? _selectedFormRecord;

    private void OnSearchClicked(object? sender, RoutedEventArgs e)
    {
        SearchResults.Clear();
        if (FormDatabaseService is null)
            return;

        foreach (var record in FormDatabaseService.Search(QueryText))
            SearchResults.Add(record);
    }
}
