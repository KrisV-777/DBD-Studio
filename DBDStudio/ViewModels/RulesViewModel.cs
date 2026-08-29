using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DBDStudio.Interfaces;
using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Interfaces.Rules;
using DBDStudio.Models;
using DBDStudio.Models.Component;
using DBDStudio.Models.Component.Condition;
using Noggog;

namespace DBDStudio.ViewModels
{
    public sealed class RulesViewModel : ViewModelBase
    {
        private readonly IRuleService _ruleService;
        private readonly ITexturePackService _texturePackService;
        private readonly IBodySlideService _bodySlideService;
        private readonly IRaceMenuPresetService _raceMenuPresetService;

        private RuleConstruct? _selectedRenderedRule;
        private string _raceMenuAssignmentWarning = string.Empty;
        private Candidate? _selectedTextureCandidateToAdd;
        private Candidate? _selectedBodySlideCandidateToAdd;

        private readonly RelayCommand _duplicateRuleCommand;
        private readonly RelayCommand _deleteRuleCommand;
        private readonly RelayCommand _resetRuleCommand;
        private readonly RelayCommand _saveRuleCommand;
        private readonly RelayCommand _addTextureCandidateCommand;
        private readonly RelayCommand _addBodySlideCandidateCommand;
        private readonly RelayCommand<Candidate> _removeTextureCandidateCommand;
        private readonly RelayCommand<Candidate> _removeBodySlideCandidateCommand;
        private readonly ObservableCollection<Candidate> _availableTexturePacks = [];
        private readonly ObservableCollection<Candidate> _availableBodySlidePresets = [];
        private readonly ObservableCollection<string> _availableRaceMenuPresets = [];

        public ObservableCollection<RuleConstruct> Rules { get; } = [];
        public ObservableCollection<Candidate> AvailableTexturePacks => _availableTexturePacks;
        public ObservableCollection<Candidate> AvailableBodySlidePresets => _availableBodySlidePresets;
        public ObservableCollection<string> AvailableRaceMenuPresets => _availableRaceMenuPresets;
        public ObservableCollection<ConditionType> AvailableConditionTypes { get; } = new(Enum.GetValues<ConditionType>());

        public IFormDatabase FormDatabase { get; }

        public RuleConstruct? SelectedRenderedRule
        {
            get => _selectedRenderedRule;
            set
            {
                if (ReferenceEquals(_selectedRenderedRule, value))
                    return;

                DetachSelectedRenderedRule(_selectedRenderedRule);
                _selectedRenderedRule = value;
                AttachSelectedRenderedRule(_selectedRenderedRule);
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedRule));
                OnPropertyChanged(nameof(SelectedRuleState));

                SelectedTextureCandidateToAdd = AvailableTexturePacks.First();
                SelectedBodySlideCandidateToAdd = AvailableBodySlidePresets.First();

                UpdateRaceMenuWarning();
                RefreshCommandStates();
            }
        }

        public Rule? SelectedRule => SelectedRenderedRule?.Underlying;

        public ConstructState SelectedRuleState => SelectedRenderedRule?.State ?? ConstructState.None;

        public Candidate? SelectedTextureCandidateToAdd
        {
            get => _selectedTextureCandidateToAdd;
            set
            {
                if (SetField(ref _selectedTextureCandidateToAdd, value))
                    _addTextureCandidateCommand.RaiseCanExecuteChanged();
            }
        }

        public Candidate? SelectedBodySlideCandidateToAdd
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
        public ICommand ResetRuleCommand => _resetRuleCommand;
        public ICommand SaveRuleCommand => _saveRuleCommand;
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

            AddRuleCommand = new RelayCommand(() => AddRule());
            _duplicateRuleCommand = new RelayCommand(DuplicateRule, () => SelectedRenderedRule is not null);
            _deleteRuleCommand = new RelayCommand(DeleteRule, () => SelectedRenderedRule?.Is(ConstructState.Ephemeral) ?? false);
            _resetRuleCommand = new RelayCommand(ResetRule, () => SelectedRenderedRule?.Is(ConstructState.Modified) ?? false);
            _saveRuleCommand = new RelayCommand(SaveRule, () => SelectedRenderedRule?.Is(ConstructState.Modified) ?? false);
            _addTextureCandidateCommand = new RelayCommand(AddTextureCandidate, CanAddTextureCandidate);
            _removeTextureCandidateCommand = new RelayCommand<Candidate>(RemoveTextureCandidate, candidate =>
                SelectedRule is not null && candidate is not null && !string.IsNullOrWhiteSpace(candidate.Name));
            RemoveTextureCandidateCommand = _removeTextureCandidateCommand;
            _addBodySlideCandidateCommand = new RelayCommand(AddBodySlideCandidate, CanAddBodySlideCandidate);
            _removeBodySlideCandidateCommand = new RelayCommand<Candidate>(RemoveBodySlideCandidate, candidate =>
                SelectedRule is not null && candidate is not null && !string.IsNullOrWhiteSpace(candidate.Name));
            RemoveBodySlideCandidateCommand = _removeBodySlideCandidateCommand;

            _ruleService.Rules.CollectionChanged += OnRuleListChanged;
            _texturePackService.TexturePacks.CollectionChanged += (_, _) => RefreshAvailableTexturePacks();
            _bodySlideService.Presets.CollectionChanged += (_, _) => RefreshAvailableBodySlidePresets();
            _raceMenuPresetService.Presets.CollectionChanged += (_, _) => RefreshAvailableRaceMenuPresets();

            RefreshAvailableTexturePacks();
            RefreshAvailableBodySlidePresets();
            RefreshAvailableRaceMenuPresets();

            Rules.AddRange(_ruleService.Rules);
            foreach (var rule in Rules) {
                AttachRule(rule);
            }

            SelectedRenderedRule = Rules.Count > 0 ? Rules[0] : null;
        }

        public void SaveRuleAs(string filePath)
        {
            if (SelectedRenderedRule is null)
                return;

            _ruleService.SaveAs(SelectedRenderedRule, filePath);
        }

        private void OnRuleListChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is null)
                    break;

                foreach (var newItem in e.NewItems.OfType<RuleConstruct>()) {
                    if (Rules.Contains(newItem))
                        continue;

                    AttachRule(newItem);
                    Rules.Add(newItem);
                    SelectedRenderedRule = newItem;
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems is null)
                    break;

                var removeIndex = Rules.IndexOf(e.OldItems.OfType<RuleConstruct>().First());
                if (removeIndex < 0)
                    break;

                DetachRule(Rules[removeIndex]);
                var nextSelection = Rules.Count <= 1 ? null : Rules[Math.Clamp(removeIndex, 1, Rules.Count - 1) - 1];
                var wasSelected = ReferenceEquals(SelectedRenderedRule, Rules[removeIndex]);
                Rules.RemoveAt(removeIndex);
                if (wasSelected) {
                    SelectedRenderedRule = nextSelection;
                }
                break;

            case NotifyCollectionChangedAction.Reset:
                var currentSelection = SelectedRenderedRule?.Uid;
                foreach (var rule in Rules) {
                    DetachRule(rule);
                }

                Rules.Clear();
                Rules.AddRange(_ruleService.Rules);
                foreach (var rule in Rules) {
                    AttachRule(rule);
                }

                SelectedRenderedRule = currentSelection is not null
                    ? Rules.FirstOrDefault(rule => rule.Uid == currentSelection)
                    : (Rules.Count > 0 ? Rules[0] : null);
                break;
            }

            OnPropertyChanged(nameof(SelectedRule));
            OnPropertyChanged(nameof(SelectedRuleState));
            RefreshCommandStates();
            UpdateRaceMenuWarning();
        }

        private void AttachSelectedRenderedRule(RuleConstruct? rule)
        {
            if (rule is null) {
                return;
            }

            rule.PropertyChanged += OnSelectedRenderedRulePropertyChanged;
        }

        private void DetachSelectedRenderedRule(RuleConstruct? rule)
        {
            if (rule is null) {
                return;
            }

            rule.PropertyChanged -= OnSelectedRenderedRulePropertyChanged;
        }

        private void OnSelectedRenderedRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not RuleConstruct || e.PropertyName != "State") {
                return;
            }

            OnPropertyChanged(nameof(SelectedRuleState));
            RefreshCommandStates();
        }

        private RuleConstruct AddRule() => _ruleService.EmplaceNew(null);

        private void DuplicateRule()
        {
            if (SelectedRenderedRule is null)
                return;

            var sourceRule = SelectedRenderedRule;
            var duplicate = _ruleService.EmplaceNew(sourceRule.Name);
            var duplicateName = duplicate.Name;
            duplicate.Underlying.Import(sourceRule.Underlying);
            duplicate.Underlying.Name = duplicateName;
        }

        private void DeleteRule()
        {
            if (SelectedRenderedRule is null)
                return;
            _ruleService.Remove(SelectedRenderedRule);
        }

        private void ResetRule()
        {
            if (SelectedRenderedRule is null)
                return;
            _ruleService.Reset(SelectedRenderedRule);
        }

        private void SaveRule()
        {
            if (SelectedRenderedRule is null)
                return;
            _ruleService.Save(SelectedRenderedRule);
        }

        private bool CanAddTextureCandidate()
        {
            return SelectedRule is not null && CanAddCandidate(SelectedRule.TextureCandidates, SelectedTextureCandidateToAdd);
        }

        private void AddTextureCandidate()
        {
            if (!CanAddTextureCandidate() || SelectedRenderedRule is null || SelectedTextureCandidateToAdd is null)
                return;

            AddCandidate(SelectedRenderedRule.Underlying.TextureCandidates, SelectedTextureCandidateToAdd);
            SelectedTextureCandidateToAdd = null;
        }

        private void RemoveTextureCandidate(Candidate? candidate)
        {
            if (SelectedRenderedRule is null || candidate is null || string.IsNullOrWhiteSpace(candidate.Name))
                return;

            RemoveCandidate(SelectedRenderedRule.Underlying.TextureCandidates, candidate);
            _addTextureCandidateCommand.RaiseCanExecuteChanged();
        }

        private bool CanAddBodySlideCandidate()
        {
            return SelectedRule is not null && CanAddCandidate(SelectedRule.BodySlideCandidates, SelectedBodySlideCandidateToAdd);
        }

        private void AddBodySlideCandidate()
        {
            if (!CanAddBodySlideCandidate() || SelectedRenderedRule is null || SelectedBodySlideCandidateToAdd is null)
                return;

            AddCandidate(SelectedRenderedRule.Underlying.BodySlideCandidates, SelectedBodySlideCandidateToAdd);
            SelectedBodySlideCandidateToAdd = null;
        }

        private void RemoveBodySlideCandidate(Candidate? candidate)
        {
            if (SelectedRenderedRule is null || candidate is null || string.IsNullOrWhiteSpace(candidate.Name))
                return;

            RemoveCandidate(SelectedRenderedRule.Underlying.BodySlideCandidates, candidate);
            _addBodySlideCandidateCommand.RaiseCanExecuteChanged();
        }

        private static bool CanAddCandidate(ObservableCollection<Candidate> existingCandidates, Candidate? candidateToAdd)
        {
            return candidateToAdd is not null
                && !string.IsNullOrWhiteSpace(candidateToAdd.Name)
                && !existingCandidates.Any(candidate => string.Equals(candidate.Name, candidateToAdd.Name, StringComparison.Ordinal));
        }

        private static void AddCandidate(ObservableCollection<Candidate> existingCandidates, Candidate candidateToAdd)
        {
            existingCandidates.Add(new Candidate {
                Name = candidateToAdd.Name,
                IsExclusive = candidateToAdd.IsExclusive
            });
        }

        private static void RemoveCandidate(ObservableCollection<Candidate> existingCandidates, Candidate candidateToRemove)
        {
            var existing = existingCandidates
                .FirstOrDefault(candidate => string.Equals(candidate.Name, candidateToRemove.Name, StringComparison.Ordinal));
            if (existing is not null) {
                existingCandidates.Remove(existing);
            }
        }

        private void RefreshAvailableTexturePacks()
        {
            _availableTexturePacks.Clear();
            _availableTexturePacks.Add(new Candidate { Name = "Any", IsExclusive = false });

            foreach (var texturePack in _texturePackService.TexturePacks) {
                _availableTexturePacks.Add(new Candidate {
                    Name = texturePack.Name,
                    IsExclusive = false
                });
            }
        }

        private void RefreshAvailableBodySlidePresets()
        {
            _availableBodySlidePresets.Clear();
            _availableBodySlidePresets.Add(new Candidate { Name = "Any", IsExclusive = false });

            foreach (var preset in _bodySlideService.Presets) {
                _availableBodySlidePresets.Add(new Candidate {
                    Name = preset.Name,
                    IsExclusive = false
                });
            }
        }

        private void RefreshAvailableRaceMenuPresets()
        {
            _availableRaceMenuPresets.Clear();
            foreach (var preset in _raceMenuPresetService.Presets) {
                _availableRaceMenuPresets.Add(preset.Name);
            }
        }

        private void AttachRule(RuleConstruct renderedRule)
        {
            renderedRule.Underlying.PropertyChanged += OnRulePropertyChanged;
            renderedRule.Underlying.TextureCandidates.CollectionChanged += OnCandidateCollectionChanged;
            renderedRule.Underlying.BodySlideCandidates.CollectionChanged += OnCandidateCollectionChanged;
            renderedRule.Underlying.Conditions.CollectionChanged += OnRuleConditionsCollectionChanged;
            foreach (var condition in renderedRule.Underlying.Conditions) {
                condition.PropertyChanged += OnConditionChanged;
            }
        }

        private void DetachRule(RuleConstruct renderedRule)
        {
            renderedRule.Underlying.PropertyChanged -= OnRulePropertyChanged;
            renderedRule.Underlying.TextureCandidates.CollectionChanged -= OnCandidateCollectionChanged;
            renderedRule.Underlying.BodySlideCandidates.CollectionChanged -= OnCandidateCollectionChanged;
            renderedRule.Underlying.Conditions.CollectionChanged -= OnRuleConditionsCollectionChanged;
            foreach (var condition in renderedRule.Underlying.Conditions) {
                condition.PropertyChanged -= OnConditionChanged;
            }
        }

        private void OnRulePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Rule rule) {
                return;
            }

            UpdateRaceMenuWarning();
        }

        private void OnCandidateCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateRaceMenuWarning();
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
            if (SelectedRule is null || string.IsNullOrWhiteSpace(SelectedRule.RaceMenuCandidate)) {
                RaceMenuAssignmentWarning = string.Empty;
                return;
            }

            var hasReferenceCondition = SelectedRule.Conditions
                .Where(c => c.ConditionType == ConditionType.IsReference)
                .Any(c => c.Values
                    .Where(v => v is ConditionValue.Form)
                    .Any(v => (v as ConditionValue.Form)!.Value?.FormReference.MaybeValid() ?? false));
            var hasNpcCondition = SelectedRule.Conditions
                .Where(c => c.ConditionType == ConditionType.IsNPC)
                .Any(c => c.Values
                    .Where(v => v is ConditionValue.Form)
                    .Any(v => (v as ConditionValue.Form)!.Value?.FormReference.MaybeValid() ?? false));

            if (!hasReferenceCondition && !hasNpcCondition) {
                RaceMenuAssignmentWarning = "RaceMenu assignments require IsReference or IsNPC conditions.";
                return;
            }

            var hasPlayerCondition = SelectedRule.Conditions
                .Any(c => c.Values
                    .Where(v => v is ConditionValue.Form)
                    .Any(v => (v as ConditionValue.Form)!.Value?.FormId is 0x14 or 0x20));
            var hasSexCondition = SelectedRule.Conditions.Any(c => c.ConditionType == ConditionType.IsSex);

            if (hasPlayerCondition && !hasSexCondition) {
                RaceMenuAssignmentWarning = "RaceMenu assignments need a IsSex condition to work with the player.";
                return;
            }

            RaceMenuAssignmentWarning = string.Empty;
        }

        private void RefreshCommandStates()
        {
            _duplicateRuleCommand.RaiseCanExecuteChanged();
            _deleteRuleCommand.RaiseCanExecuteChanged();
            _resetRuleCommand.RaiseCanExecuteChanged();
            _saveRuleCommand.RaiseCanExecuteChanged();
            _addTextureCandidateCommand.RaiseCanExecuteChanged();
            _addBodySlideCandidateCommand.RaiseCanExecuteChanged();
            _removeTextureCandidateCommand.RaiseCanExecuteChanged();
            _removeBodySlideCandidateCommand.RaiseCanExecuteChanged();
        }
    }
}
