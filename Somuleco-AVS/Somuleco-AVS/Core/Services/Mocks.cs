using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Somuleco_AVS.Core.Models;

namespace Somuleco_AVS.Core.Services;

public sealed class MockProjectService : IProjectService
{
    private static readonly IReadOnlyList<AVSProject> Items =
    [
        new("project-generative-ai", "Introduction to Generative AI", "A practical lesson for curious builders.", AVSProjectType.Lesson, AVSProjectState.InProduction, "Edited 18 minutes ago", "AI Foundations"),
        new("project-excel", "Excel Financial Forecasting", "A screen-led walkthrough with downloadable examples.", AVSProjectType.Screencast, AVSProjectState.Draft, "Edited yesterday"),
        new("project-leadership", "Leadership Fundamentals", "A polished course media project ready for review.", AVSProjectType.Presentation, AVSProjectState.InReview, "Edited 3 days ago", "Leadership"),
        new("project-python", "Python for Data Analysis", "A recording project with transcript and clip candidates.", AVSProjectType.Recording, AVSProjectState.Published, "Published Aug 20")
    ];

    public Task<IReadOnlyList<AVSProject>> GetProjectsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Items);
    public Task<AVSProject?> GetProjectAsync(string id, CancellationToken cancellationToken = default) => Task.FromResult(Items.FirstOrDefault(item => item.Id == id));
}

public sealed class MockProcessingService : IProcessingService
{
    public Task<IReadOnlyList<ProcessingJob>> GetJobsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ProcessingJob>>(
    [
        new("job-1", "Introduction to Generative AI", ProcessingJobState.Processing, .72),
        new("job-2", "Leadership Fundamentals captions", ProcessingJobState.Completed, 1),
        new("job-3", "Python for Data Analysis proxy", ProcessingJobState.Queued, 0)
    ]);
}

public sealed class MockAIService : IAIService
{
    public Task<string> SuggestNextStepAsync(string prompt, CancellationToken cancellationToken = default) => Task.FromResult("Turn your latest recording into a lesson outline and three short Clips.");
}