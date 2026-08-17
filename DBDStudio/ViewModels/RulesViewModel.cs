using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DBDStudio.Interfaces;
using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Interfaces.Rules;
using DBDStudio.Models;
using DBDStudio.Models.Rules;

namespace DBDStudio.ViewModels
{
    public sealed class RulesViewModel : ViewModelBase
    {
        private readonly IRuleService _ruleService;
        private readonly ITexturePackService _texturePackService;
        private readonly IBodySlideService _bodySlideService;
        private readonly IRaceMenuPresetService _raceMenuPresetService;
        private Rule? _selectedRule;
        private string _raceMenuAssignmentWarning = string.Empty;
        private string? _selectedTextureCandidateToAdd;
        private string? _selectedBodySlideCandidateToAdd;

        private readonly RelayCommand _duplicateRuleCommand;
        private readonly RelayCommand _deleteRuleCommand;
        private readonly RelayCommand _addTextureCandidateCommand;
        private readonly RelayCommand _addBodySlideCandidateCommand;

        public ObservableCollection<Rule> Rules { get; } = [];
        public ObservableCollection<string> AvailableTexturePacks => [.. _texturePackService.TexturePacks.Select(p => p.Name)];
        public ObservableCollection<string> AvailableBodySlidePresets => [.. _bodySlideService.Presets.Select(p => p.Preset)];
        public ObservableCollection<string> AvailableRaceMenuPresets => [.. _raceMenuPresetService.Presets.Select(p => p.Name)];
        public ObservableCollection<ConditionType> AvailableConditionTypes { get; } = new(Enum.GetValues<ConditionType>());
        public ObservableCollection<string> ConflictWarnings { get; } = [];

        public IFormDatabase FormDatabase { get; }

        public Rule? SelectedRule
        {
            get => _selectedRule;
            set
            {
                if (!SetField(ref _selectedRule, value))
                    return;

                SelectedTextureCandidateToAdd = null;
                SelectedBodySlideCandidateToAdd = null;
                UpdateRaceMenuWarning();
                RefreshCommandStates();
            }
        }

        public string? SelectedTextureCandidateToAdd
        {
            get => _selectedTextureCandidateToAdd;
            set
            {
                if (SetField(ref _selectedTextureCandidateToAdd, value))
                    _addTextureCandidateCommand.RaiseCanExecuteChanged();
            }
        }

        public string? SelectedBodySlideCandidateToAdd
        {
            get => _selectedBodySlideCandidateToAdd;
            set
            {
                if (SetField(ref _selectedBodySlideCandidateToAdd, value))
                    _addBodySlideCandidateCommand.RaiseCanExecuteChanged();
            }
        }

        public string RaceMenuAssignmentWarning
        {
            get => _raceMenuAssignmentWarning;
            private set => SetField(ref _raceMenuAssignmentWarning, value);
        }

        public ICommand AddRuleCommand { get; }
        public ICommand DuplicateRuleCommand => _duplicateRuleCommand;
        public ICommand DeleteRuleCommand => _deleteRuleCommand;
        public ICommand AddTextureCandidateCommand => _addTextureCandidateCommand;
        public ICommand RemoveTextureCandidateCommand { get; }
        public ICommand AddBodySlideCandidateCommand => _addBodySlideCandidateCommand;
        public ICommand RemoveBodySlideCandidateCommand { get; }

        public RulesViewModel(
            IRuleService ruleService,
            ITexturePackService texturePackService,
            IBodySlideService bodySlideService,
            IRaceMenuPresetService raceMenuPresetService,
            IFormDatabase formDatabase)
        {
            _ruleService = ruleService;
            _texturePackService = texturePackService;
            _bodySlideService = bodySlideService;
            _raceMenuPresetService = raceMenuPresetService;
            FormDatabase = formDatabase;

            AddRuleCommand = new RelayCommand(AddRule);
            _duplicateRuleCommand = new RelayCommand(DuplicateRule, () => SelectedRule is not null);
            _deleteRuleCommand = new RelayCommand(DeleteRule, () => SelectedRule is not null);
            _addTextureCandidateCommand = new RelayCommand(AddTextureCandidate, CanAddTextureCandidate);
            RemoveTextureCandidateCommand = new RelayCommand<string>(RemoveTextureCandidate, candidate => SelectedRule is not null && !string.IsNullOrWhiteSpace(candidate));
            _addBodySlideCandidateCommand = new RelayCommand(AddBodySlideCandidate, CanAddBodySlideCandidate);
            RemoveBodySlideCandidateCommand = new RelayCommand<string>(RemoveBodySlideCandidate, candidate => SelectedRule is not null && !string.IsNullOrWhiteSpace(candidate));

            foreach (var rule in _ruleService.GetRules())
            {
                AttachRule(rule);
                Rules.Add(rule);
            }

            SelectedRule = Rules.Count > 0 ? Rules[0] : null;
            RefreshConflictWarnings();
        }

        private void AddRule()
        {
            var baseName = "New Rule";
            var uniqueName = CreateUniqueRuleName(baseName);
            var rule = new Rule { Name = uniqueName };
            _ruleService.Add(rule);

            var addedRule = _ruleService.GetRules().Last();
            AttachRule(addedRule);
            Rules.Add(addedRule);
            SelectedRule = addedRule;
            RefreshConflictWarnings();
        }

        private void DuplicateRule()
        {
            if (SelectedRule is null)
                return;

            var copy = SelectedRule.DeepClone();
            copy.Name = CreateUniqueCopyName(SelectedRule.Name);
            _ruleService.Add(copy);

            var addedRule = _ruleService.GetRules().Last();
            AttachRule(addedRule);
            Rules.Add(addedRule);
            SelectedRule = addedRule;
            RefreshConflictWarnings();
        }

        private void DeleteRule()
        {
            if (SelectedRule is null)
                return;

            var index = Rules.IndexOf(SelectedRule);
            DetachRule(SelectedRule);
            _ruleService.Remove(SelectedRule);
            Rules.Remove(SelectedRule);

            SelectedRule = Rules.Count > 0 ? Rules[Math.Max(0, index - 1)] : null;
            RefreshConflictWarnings();
        }

        private bool CanAddTextureCandidate()
        {
            return SelectedRule is not null
                && !string.IsNullOrWhiteSpace(SelectedTextureCandidateToAdd)
                && !SelectedRule.TextureCandidates.Contains(SelectedTextureCandidateToAdd, StringComparer.Ordinal);
        }

        private void AddTextureCandidate()
        {
            if (!CanAddTextureCandidate() || SelectedRule is null || SelectedTextureCandidateToAdd is null)
                return;

            SelectedRule.TextureCandidates.Add(SelectedTextureCandidateToAdd);
            SelectedTextureCandidateToAdd = null;
        }

        private void RemoveTextureCandidate(string? candidate)
        {
            if (SelectedRule is null || string.IsNullOrWhiteSpace(candidate))
                return;

            SelectedRule.TextureCandidates.Remove(candidate);
            _addTextureCandidateCommand.RaiseCanExecuteChanged();
        }

        private bool CanAddBodySlideCandidate()
        {
            return SelectedRule is not null
                && !string.IsNullOrWhiteSpace(SelectedBodySlideCandidateToAdd)
                && !SelectedRule.BodySlideCandidates.Contains(SelectedBodySlideCandidateToAdd, StringComparer.Ordinal);
        }

        private void AddBodySlideCandidate()
        {
            if (!CanAddBodySlideCandidate() || SelectedRule is null || SelectedBodySlideCandidateToAdd is null)
                return;

            SelectedRule.BodySlideCandidates.Add(SelectedBodySlideCandidateToAdd);
            SelectedBodySlideCandidateToAdd = null;
        }

        private void RemoveBodySlideCandidate(string? candidate)
        {
            if (SelectedRule is null || string.IsNullOrWhiteSpace(candidate))
                return;

            SelectedRule.BodySlideCandidates.Remove(candidate);
            _addBodySlideCandidateCommand.RaiseCanExecuteChanged();
        }

        private void AttachRule(Rule rule)
        {
            rule.Conditions.CollectionChanged += OnRuleConditionsCollectionChanged;
            foreach (var condition in rule.Conditions)
                condition.PropertyChanged += OnConditionChanged;
        }

        private void DetachRule(Rule rule)
        {
            rule.Conditions.CollectionChanged -= OnRuleConditionsCollectionChanged;
            foreach (var condition in rule.Conditions)
                condition.PropertyChanged -= OnConditionChanged;
        }

        private void OnRuleConditionsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null) {
                foreach (var oldItem in e.OldItems.OfType<Condition>()) {
                    oldItem.PropertyChanged -= OnConditionChanged;
                }
            }

            if (e.NewItems is not null) {
                foreach (var newItem in e.NewItems.OfType<Condition>()) {
                    newItem.PropertyChanged += OnConditionChanged;
                }
            }

            UpdateRaceMenuWarning();
        }

        private void OnConditionChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(Condition.ConditionType) or nameof(Condition.Operator) or nameof(Condition.Conjunction)) {
                UpdateRaceMenuWarning();
            }
        }

        private void UpdateRaceMenuWarning()
        {
            if (SelectedRule is null || string.IsNullOrWhiteSpace(SelectedRule.RaceMenuCandidate))
            {
                RaceMenuAssignmentWarning = string.Empty;
                return;
            }

            var hasReferenceCondition = SelectedRule.Conditions.Any(c => c.ConditionType == ConditionType.IsReference);
            var hasNpcCondition = SelectedRule.Conditions.Any(c => c.ConditionType == ConditionType.IsNPC);

            RaceMenuAssignmentWarning = hasReferenceCondition || hasNpcCondition
                ? string.Empty
                : "RaceMenu assignments work best with IsReference or IsNPC conditions.";
        }

        private void RefreshConflictWarnings()
        {
            ConflictWarnings.Clear();
            var groupedByName = Rules
                .GroupBy(rule => rule.Name, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1);

            foreach (var duplicate in groupedByName)
                ConflictWarnings.Add($"Duplicate rule name detected: {duplicate.Key}");

            if (ConflictWarnings.Count == 0)
                ConflictWarnings.Add("No obvious naming conflicts found.");
        }

        private string CreateUniqueRuleName(string baseName)
        {
            if (!Rules.Any(r => string.Equals(r.Name, baseName, StringComparison.OrdinalIgnoreCase)))
                return baseName;

            var index = 2;
            while (true)
            {
                var candidate = $"{baseName} {index}";
                if (!Rules.Any(r => string.Equals(r.Name, candidate, StringComparison.OrdinalIgnoreCase)))
                    return candidate;

                index++;
            }
        }

        private string CreateUniqueCopyName(string originalName)
        {
            var firstCopyName = $"{originalName} (Copy)";
            if (!Rules.Any(r => string.Equals(r.Name, firstCopyName, StringComparison.OrdinalIgnoreCase)))
                return firstCopyName;

            var index = 2;
            while (true)
            {
                var name = $"{originalName} (Copy {index})";
                if (!Rules.Any(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase)))
                    return name;

                index++;
            }
        }

        private void RefreshCommandStates()
        {
            _duplicateRuleCommand.RaiseCanExecuteChanged();
            _deleteRuleCommand.RaiseCanExecuteChanged();
            _addTextureCandidateCommand.RaiseCanExecuteChanged();
            _addBodySlideCandidateCommand.RaiseCanExecuteChanged();
            if (RemoveTextureCandidateCommand is RelayCommand<string> removeTexture)
                removeTexture.RaiseCanExecuteChanged();
            if (RemoveBodySlideCandidateCommand is RelayCommand<string> removeBodySlide)
                removeBodySlide.RaiseCanExecuteChanged();
        }
    }
}
