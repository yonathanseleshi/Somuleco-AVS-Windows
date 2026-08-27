namespace Somuleco_AVS.Core.Models;

public enum AVSProjectType { Lesson, Clip, Podcast, Screencast, Presentation, Recording, Webinar, Livestream, StandaloneVideo }
public enum AVSProjectState { Draft, InProduction, InReview, ReadyToPublish, Published, Archived }

public sealed record AVSProject(string Id, string Title, string Description, AVSProjectType Type, AVSProjectState State, string ModifiedLabel, string? LearningContext = null);