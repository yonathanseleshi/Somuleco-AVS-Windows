using System.Threading;
using System.Threading.Tasks;

namespace Somuleco_AVS.Platform.Rendering;

public interface IPreviewRenderer { Task RenderAsync(CancellationToken cancellationToken = default); }
public interface IRenderExecutor { Task ExecuteAsync(CancellationToken cancellationToken = default); }