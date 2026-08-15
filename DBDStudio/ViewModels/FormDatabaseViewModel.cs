using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia.Threading;
using DBDStudio.Interfaces.Mutagen;
using DBDStudio.Models;
using Noggog;

namespace DBDStudio.ViewModels
{
    public sealed class FormDatabaseViewModel : ViewModelBase
    {
        private readonly IFormDatabase _database;
        private ObservableCollection<FormRecord> _filteredRecords = new();
        private string _pluginSearchText = string.Empty;
        private string _searchText = string.Empty;
        private bool _skippingPluginRefresh = false;
        private bool _hideEmptyPlugins = true;

        private ICommand? _enableAllPluginsCommand;
        private ICommand? _disableAllPluginsCommand;

        public FormDatabaseViewModel(IFormDatabase dataBase)
        {
            _database = dataBase;
            _database.DatabaseChanged += (s, e) =>
            {
                if (e.Type == DatabaseChangedEventArgs.DatabaseChangeType.PluginsAdded ||
                    e.Type == DatabaseChangedEventArgs.DatabaseChangeType.PluginsRemoved) {
                    Dispatcher.UIThread.Post(RefreshPluginList);
                }
            };
            _database.LoadDatabase();
            RefreshPluginList();
        }

        #region Properties

        public ObservableCollection<IPluginData> Plugins { get; } = [];

        public ObservableCollection<FormRecord> Records
        {
            get => _filteredRecords;
            private set => SetField(ref _filteredRecords, value);
        }

        public string PluginSearchText
        {
            get => _pluginSearchText;
            set
            {
                if (SetField(ref _pluginSearchText, value))
                {
                    RefreshPluginList();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    RefreshRecordList();
                }
            }
        }

        public bool HideEmptyPlugins
        {
            get => _hideEmptyPlugins;
            set
            {
                if (SetField(ref _hideEmptyPlugins, value))
                {
                    RefreshPluginList();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand EnableAllPluginsCommand =>
            _enableAllPluginsCommand ??= new RelayCommand(() =>
            {
                _skippingPluginRefresh = true;
                try {
                    _database.Plugins.ForEach(p => p.IsEnabled = true);
                } finally {
                    _skippingPluginRefresh = false;
                }
                RefreshPluginList();
            });

        public ICommand DisableAllPluginsCommand =>
            _disableAllPluginsCommand ??= new RelayCommand(() =>
            {
                _skippingPluginRefresh = true;
                try {
                    _database.Plugins.ForEach(p => p.IsEnabled = false);
                } finally {
                    _skippingPluginRefresh = false;
                }
                RefreshPluginList();
            });

        #endregion

        #region Private Methods

        private void RefreshPluginList()
        {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(RefreshPluginList);
                return;
            }

            var filtered = _database.Plugins
                .Where(p =>
                {
                    p.PropertyChanged -= OnPluginPropertyChanged;
                    p.PropertyChanged += OnPluginPropertyChanged;
                    return !HideEmptyPlugins || p.Records.Any();
                })
                .Where(p => string.IsNullOrEmpty(_pluginSearchText) || p.PluginName.Contains(_pluginSearchText, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(p => p.PluginName);

            Plugins.Clear();
            Plugins.AddRange(filtered);
            RefreshRecordList();
        }

        private void OnPluginPropertyChanged(object? s, PropertyChangedEventArgs e)
        {
            if (_skippingPluginRefresh)
                return;

            if (e.PropertyName == nameof(IPluginData.IsEnabled) ||
                e.PropertyName == nameof(IPluginData.LoadState))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (e.PropertyName == nameof(IPluginData.LoadState) && HideEmptyPlugins) {
                        RefreshPluginList();
                        return;
                    }
                    RefreshRecordList();
                });
            }
        }

        private void RefreshRecordList()
        {
            if (!Dispatcher.UIThread.CheckAccess()) {
                Dispatcher.UIThread.Post(RefreshRecordList);
                return;
            }

            var observableRecords = new ObservableCollection<FormRecord>();

            Plugins
                .Where(p => p.IsEnabled)
                .SelectMany(p => p.Records)
                .Where(r => string.IsNullOrEmpty(_searchText) ||
                    r.Name.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    r.EditorId.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    r.FormId.ToString().Contains(_searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    r.Plugin.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase) ||
                    r.RecordType.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase))
                .ToList()
                .ForEach(observableRecords.Add);

            Records = observableRecords;
        }

        #endregion
    }
}
