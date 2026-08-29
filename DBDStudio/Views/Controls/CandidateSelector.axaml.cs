using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using DBDStudio.Models.Component;

namespace DBDStudio.Views.Controls
{
    public partial class CandidateSelector : UserControl, INotifyPropertyChanged
    {
        public static readonly StyledProperty<IEnumerable<Candidate>?> AvailableCandidatesProperty =
            AvaloniaProperty.Register<CandidateSelector, IEnumerable<Candidate>?>(nameof(AvailableCandidates));

        public static readonly StyledProperty<ObservableCollection<Candidate>?> SelectedCandidatesProperty =
            AvaloniaProperty.Register<CandidateSelector, ObservableCollection<Candidate>?>(nameof(SelectedCandidates));

        public static readonly StyledProperty<bool> IsExpandedProperty =
            AvaloniaProperty.Register<CandidateSelector, bool>(nameof(IsExpanded), false);

        public static readonly StyledProperty<double> ExpansionRotationProperty =
            AvaloniaProperty.Register<CandidateSelector, double>(nameof(ExpansionRotation), 0d);

        private readonly ObservableCollection<CandidateRowViewModel> _rows = [];
        private ObservableCollection<Candidate>? _lastSelectedCandidates;

        public CandidateSelector()
        {
            InitializeComponent();
            Rows = _rows;
            UpdateRows();
        }

        public IEnumerable<Candidate>? AvailableCandidates
        {
            get => GetValue(AvailableCandidatesProperty);
            set => SetValue(AvailableCandidatesProperty, value);
        }

        public ObservableCollection<Candidate>? SelectedCandidates
        {
            get => GetValue(SelectedCandidatesProperty);
            set
            {
                if (_lastSelectedCandidates == value)
                    return;

                if (_lastSelectedCandidates is not null)
                    _lastSelectedCandidates.CollectionChanged -= OnSelectedCandidatesChanged;

                SetValue(SelectedCandidatesProperty, value);
                _lastSelectedCandidates = value;

                if (_lastSelectedCandidates is not null)
                    _lastSelectedCandidates.CollectionChanged += OnSelectedCandidatesChanged;

                UpdateRows();
            }
        }

        public bool IsExpanded
        {
            get => GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, value);
        }

        public double ExpansionRotation
        {
            get => GetValue(ExpansionRotationProperty);
            set => SetValue(ExpansionRotationProperty, value);
        }

        public string SelectedSummaryText => BuildSelectedSummaryText();

        public ObservableCollection<CandidateRowViewModel> Rows { get; }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == AvailableCandidatesProperty || change.Property == SelectedCandidatesProperty) {
                UpdateRows();
            }

            if (change.Property == IsExpandedProperty) {
                ExpansionRotation = IsExpanded ? 180d : 0d;
            }
        }

        private void OnSelectedCandidatesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateRows();
        }

        private void UpdateRows()
        {
            _rows.Clear();

            if (AvailableCandidates is null)
                return;

            var selectedNames = SelectedCandidates is null
                ? new HashSet<string>(StringComparer.Ordinal)
                : new HashSet<string>(SelectedCandidates.Select(candidate => candidate.Name), StringComparer.Ordinal);

            foreach (var candidate in AvailableCandidates) {
                var row = new CandidateRowViewModel {
                    Candidate = candidate,
                    IsSelected = selectedNames.Contains(candidate.Name),
                    IsExclusive = candidate.IsExclusive
                };
                _rows.Add(row);
            }

            NotifyPropertyChanged(nameof(SelectedSummaryText));
        }

        private void OnToggleExpandedClick(object? sender, RoutedEventArgs e) => IsExpanded = !IsExpanded;

        private void OnRowClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not CandidateRowViewModel row || SelectedCandidates is null)
                return;

            var existing = SelectedCandidates.FirstOrDefault(
                candidate => string.Equals(candidate.Name, row.Candidate.Name, StringComparison.Ordinal));
            if (existing is not null) {
                SelectedCandidates.Remove(existing);
            } else {
                if (row.IsRandom) {
                    SelectedCandidates.Clear();
                } else {
                    var randomCandidate = SelectedCandidates.FirstOrDefault(candidate => candidate.Name == "Any");
                    if (randomCandidate is not null) {
                        SelectedCandidates.Remove(randomCandidate);
                    }
                }
                SelectedCandidates.Add(new Candidate {
                    Name = row.Candidate.Name,
                    IsExclusive = row.Candidate.IsExclusive
                });
            }

            UpdateRows();
        }

        private void OnExclusiveToggleClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton toggleButton)
                return;

            if (toggleButton.DataContext is not CandidateRowViewModel row)
                return;

            var nextValue = toggleButton.IsChecked == true;
            row.Candidate.IsExclusive = nextValue;
            row.IsExclusive = nextValue;

            if (SelectedCandidates is not null) {
                var selected = SelectedCandidates.FirstOrDefault(
                    candidate => string.Equals(candidate.Name, row.Candidate.Name, StringComparison.Ordinal));
                selected?.IsExclusive = nextValue;
            }
        }

        private string BuildSelectedSummaryText()
        {
            if (SelectedCandidates is null || SelectedCandidates.Count == 0)
                return "Select candidates";

            return string.Join(", ", SelectedCandidates.Select(candidate => candidate.Name));
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public sealed class CandidateRowViewModel : INotifyPropertyChanged
        {
            private bool _isSelected;
            private bool _isExclusive;

            public Candidate Candidate { get; set; } = new();

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    if (_isSelected == value)
                        return;

                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }

            public bool IsExclusive
            {
                get => _isExclusive;
                set
                {
                    if (_isExclusive == value)
                        return;

                    _isExclusive = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExclusive)));
                }
            }

            public bool IsRandom => Candidate.Name == "Any";

            public event PropertyChangedEventHandler? PropertyChanged;
        }
    }
}
