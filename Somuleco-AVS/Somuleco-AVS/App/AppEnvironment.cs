using Somuleco_AVS.Core.Services;

namespace Somuleco_AVS;

public sealed class AppEnvironment
{
    public required IProjectService Projects { get; init; }
    public required IProcessingService Processing { get; init; }
    public required IAIService AI { get; init; }

    public static AppEnvironment CreateMock() => new()
    {
        Projects = new MockProjectService(),
        Processing = new MockProcessingService(),
        AI = new MockAIService()
    };
}