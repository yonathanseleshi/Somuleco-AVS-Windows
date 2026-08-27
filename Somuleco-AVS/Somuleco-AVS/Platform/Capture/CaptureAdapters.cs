using System.Threading;
using System.Threading.Tasks;

namespace Somuleco_AVS.Platform.Capture;

public interface ICameraCaptureAdapter { Task StartAsync(CancellationToken cancellationToken = default); }
public interface IMicrophoneCaptureAdapter { Task StartAsync(CancellationToken cancellationToken = default); }
public interface IScreenCaptureAdapter { Task StartAsync(CancellationToken cancellationToken = default); }
public interface ISystemAudioCaptureAdapter { Task StartAsync(CancellationToken cancellationToken = default); }