using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Somuleco_AVS.Core.Models;

namespace Somuleco_AVS;

public sealed partial class ProjectDetailPage : Page
{
    public ProjectDetailPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is not AVSProject project) return;
        ContentPanel.Children.Add(new TextBlock { Text = project.Title, Style = (Style)Application.Current.Resources["SectionTitleStyle"] });
        ContentPanel.Children.Add(new TextBlock { Text = project.Description, FontSize = 17 });
        var card = new Border { Style = (Style)Application.Current.Resources["CardStyle"] };
        var details = new StackPanel { Spacing = 10 };
        details.Children.Add(new TextBlock { Text = $"{project.Type}  •  {project.State}" });
        details.Children.Add(new TextBlock { Text = $"Modified {project.ModifiedLabel}" });
        details.Children.Add(new TextBlock { Text = $"Media: 4 source assets    Transcript: Ready    Processing: In progress    Publication: Draft" });
        details.Children.Add(new TextBlock { Text = project.LearningContext is null ? "Learning context: not linked" : $"Learning context: {project.LearningContext}" });
        card.Child = details;
        ContentPanel.Children.Add(card);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        actions.Children.Add(new Button { Content = "Open in Editor", Style = (Style)Application.Current.Resources["AccentButtonStyle"] });
        actions.Children.Add(new Button { Content = "Open in Studio" });
        actions.Children.Add(new Button { Content = "Create Clip" });
        actions.Children.Add(new Button { Content = "Publish" });
        ContentPanel.Children.Add(actions);
        var tabs = new TabView();
        foreach (var tabName in new[] { "Overview", "Media", "Script", "Transcript", "Review", "Publish" })
        {
            tabs.TabItems.Add(new TabViewItem { Header = tabName });
        }
        ContentPanel.Children.Add(tabs);
    }
}