using System.Threading;
using System.Threading.Tasks;

namespace Somuleco_AVS.Platform.RustBridge;

public interface IRustMediaCoreBridge
{
    Task ValidateProjectAsync(string projectPath, CancellationToken cancellationToken = default);
    Task SaveProjectAsync(string projectPath, CancellationToken cancellationToken = default);
}