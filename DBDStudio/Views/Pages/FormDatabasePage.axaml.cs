using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using DBDStudio.Interfaces.Mutagen;

namespace DBDStudio.Views.Pages
{
    public partial class FormDatabasePage : UserControl
    {
        public FormDatabasePage()
        {
            InitializeComponent();
        }

        private void OnPluginRowTapped(object? sender, TappedEventArgs e)
        {
            if (e.Source is not Visual visual)
                return;

            if (visual.FindAncestorOfType<CheckBox>() is not null)
                return;

            var row = visual.FindAncestorOfType<TableViewRow>();

            if (row?.DataContext is IPluginData plugin) {
                plugin.IsEnabled = !plugin.IsEnabled;
            }
        }
    }
}
