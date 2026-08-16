using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

using DBDStudio.Interfaces.Mutagen;
using Noggog;
using DBDStudio.Models.Mutagen;

namespace DBDStudio.Views.Controls
{
    public partial class FormSearchControl : UserControl
    {
        public static readonly StyledProperty<IFormDatabase?> FormDatabaseProperty =
            AvaloniaProperty.Register<FormSearchControl, IFormDatabase?>(nameof(FormDatabase));

        public static readonly StyledProperty<FormRecord?> SelectedFormRecordProperty =
            AvaloniaProperty.Register<FormSearchControl, FormRecord?>(nameof(SelectedFormRecord));

        public static readonly StyledProperty<FormType?> FilteredFormTypeProperty =
            AvaloniaProperty.Register<FormSearchControl, FormType?>(nameof(FilteredFormType));

        public static readonly DirectProperty<FormSearchControl, string> SearchTooltipProperty =
            AvaloniaProperty.RegisterDirect<FormSearchControl, string>(nameof(_searchTooltip), control => control._searchTooltip);

        public static readonly StyledProperty<string> QueryTextProperty =
            AvaloniaProperty.Register<FormSearchControl, string>(nameof(QueryText), string.Empty);

        private OverlayLayer? _overlayLayer;

        private FormSearchResults? _resultsOverlay;

        private IEnumerable<FormRecord> _allRecords = [];

        private string _searchTooltip = string.Empty;

        private bool _isApplyingSelection;

        public FormSearchControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #region Properties

        public IFormDatabase? FormDatabase
        {
            get => GetValue(FormDatabaseProperty);
            set => SetValue(FormDatabaseProperty, value);
        }

        public FormRecord? SelectedFormRecord
        {
            get => GetValue(SelectedFormRecordProperty);
            set => SetValue(SelectedFormRecordProperty, value);
        }

        public FormType? FilteredFormType
        {
            get => GetValue(FilteredFormTypeProperty);
            set => SetValue(FilteredFormTypeProperty, value);
        }

        public string QueryText
        {
            get => GetValue(QueryTextProperty);
            set => SetValue(QueryTextProperty, value);
        }

        public ObservableCollection<FormRecord> SearchResults { get; } = [];

        #endregion

        #region Lifecycle

        private void OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (FormDatabase is null) {
                Debug.WriteLine("FormSearchControl: FormDatabase is null on load.");
                return;
            }

            _allRecords = FormDatabase.Plugins
                .Where(plugin => plugin.IsEnabled)
                .SelectMany(plugin =>
                    FilteredFormType is not null && plugin.RecordsByFormKey.TryGetValue(FilteredFormType.Value, out var records)
                        ? records : plugin.Records)
                .OrderBy(record => record.Name)
                .ThenBy(record => record.Plugin)
                .ThenBy(record => record.FormId)
                .ToArray() ?? [];

            FindOverlayLayer();
        }

        private void OnUnloaded(object? sender, RoutedEventArgs e)
        {
            CloseResults();

            if (_resultsOverlay is not null) {
                _resultsOverlay.ResultSelected -= OnResultSelected;
                _overlayLayer?.Children.Remove(_resultsOverlay);
                _resultsOverlay = null;
            }

            _overlayLayer = null;
        }

        private void FindOverlayLayer() => _overlayLayer = OverlayLayer.GetOverlayLayer(this);

        #endregion

        #region SearchBox Events

        private void OnSearchBoxGotFocus(object? sender, FocusChangedEventArgs e)
        {
            if (_isApplyingSelection)
                return;

            Search();

            if (SearchResults.Count > 0 && !HasCommittedSelection())
                OpenResults();
            else
                CloseResults();
        }

        private void OnSearchBoxTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_isApplyingSelection)
                return;

            Search();

            if (SearchResults.Count > 0 && !string.IsNullOrWhiteSpace(QueryText) && !HasCommittedSelection()) {
                OpenResults();
                return;
            }

            CloseResults();
        }

        /// <summary>
        /// Handle user event when clicking outside of the search box. Delay commiting the selection until
        /// after the click event has been processed to avoid losing the selection when the user clicks on a result.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void OnSearchBoxLostFocus(object? sender, RoutedEventArgs e) =>
            Dispatcher.UIThread.Post(CheckFocusAfterSearchBoxLostFocus);

        /// <summary>
        /// Handle the actual lost focus event after the click event has been processed.
        /// Commit and close the overlay if the user is outside the search box and the results overlay.
        /// </summary>
        private void CheckFocusAfterSearchBoxLostFocus()
        {
            if (_isApplyingSelection || string.IsNullOrEmpty(QueryText))
                return;

            if (_resultsOverlay is not null &&
                _resultsOverlay.IsVisible &&
                (_resultsOverlay.IsPointerOver || SearchBox.IsFocused || IsKeyboardFocusWithin)) {
                return;
            }

            if (_resultsOverlay is null ||
                !_resultsOverlay.IsVisible) {
                return;
            }

            Commit();
        }

        private void OnSearchBoxKeyDown(object? sender, KeyEventArgs e)
        {
            if (_isApplyingSelection)
                return;

            switch (e.Key) {
            case Key.Enter:

                Commit();

                e.Handled = true;
                break;

            case Key.Escape:

                CloseResults();

                e.Handled = true;
                break;
            case Key.Down:
                // TODO: Move selection down in results list if it's open.
                break;
            case Key.Up:
                // TODO: Move selection up in results list if it's open.
                break;
            }
        }

        #endregion

        #region Results Overlay

        private void OpenResults()
        {
            if (_overlayLayer is null)
                FindOverlayLayer();

            if (_overlayLayer is null)
                return;

            if (_resultsOverlay is null) {
                _resultsOverlay = new FormSearchResults {
                    SearchResults = SearchResults
                };

                _resultsOverlay.ResultSelected += OnResultSelected;
                _overlayLayer.Children.Add(_resultsOverlay);
            }

            UpdateResultsOverlayPosition();

            _resultsOverlay.IsVisible = true;
        }

        private void CloseResults()
        {
            if (_resultsOverlay is null)
                return;

            _resultsOverlay.IsVisible = false;
        }

        private void OnResultSelected(object? sender, FormRecord record) => CommitRecord(record);

        private void UpdateResultsOverlayPosition()
        {
            if (_overlayLayer is null || _resultsOverlay is null) {
                return;
            }

            var point = SearchBox.TranslatePoint(
                new Point(0, SearchBox.Bounds.Height),
                _overlayLayer);

            if (point is null)
                return;

            const double spacing = 4;
            const double windowMargin = 8;

            var x = point.Value.X;
            var y = point.Value.Y + spacing;
            var width = SearchBox.Bounds.Width;
            var availableHeight = _overlayLayer.Bounds.Height - y - windowMargin;

            Canvas.SetLeft(_resultsOverlay, x);
            Canvas.SetTop(_resultsOverlay, y);

            _resultsOverlay.Width = width;
            _resultsOverlay.MaxHeight = Math.Max(0, availableHeight);
        }

        protected override void OnSizeChanged(SizeChangedEventArgs e)
        {
            base.OnSizeChanged(e);

            if (_resultsOverlay?.IsVisible == true)
                UpdateResultsOverlayPosition();
        }

        #endregion

        #region Searching

        private void Search()
        {
            SearchResults.Clear();

            _allRecords
                .Where(record => record.MatchQuery(QueryText))
                .Take(200)
                .ForEach(SearchResults.Add);
        }

        #endregion

        #region Commit / Validation

        private void Commit()
        {
            if (_isApplyingSelection)
                return;

            if (string.IsNullOrWhiteSpace(QueryText)) {
                CommitRecord(null);
                return;
            }
            CommitRecord(SearchResults.FirstOrDefault(SelectedFormRecord));
        }

        private void CommitRecord(FormRecord? record)
        {
            if (_isApplyingSelection)
                return;

            _isApplyingSelection = true;

            try {
                QueryText = record?.FormReference.ToString() ?? string.Empty;
                SetAndRaise(SearchTooltipProperty, ref _searchTooltip, record?.EditorId ?? string.Empty);

                if (!ReferenceEquals(SelectedFormRecord, record))
                    SelectedFormRecord = record;

                CloseResults();
            } finally {
                _isApplyingSelection = false;
            }
        }

        private bool HasCommittedSelection()
        {
            var committedReference = SelectedFormRecord?.FormReference.ToString();

            return committedReference is not null &&
                string.Equals(QueryText, committedReference, StringComparison.Ordinal);
        }

        #endregion
    }
}
