using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Somuleco_AVS.Diagnostics;

/// <summary>
/// Process-wide unhandled-exception capture. Covers three surfaces: the WinUI dispatcher
/// thread (registered from <see cref="RegisterXamlUnhandledException"/> once an <c>App</c>
/// instance exists), any other unhandled exception via <see cref="AppDomain.UnhandledException"/>,
/// and unobserved <see cref="Task"/> exceptions. It cannot catch a failure earlier than the
/// module initializer below (e.g. native Windows App SDK bootstrap failures before Main runs);
/// that class of failure is diagnosed separately (WIN-FND-01 Implementation Task 1.1).
/// </summary>
internal static class CrashLogger
{
    /// <summary>
    /// Module initializers run when this assembly is loaded, before <c>Main</c> executes,
    /// which is the earliest point managed code can register a process-wide handler.
    /// </summary>
    [ModuleInitializer]
    internal static void RegisterProcessWideHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    internal static void RegisterXamlUnhandledException(Microsoft.UI.Xaml.Application app)
    {
        app.UnhandledException += OnXamlUnhandledException;
    }

    private static void OnXamlUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Deliberately not setting e.Handled = true: this wave adds diagnosis, not suppression.
        WriteCrashLog("XamlUnhandledException", e.Exception);
    }

    private static void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        WriteCrashLog("AppDomainUnhandledException", e.ExceptionObject as Exception);
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        WriteCrashLog("UnobservedTaskException", e.Exception);
    }

    private static void WriteCrashLog(string source, Exception? exception)
    {
        try
        {
            // Logging next to the executable is a developer-build-only convenience: this
            // location is not writable once the app is installed via MSIX. A proper per-user
            // storage-root resolution belongs to WIN-FND-08.
            var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
            Directory.CreateDirectory(logDirectory);

            var fileName = $"crash-{DateTime.Now:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid():N}.log";
            var filePath = Path.Combine(logDirectory, fileName);

            var content =
                $"Timestamp: {DateTime.Now:O}{Environment.NewLine}" +
                $"Source: {source}{Environment.NewLine}" +
                $"Type: {exception?.GetType().FullName ?? "(null exception object)"}{Environment.NewLine}" +
                $"Message: {exception?.Message}{Environment.NewLine}" +
                $"StackTrace:{Environment.NewLine}{exception?.StackTrace}{Environment.NewLine}";

            File.WriteAllText(filePath, content);
        }
        catch (Exception loggingFailure)
        {
            // The logging path must never throw or mask the original exception. Fall back to
            // Debug/Trace output so the information is not silently lost even when the disk
            // write itself fails (e.g. disk full, permission denied, path too long).
            Debug.WriteLine($"[CrashLogger] Failed to write crash log for {source}: {loggingFailure}");
            Debug.WriteLine($"[CrashLogger] Original exception ({source}): {exception}");
            Trace.WriteLine($"[CrashLogger] Failed to write crash log for {source}: {loggingFailure}");
            Trace.WriteLine($"[CrashLogger] Original exception ({source}): {exception}");
        }
    }
}
