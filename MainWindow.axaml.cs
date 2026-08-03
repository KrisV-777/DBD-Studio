using Avalonia.Controls;
using Body_Distribution_Studio.ViewModels;

namespace Body_Distribution_Studio;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
