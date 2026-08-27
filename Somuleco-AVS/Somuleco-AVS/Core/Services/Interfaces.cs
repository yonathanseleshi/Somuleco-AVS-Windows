using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Somuleco_AVS.Core.Models;

namespace Somuleco_AVS.Core.Services;

public interface IProjectService { Task<IReadOnlyList<AVSProject>> GetProjectsAsync(CancellationToken cancellationToken = default); Task<AVSProject?> GetProjectAsync(string id, CancellationToken cancellationToken = default); }
public interface IProcessingService { Task<IReadOnlyList<ProcessingJob>> GetJobsAsync(CancellationToken cancellationToken = default); }
public interface IAIService { Task<string> SuggestNextStepAsync(string prompt, CancellationToken cancellationToken = default); }
public interface IAuthService { Task<bool> IsSignedInAsync(CancellationToken cancellationToken = default); }
public interface IMediaLibraryService { Task<IReadOnlyList<MediaAsset>> GetAssetsAsync(CancellationToken cancellationToken = default); }
public interface IRecordingService { Task StartAsync(CancellationToken cancellationToken = default); Task StopAsync(CancellationToken cancellationToken = default); }
public interface ICaptureDeviceService { Task<IReadOnlyList<string>> GetDevicesAsync(CancellationToken cancellationToken = default); }
public interface ILiveService { Task<IReadOnlyList<LiveExperience>> GetExperiencesAsync(CancellationToken cancellationToken = default); }
public interface IRealtimeService { Task<AVSSession?> GetSessionAsync(string id, CancellationToken cancellationToken = default); }
public interface IPublishingService { Task<Publication> PublishAsync(string projectId, CancellationToken cancellationToken = default); }
public interface IDesktopFileService { Task<IReadOnlyList<string>> ImportAsync(CancellationToken cancellationToken = default); }
public interface ILocalProjectStore { Task<AVSProject?> LoadAsync(string path, CancellationToken cancellationToken = default); Task SaveAsync(AVSProject project, string path, CancellationToken cancellationToken = default); }