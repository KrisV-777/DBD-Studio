using System;
using System.Collections.ObjectModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using DBDStudio.Models;

namespace DBDStudio.Views.Controls
{
    public partial class FormSearchResults : UserControl
    {
        public static readonly StyledProperty<ObservableCollection<FormRecord>?> SearchResultsProperty =
            AvaloniaProperty.Register<FormSearchResults, ObservableCollection<FormRecord>?>(nameof(SearchResults));

        public FormSearchResults()
        {
            InitializeComponent();
        }

        public ObservableCollection<FormRecord>? SearchResults
        {
            get => GetValue(SearchResultsProperty);
            set => SetValue(SearchResultsProperty, value);
        }

        public event EventHandler<FormRecord>? ResultSelected;

        private void OnResultClicked(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.DataContext is not FormRecord record)
                return;

            ResultSelected?.Invoke(this, record);

            e.Handled = true;
        }
    }
}
