using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Somuleco_AVS.Core.Models;

namespace Somuleco_AVS;

public sealed partial class WorkspacePage : Page
{
    private string _destination = "Home";

    public WorkspacePage() => InitializeComponent();

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _destination = e.Parameter as string ?? "Home";
        await RenderAsync();
    }

    private async Task RenderAsync()
    {
        ContentPanel.Children.Clear();
        AddHeader(GetTitle(_destination), GetSubtitle(_destination));

        if (_destination == "Projects")
        {
            var projects = await App.Environment.Projects.GetProjectsAsync();
            AddProjects(projects);
            return;
        }

        if (_destination == "Processing")
        {
            var jobs = await App.Environment.Processing.GetJobsAsync();
            foreach (var job in jobs) AddJob(job);
            return;
        }

        AddWorkspaceContent(_destination);
    }

    private void AddHeader(string title, string subtitle)
    {
        ContentPanel.Children.Add(new TextBlock { Text = title, Style = (Style)Application.Current.Resources["SectionTitleStyle"] });
        ContentPanel.Children.Add(new TextBlock { Text = subtitle, Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MutedTextBrush"], FontSize = 15 });
    }

    private void AddProjects(IReadOnlyList<AVSProject> projects)
    {
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        actions.Children.Add(new Button { Content = "New project", Style = (Style)Application.Current.Resources["AccentButtonStyle"] });
        actions.Children.Add(new Button { Content = "Import media" });
        ContentPanel.Children.Add(actions);
        var list = new StackPanel { Spacing = 10 };
        foreach (var project in projects)
        {
            var button = new Button { HorizontalContentAlignment = HorizontalAlignment.Stretch, Padding = new Thickness(0), Background = null, BorderThickness = new Thickness(0) };
            var card = new Border { Style = (Style)Application.Current.Resources["CardStyle"] };
            var details = new StackPanel { Spacing = 5 };
            details.Children.Add(new TextBlock { Text = project.Title, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            details.Children.Add(new TextBlock { Text = $"{project.Type}  •  {project.State}  •  {project.ModifiedLabel}", Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MutedTextBrush"] });
            details.Children.Add(new TextBlock { Text = project.Description });
            card.Child = details;
            button.Content = card;
            button.Click += (_, _) => Frame.Navigate(typeof(ProjectDetailPage), project);
            list.Children.Add(button);
        }
        ContentPanel.Children.Add(list);
    }

    private void AddJob(ProcessingJob job)
    {
        var card = new Border { Style = (Style)Application.Current.Resources["CardStyle"] };
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(new TextBlock { Text = job.Name, FontSize = 17, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        stack.Children.Add(new ProgressBar { Value = job.Progress * 100, Maximum = 100 });
        stack.Children.Add(new TextBlock { Text = $"{job.State}  •  {job.Progress:P0}", Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MutedTextBrush"] });
        card.Child = stack;
        ContentPanel.Children.Add(card);
    }

    private void AddWorkspaceContent(string destination)
    {
        var copy = destination switch
        {
            "Home" => "Welcome back, Maya. Continue building the Introduction to Generative AI lesson, or start something new.",
            "Studio" => "Recording control room scaffold: camera, microphone, screen, scenes, program preview, and teleprompter are ready for native adapters.",
            "Editor" => "A non-destructive editing workspace will host the preview, media bin, transcript, inspector, and timeline tracks.",
            "Clips" => "Find Clip candidates, shape a vertical preview, tune captions, and preserve source lineage back to the original project.",
            "Create" => "What do you want to create? Try: Create a beginner lesson on neural networks.",
            "Library" => "Your local-first media library for video, audio, images, recordings, Clips, podcasts, replays, and brand assets.",
            "Live" => "Live production workspace for workshops, webinars, classes, podcasts, and replays. Realtime transport is intentionally mocked.",
            "Meet" => "Start or join a meeting with participant, camera, microphone, captions, chat, and screen-share surfaces prepared.",
            "Podcasts" => "Plan episodes, record guests, edit the transcript, and generate Clips from a focused podcast production workspace.",
            "Publish" => "Prepare a title, description, thumbnail, captions, transcript, destination, and schedule for publication.",
            "Analytics" => "Creator overview: views, watch time, completion, Clip completion, live attendance, followers, and course conversion.",
            "Settings" => "General, recording, audio, video, teleprompter, live, storage, and AI preferences will live here using native Windows settings patterns.",
            _ => "This AVS workspace is ready for its feature wave."
        };
        var card = new Border { Style = (Style)Application.Current.Resources["CardStyle"] };
        card.Child = new TextBlock { Text = copy, TextWrapping = TextWrapping.Wrap, FontSize = 18, LineHeight = 28 };
        ContentPanel.Children.Add(card);
        ContentPanel.Children.Add(new TextBlock { Text = "Mock mode • Local-first • Native Windows foundation", Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["MutedTextBrush"] });
    }

    private static string GetTitle(string destination) => destination switch { "Home" => "Good morning, Maya", "Processing" => "Processing center", _ => destination };
    private static string GetSubtitle(string destination) => destination == "Home" ? "Your creative work, gathered in one calm place." : "Somuleco Audio Video Studio";
}