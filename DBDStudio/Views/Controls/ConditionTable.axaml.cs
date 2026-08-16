using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Interfaces.Rules;
using DBDStudio.Models.Rules;
using DBDStudio.ViewModels;

namespace DBDStudio.Views.Controls
{
    public partial class ConditionTable : UserControl
    {
        public static readonly StyledProperty<ObservableCollection<Condition>?> ConditionsProperty =
            AvaloniaProperty.Register<ConditionTable, ObservableCollection<Condition>?>(nameof(Conditions));

        public static readonly StyledProperty<Condition?> SelectedConditionProperty =
            AvaloniaProperty.Register<ConditionTable, Condition?>(nameof(SelectedCondition));

        public static readonly StyledProperty<IFormDatabase?> FormDatabaseProperty =
            AvaloniaProperty.Register<ConditionTable, IFormDatabase?>(nameof(FormDatabase));

        private static readonly IReadOnlyList<ConditionType> ConditionTypes = Enum.GetValues<ConditionType>();

        public ConditionTable()
        {
            InitializeComponent();
        }

        public ObservableCollection<Condition>? Conditions
        {
            get => GetValue(ConditionsProperty);
            set => SetValue(ConditionsProperty, value);
        }

        public Condition? SelectedCondition
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

            var condition = new Condition {
                ConditionType = AvailableConditionTypes.Count > 0 ? AvailableConditionTypes[0] : ConditionType.IsReference,
                Operator = Operator.Equals,
                Conjunction = Conjunction.And
            };

            conditions.Add(condition);
            SelectedCondition = condition;
        }

        private void OnMoveUpClick(object? sender, RoutedEventArgs e)
        {
            var conditions = ResolveConditions();
            if (sender is not Button button || button.Tag is not Condition condition || conditions is null)
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
            var conditions = ResolveConditions();
            if (sender is not Button button || button.Tag is not Condition condition || conditions is null)
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
            var conditions = ResolveConditions();
            if (sender is not Button button || button.Tag is not Condition condition || conditions is null)
                return;

            var index = conditions.IndexOf(condition);
            if (index < 0)
                return;

            var copy = condition.DeepClone();
            conditions.Insert(index + 1, copy);
            SelectedCondition = copy;
        }

        private void OnDeleteClick(object? sender, RoutedEventArgs e)
        {
            var conditions = ResolveConditions();
            if (sender is not Button button || button.Tag is not Condition condition || conditions is null)
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

        private ObservableCollection<Condition>? ResolveConditions()
        {
            if (Conditions is not null)
                return Conditions;

            if (DataContext is RulesViewModel viewModel)
                return viewModel.SelectedRule?.Conditions;

            return null;
        }
    }
}
