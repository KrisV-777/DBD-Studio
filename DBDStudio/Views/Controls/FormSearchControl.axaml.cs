using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DBDStudio.Interfaces;
using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Models;

namespace DBDStudio.Views.Controls
{
    public partial class FormSearchControl : UserControl
    {
        public static readonly StyledProperty<IFormDatabase?> FormDatabaseProperty =
            AvaloniaProperty.Register<FormSearchControl, IFormDatabase?>(nameof(FormDatabase));

        public static readonly StyledProperty<FormReference?> SelectedFormReferenceProperty =
            AvaloniaProperty.Register<FormSearchControl, FormReference?>(nameof(SelectedFormReference));

        public static readonly StyledProperty<string> QueryTextProperty =
            AvaloniaProperty.Register<FormSearchControl, string>(nameof(QueryText), string.Empty);

        public FormSearchControl()
        {
            InitializeComponent();
        }

        public IFormDatabase? FormDatabase
        {
            get => GetValue(FormDatabaseProperty);
            set => SetValue(FormDatabaseProperty, value);
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
            if (FormDatabase is null)
                return;

            // foreach (var record in FormDatabase.Search(QueryText))
            //     SearchResults.Add(record);
        }
    }
}
