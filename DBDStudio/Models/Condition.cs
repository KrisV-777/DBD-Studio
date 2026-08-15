using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DBDStudio.Models
{
    public sealed class Condition : INotifyPropertyChanged
    {
        private string _type = string.Empty;
        private string _operator = "==";
        private string _value = string.Empty;
        private int _group = 0;

        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public string Operator
        {
            get => _operator;
            set => SetProperty(ref _operator, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        // Conditions with the same group are OR-connected; groups are AND-connected.
        public int Group
        {
            get => _group;
            set => SetProperty(ref _group, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
