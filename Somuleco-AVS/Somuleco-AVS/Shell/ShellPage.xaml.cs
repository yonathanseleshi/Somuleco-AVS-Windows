using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Somuleco_AVS;

public sealed partial class ShellPage : Page
{
    public ShellPage()
    {
        InitializeComponent();
        WorkspaceFrame.Navigate(typeof(WorkspacePage), "Home");
        AppNavigation.SelectedItem = AppNavigation.MenuItems[0];
    }

    private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string destination)
        {
            WorkspaceFrame.Navigate(typeof(WorkspacePage), destination);
        }
    }

    private void OnNewProjectClick(object sender, RoutedEventArgs e) => WorkspaceFrame.Navigate(typeof(WorkspacePage), "Create");
    private void OnRecordClick(object sender, RoutedEventArgs e) => WorkspaceFrame.Navigate(typeof(WorkspacePage), "Studio");
    private void OnImportMediaClick(object sender, RoutedEventArgs e) => WorkspaceFrame.Navigate(typeof(WorkspacePage), "Library");
    private void OnProcessingClick(object sender, RoutedEventArgs e) => WorkspaceFrame.Navigate(typeof(WorkspacePage), "Processing");
}