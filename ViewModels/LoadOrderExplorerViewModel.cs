using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using DBDStudio.Core.Interfaces;
using DBDStudio.Core.Models;

namespace DBDStudio.ViewModels;

public sealed class LoadOrderExplorerViewModel : ViewModelBase
{
    private readonly ILoadOrderService _loadOrderService;
    private string _searchText = string.Empty;
    private FormRecord? _selectedRecord;

    public LoadOrderExplorerViewModel(ILoadOrderService loadOrderService)
    {
        _loadOrderService = loadOrderService;
        loadOrderService.StatusChanged += (_, _) => Dispatcher.UIThread.InvokeAsync(Refresh);
        CopyEditorIdCommand = new RelayCommand(() => { }, () => SelectedRecord is not null);
        CopyFormIdCommand = new RelayCommand(() => { }, () => SelectedRecord is not null);
        CopyFormKeyCommand = new RelayCommand(() => { }, () => SelectedRecord is not null);
        UseInRuleCommand = new RelayCommand(() => { }, () => SelectedRecord is not null);
        RefreshCommand = new RelayCommand(Refresh);
        Refresh();
    }

    public ICommand CopyEditorIdCommand { get; }
    public ICommand CopyFormIdCommand { get; }
    public ICommand CopyFormKeyCommand { get; }
    public ICommand UseInRuleCommand { get; }
    public ICommand RefreshCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
                Refresh();
        }
    }

    public FormRecord? SelectedRecord
    {
        get => _selectedRecord;
        set
        {
            if (SetField(ref _selectedRecord, value))
            {
                OnPropertyChanged(nameof(DetailName));
                OnPropertyChanged(nameof(DetailEditorId));
                OnPropertyChanged(nameof(DetailFormId));
                OnPropertyChanged(nameof(DetailPlugin));
                OnPropertyChanged(nameof(DetailRecordType));
            }
        }
    }

    public ObservableCollection<FormRecord> Records { get; } = [];

    public string DetailName => SelectedRecord?.DisplayName ?? "—";
    public string DetailEditorId => SelectedRecord?.EditorId ?? "—";
    public string DetailFormId => SelectedRecord?.FormId ?? "—";
    public string DetailPlugin => SelectedRecord?.Plugin ?? "—";
    public string DetailRecordType => SelectedRecord?.RecordType ?? "—";

    private void Refresh()
    {
        Records.Clear();
        foreach (var record in _loadOrderService.Search(SearchText))
            Records.Add(record);

        SelectedRecord ??= Records.Count > 0 ? Records[0] : null;
        OnPropertyChanged(nameof(DetailName));
        OnPropertyChanged(nameof(DetailEditorId));
        OnPropertyChanged(nameof(DetailFormId));
        OnPropertyChanged(nameof(DetailPlugin));
        OnPropertyChanged(nameof(DetailRecordType));
    }
}
