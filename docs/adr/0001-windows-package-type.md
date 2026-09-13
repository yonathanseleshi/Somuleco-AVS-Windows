# ADR-0001: WindowsPackageType=None for unpackaged launch, plus the Windows App SDK 2.4.0 runtime installation this exposed

**Status:** Accepted
**Date:** 2026-09-12
**Wave:** WIN-FND-01 (Executable Baseline and Repository Quality)

## Context

The Windows application (`Somuleco-AVS.csproj`) could not launch when run
directly from a plain build output (`bin\...\Somuleco-AVS.exe`). It exited
immediately with code `-532462766` (`0xE0434352`), the standard signature for
an unhandled managed exception. The app *did* launch correctly when deployed
via Visual Studio F5 through the packaging project (`.wapproj`), since
packaged activation supplies the package identity the app was implicitly
depending on.

## Investigation

Before changing anything, a diagnostic crash logger
(`Somuleco_AVS.Diagnostics.CrashLogger`, added as part of this wave's
Implementation Task 3 and reused here for Task 1.1's evidence-gathering
requirement) was wired up via a `[ModuleInitializer]`-registered
`AppDomain.CurrentDomain.UnhandledException` handler — the earliest point
managed code can hook a process-wide handler, since it runs when the
assembly loads, before `Main` executes.

Running the unpackaged exe with this instrumentation in place captured the
actual exception for the first time:

```
Type: System.Runtime.InteropServices.COMException
Message: Class not registered (0x80040154 (REGDB_E_CLASSNOTREG))
StackTrace:
   at System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Int32 errorCode)
   at WinRT.ActivationFactory.Get(String typeName, Guid iid)
   at Microsoft.UI.Xaml.Application.get__objRef_global__Microsoft_UI_Xaml_IApplicationStatics()
   at Microsoft.UI.Xaml.Application.Start(ApplicationInitializationCallback callback)
   at Somuleco_AVS.Program.Main(String[] args) in ...\App.g.i.cs:line 33
```

This confirmed the working hypothesis from the wave plan: WinUI's native
activation factory could not be resolved via registration-free/package
activation, because the project had no unpackaged-app identity path. The
project set `UseWinUI=true` and `WinUISDKReferences=false` but never set
`WindowsPackageType` or `EnableMsixTooling`.

## Decision

1. Add to `Somuleco-AVS.csproj`:
   ```xml
   <WindowsPackageType>None</WindowsPackageType>
   <EnableMsixTooling>true</EnableMsixTooling>
   ```
   This is the officially documented, simplest way to enable the Windows App
   SDK runtime for an unpackaged/framework-dependent app (per the Windows
   App SDK deployment guide). Setting `WindowsPackageType=None` causes the
   `Microsoft.WindowsAppSDK.Foundation` NuGet package to inject an
   auto-initializer (`Microsoft.Windows.ApplicationModel.DynamicDependency.
   BootstrapCS.AutoInitialize.AccessWindowsAppSDK`, generated into the build
   via `Microsoft.WindowsAppSDK.Bootstrap.CS.targets`) that calls
   `Bootstrap.TryInitialize` automatically before `Main` runs. **No manual
   bootstrap call was written** — the auto-injected code already exists and
   is the correct mechanism; writing a second, hand-rolled bootstrap call
   would have duplicated it. This confirms the plan's fallback path
   (Implementation Task 1.4, "discover the real API before writing a call")
   was unnecessary once the actual mechanism was understood — there was
   nothing to discover or write, only a property to set.

2. Rebuilding and re-running after step 1 produced a **different, more
   specific** failure — not a launch, but progress:
   ```
   Exit code: -2140733418  (0x80670016)
   ```
   No crash log was written for this one, which is itself informative: the
   auto-initializer calls `Environment.Exit(hr)` directly on bootstrap
   failure (see `MddBootstrapAutoInitializer.cs` in the
   `Microsoft.WindowsAppSDK.Foundation` package) rather than throwing, so
   this is a deliberate exit, not a crash — consistent with `AppDomain.
   UnhandledException` correctly *not* firing.

3. Web research against Microsoft's own documentation and the Windows App
   SDK GitHub repository identified `0x80670016` as "Package dependency
   criteria could not be resolved" — the Bootstrap API found the Windows App
   SDK **Framework** package (already registered on this machine at version
   2.4.0.0, presumably from the Visual Studio WinUI workload) but could not
   find the **Main**, **Singleton**, or **DDLM** packages that an unpackaged
   app's bootstrap also requires. Checking `Get-AppxPackage` confirmed only
   the Framework package (`Microsoft.WindowsAppRuntime.2`) was present for
   2.4.0 — no `MicrosoftCorporationII.WinAppRuntime.Main.2`, no
   `MicrosoftCorporationII.WinAppRuntime.Singleton`, no
   `Microsoft.WinAppRuntime.DDLM.2.4.0.0-*`.

4. Downloaded and ran the official Windows App SDK 2.4.0 runtime installer
   for the exact version this project references
   (`https://aka.ms/windowsappsdk/2.4/2.4.0/windowsappruntimeinstall-x64.exe`,
   matching the `Microsoft.WindowsAppSDK` NuGet package version 2.4.0 in
   `Somuleco-AVS.csproj`), unelevated (`--quiet`, current-user scope only,
   no system-wide registration):
   ```
   exit code: 0
   ```
   `Get-AppxPackage` afterward showed all four required package types
   present for 2.4.0.0 (Framework, Main, Singleton, and two DDLM entries —
   one per architecture, x64 and x86).

5. Re-ran the unpackaged exe: it now launches and shows a window titled
   "Somuleco AVS", and closes cleanly via its normal window-close affordance
   with no orphaned process and no new `APPCRASH` event.

## Root cause, stated precisely

Two independent, additive problems, not one:

1. **Code/project-configuration problem** (fixed by this ADR's change):
   `Somuleco-AVS.csproj` lacked `WindowsPackageType=None`, so no bootstrap
   initialization code ran at all, and WinUI's native activation failed with
   `REGDB_E_CLASSNOTREG`.
2. **Machine/environment problem** (fixed by installing the runtime, not by
   any code change): even with bootstrap code now running, this specific
   development machine had never had the Windows App SDK 2.4.0 runtime's
   Main/Singleton/DDLM packages installed — only the Framework package was
   present. This is a one-time per-machine developer setup step, not
   something a project file can fix, and it is not currently automated by
   any part of this repository's build or CI (a gap for `WIN-FND-10` to
   consider if runtime installation is discovered to be needed in CI or
   provisioning scripts too).

## Why WindowsPackageType=None is correct for this project (and not
switching to packaged-only distribution)

`Prompt/gpt-windows.prompt.md` explicitly directs a native WinUI 3 desktop
scaffold — "no MAUI, no web wrapper" — with the app intended to run and be
developed without requiring a store/MSIX-first workflow at this stage of
Foundation. `WindowsPackageType=None` is the documented, standard setting
for exactly this kind of framework-dependent unpackaged app, and it does not
preclude also building and shipping the MSIX package (the `.wapproj` still
builds unchanged, and continues to be validated as part of this wave's
Acceptance Criterion 5). Switching to packaged-only distribution would make
the fast inner-loop `dotnet build && run .exe` workflow impossible, which
directly conflicts with Foundational Waves Guide §4.2's priority on a fast,
reliable executable state during Foundation.

## Consequences

- The app now launches unpackaged, satisfying this wave's primary
  objective.
- Any other developer machine setting up this repository for the first time
  will need the same one-time runtime installation step. This is
  documented in `docs/RUNNING.md` ("If the app won't launch", item 4) so it
  is not rediscovered by trial and error.
- `WIN-FND-10` (Packaging, CI/CD, and Foundation Reconciliation) should
  confirm whether CI runners (`windows-latest` GitHub Actions images) ship
  with the Windows App SDK 2.4.0 Main/Singleton/DDLM packages preinstalled,
  or whether the CI pipeline will also need to run the runtime installer
  before any launch-based validation step. This was not tested as part of
  this wave (no CI exists yet).
- No manual `Bootstrap.TryInitialize` call was added to application code;
  the auto-injected initializer from `Microsoft.WindowsAppSDK.Foundation` is
  relied on as-is. If a future wave needs finer control over bootstrap
  options (e.g. suppressing the `OnNoMatch_ShowUI` default, which shows a
  UI prompt to end users if their runtime is missing/outdated), that is a
  deliberate, separate decision to make at that time, not implied by this
  ADR.
