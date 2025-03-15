using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XBoundObjectMSTest.TestClassesForModeling.Test_ModelInit
{
    class ClassWithOnePropertyINPC : INotifyPropertyChanged
    {
        public object? A
        {
            get => _a;
            set
            {
                if (!Equals(_a, value))
                {
                    _a = value;
                    OnPropertyChanged();
                }
            }
        }
        object? _a = null;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
