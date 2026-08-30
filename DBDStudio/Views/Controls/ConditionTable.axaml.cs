using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Interfaces.Rules;
using DBDStudio.Models.Component.Condition;
using DBDStudio.ViewModels;

namespace DBDStudio.Views.Controls
{
    public partial class ConditionTable : UserControl
    {
        public static readonly StyledProperty<ObservableCollection<ICondition>?> ConditionsProperty =
            AvaloniaProperty.Register<ConditionTable, ObservableCollection<ICondition>?>(nameof(Conditions));

        public static readonly StyledProperty<ICondition?> SelectedConditionProperty =
            AvaloniaProperty.Register<ConditionTable, ICondition?>(nameof(SelectedCondition));

        public static readonly StyledProperty<IFormDatabase?> FormDatabaseProperty =
            AvaloniaProperty.Register<ConditionTable, IFormDatabase?>(nameof(FormDatabase));

        private static readonly IReadOnlyList<ConditionType> ConditionTypes = Enum.GetValues<ConditionType>();

        public ConditionTable()
        {
            InitializeComponent();
        }

        public ObservableCollection<ICondition>? Conditions
        {
            get => GetValue(ConditionsProperty);
            set => SetValue(ConditionsProperty, value);
        }

        public ICondition? SelectedCondition
        {
            get => GetValue(SelectedConditionProperty);
            set => SetValue(SelectedConditionProperty, value);
        }

        public IFormDatabase? FormDatabase
        {
            get => GetValue(FormDatabaseProperty);
            set => SetValue(FormDatabaseProperty, value);
        }

        public IReadOnlyList<ConditionType> AvailableConditionTypes => ConditionTypes;
        public IReadOnlyList<string> AvailableOperatorSymbols => Condition.OperatorSymbols;
        public IReadOnlyList<string> AvailableConjunctionLabels => Condition.ConjunctionLabels;

        private void OnAddConditionClick(object? sender, RoutedEventArgs e)
        {
            var conditions = ResolveConditions();
            if (conditions is null)
                return;

            ICondition condition = new Condition {
                ConditionType = AvailableConditionTypes.Count > 0 ? AvailableConditionTypes[0] : ConditionType.GetIsReference,
                Operator = Operator.Equals,
                Comparator = 0f,
                Conjunction = Conjunction.And
            };

            conditions.Add(condition);
            SelectedCondition = condition;
        }

        private void OnMoveUpClick(object? sender, RoutedEventArgs e)
        {
            if (!TryResolveRowContext(sender, out var conditions, out var condition))
                return;

            var index = conditions.IndexOf(condition);
            if (index > 0) {
                conditions.RemoveAt(index);
                conditions.Insert(index - 1, condition);
                SelectedCondition = condition;
            }
        }

        private void OnMoveDownClick(object? sender, RoutedEventArgs e)
        {
            if (!TryResolveRowContext(sender, out var conditions, out var condition))
                return;

            var index = conditions.IndexOf(condition);
            if (index >= 0 && index < conditions.Count - 1) {
                conditions.RemoveAt(index);
                conditions.Insert(index + 1, condition);
                SelectedCondition = condition;
            }
        }

        private void OnDuplicateClick(object? sender, RoutedEventArgs e)
        {
            if (!TryResolveRowContext(sender, out var conditions, out var condition))
                return;

            var index = conditions.IndexOf(condition);
            if (index < 0)
                return;

            var copy = condition.Copy();
            conditions.Insert(index + 1, copy);
            SelectedCondition = copy;
        }

        private void OnDeleteClick(object? sender, RoutedEventArgs e)
        {
            if (!TryResolveRowContext(sender, out var conditions, out var condition))
                return;

            var index = conditions.IndexOf(condition);
            if (index < 0)
                return;

            conditions.RemoveAt(index);
            if (conditions.Count == 0) {
                SelectedCondition = null;
                return;
            }

            var nextIndex = Math.Min(index, conditions.Count - 1);
            SelectedCondition = conditions[nextIndex];
        }

        private bool TryResolveRowContext(object? sender, out ObservableCollection<ICondition> conditions, out ICondition condition)
        {
            conditions = null!;
            condition = null!;

            var resolvedConditions = ResolveConditions();
            if (resolvedConditions is null || sender is not Button button || button.Tag is not ICondition rowCondition) {
                return false;
            }

            conditions = resolvedConditions;
            condition = rowCondition;
            return true;
        }

        private ObservableCollection<ICondition>? ResolveConditions()
        {
            if (Conditions is not null)
                return Conditions;

            if (DataContext is RulesViewModel viewModel)
                return viewModel.SelectedRule?.Conditions;

            return null;
        }
    }
}
