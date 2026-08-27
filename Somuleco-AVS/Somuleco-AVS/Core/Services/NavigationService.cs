using System;

namespace Somuleco_AVS.Core.Services;

public interface INavigationService
{
    string CurrentWorkspace { get; }
    event EventHandler<string>? Navigated;
    void NavigateTo(string workspace);
}

public sealed class NavigationService : INavigationService
{
    public string CurrentWorkspace { get; private set; } = "Home";
    public event EventHandler<string>? Navigated;

    public void NavigateTo(string workspace)
    {
        CurrentWorkspace = workspace;
        Navigated?.Invoke(this, workspace);
    }
}