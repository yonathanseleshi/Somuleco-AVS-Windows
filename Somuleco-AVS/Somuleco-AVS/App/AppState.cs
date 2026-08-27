using System;
using System.ComponentModel;

namespace Somuleco_AVS;

public sealed class AppState : INotifyPropertyChanged
{
    private string _selectedWorkspace = "Home";
    public string SelectedWorkspace
    {
        get => _selectedWorkspace;
        set
        {
            if (_selectedWorkspace == value) return;
            _selectedWorkspace = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedWorkspace)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}