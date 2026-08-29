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

        public ObservableCollection<RuleConstruct> Rules { get; } = [];
        public ObservableCollection<Candidate> AvailableTexturePacks => [
            new Candidate { Name = "Any", IsExclusive = false },
            .. _texturePackService.TexturePacks.Select(p => new Candidate {
                Name = p.Name, IsExclusive = false
            })];
        public ObservableCollection<Candidate> AvailableBodySlidePresets => [
            new Candidate { Name = "Any", IsExclusive = false },
            .. _bodySlideService.Presets.Select(p => new Candidate {
                Name = p.Name, IsExclusive = false
            })];
        public ObservableCollection<string> AvailableRaceMenuPresets => [.. _raceMenuPresetService.Presets.Select(p => p.Name)];
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
            RemoveTextureCandidateCommand = new RelayCommand<Candidate>(RemoveTextureCandidate, candidate =>
                SelectedRule is not null && candidate is not null && !string.IsNullOrWhiteSpace(candidate.Name));
            _addBodySlideCandidateCommand = new RelayCommand(AddBodySlideCandidate, CanAddBodySlideCandidate);
            RemoveBodySlideCandidateCommand = new RelayCommand<Candidate>(RemoveBodySlideCandidate, candidate =>
                SelectedRule is not null && candidate is not null && !string.IsNullOrWhiteSpace(candidate.Name));

            _ruleService.Rules.CollectionChanged += OnRuleListChanged;

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
            return SelectedRule is not null
                && SelectedTextureCandidateToAdd is not null
                && !string.IsNullOrWhiteSpace(SelectedTextureCandidateToAdd.Name)
                && !SelectedRule.TextureCandidates.Any(c => string.Equals(c.Name, SelectedTextureCandidateToAdd.Name, StringComparison.Ordinal));
        }

        private void AddTextureCandidate()
        {
            if (!CanAddTextureCandidate() || SelectedRenderedRule is null || SelectedTextureCandidateToAdd is null)
                return;

            SelectedRenderedRule.Underlying.TextureCandidates.Add(new Candidate {
                Name = SelectedTextureCandidateToAdd.Name,
                IsExclusive = SelectedTextureCandidateToAdd.IsExclusive
            });
            SelectedTextureCandidateToAdd = null;
        }

        private void RemoveTextureCandidate(Candidate? candidate)
        {
            if (SelectedRenderedRule is null || candidate is null || string.IsNullOrWhiteSpace(candidate.Name))
                return;

            var existing = SelectedRenderedRule.Underlying.TextureCandidates
                .FirstOrDefault(c => string.Equals(c.Name, candidate.Name, StringComparison.Ordinal));
            if (existing is not null) {
                SelectedRenderedRule.Underlying.TextureCandidates.Remove(existing);
            }
            _addTextureCandidateCommand.RaiseCanExecuteChanged();
        }

        private bool CanAddBodySlideCandidate()
        {
            return SelectedRule is not null
                && SelectedBodySlideCandidateToAdd is not null
                && !string.IsNullOrWhiteSpace(SelectedBodySlideCandidateToAdd.Name)
                && !SelectedRule.BodySlideCandidates.Any(c => string.Equals(c.Name, SelectedBodySlideCandidateToAdd.Name, StringComparison.Ordinal));
        }

        private void AddBodySlideCandidate()
        {
            if (!CanAddBodySlideCandidate() || SelectedRenderedRule is null || SelectedBodySlideCandidateToAdd is null)
                return;

            SelectedRenderedRule.Underlying.BodySlideCandidates.Add(new Candidate {
                Name = SelectedBodySlideCandidateToAdd.Name,
                IsExclusive = SelectedBodySlideCandidateToAdd.IsExclusive
            });
            SelectedBodySlideCandidateToAdd = null;
        }

        private void RemoveBodySlideCandidate(Candidate? candidate)
        {
            if (SelectedRenderedRule is null || candidate is null || string.IsNullOrWhiteSpace(candidate.Name))
                return;

            var existing = SelectedRenderedRule.Underlying.BodySlideCandidates
                .FirstOrDefault(c => string.Equals(c.Name, candidate.Name, StringComparison.Ordinal));
            if (existing is not null) {
                SelectedRenderedRule.Underlying.BodySlideCandidates.Remove(existing);
            }
            _addBodySlideCandidateCommand.RaiseCanExecuteChanged();
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
            if (RemoveTextureCandidateCommand is RelayCommand<string> removeTexture) {
                removeTexture.RaiseCanExecuteChanged();
            }

            if (RemoveBodySlideCandidateCommand is RelayCommand<string> removeBodySlide) {
                removeBodySlide.RaiseCanExecuteChanged();
            }
        }
    }
}
