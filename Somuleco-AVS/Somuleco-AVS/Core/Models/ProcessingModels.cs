namespace Somuleco_AVS.Core.Models;

public enum ProcessingJobState { Queued, Processing, Completed, Failed, Retrying, Cancelled }
public sealed record ProcessingJob(string Id, string Name, ProcessingJobState State, double Progress);