# Running, Building, and Packaging Somuleco AVS (Windows)

All commands below assume the working directory is the **Git repository root**
(`Somuleco-AVS-Windows/Somuleco-AVS/` on disk — this is the folder that
contains `.git`, `Somuleco-AVS.slnx`, `global.json`, and this `docs/` folder).
The outer `Somuleco-AVS-Windows` folder is just the working-copy container and
is never a valid working directory for these commands.

## Prerequisites

- .NET SDK **10.0.400** exactly — pinned by `global.json` at the repository
  root with `rollForward: "disable"`. If a different SDK is installed,
  `dotnet` commands will fail with a clear SDK-resolution error rather than
  silently building with a different version. See
  [`docs/adr/0002-tfm-and-sdk-pinning.md`](adr/0002-tfm-and-sdk-pinning.md).
- Visual Studio 2022/2026 with the **Windows App SDK** / WinUI workload, or
  the standalone Windows SDK build tools — needed to build the `.wapproj`
  packaging project (MSBuild, not the `dotnet` CLI, is required for that
  project type; see below).
- **Windows App SDK 2.4.0 runtime** installed for the current user. This is
  required to *run* the app (not just build it) — see "If the app won't
  launch" below if it's missing.

## Building the application project

The application project (`Somuleco-AVS/Somuleco-AVS.csproj`, i.e.
`Somuleco-AVS/Somuleco-AVS/Somuleco-AVS.csproj` relative to the repo root) is
a modern SDK-style project and builds with the `dotnet` CLI. `Platform` must
be specified explicitly since the project supports `x86`, `x64`, and `ARM64`:

```powershell
dotnet build "Somuleco-AVS\Somuleco-AVS\Somuleco-AVS.csproj" -p:Platform=x64
```

Release configuration:

```powershell
dotnet build "Somuleco-AVS\Somuleco-AVS\Somuleco-AVS.csproj" -c Release -p:Platform=x64
```

## Building the packaging project (MSIX)

The packaging project (`Somuleco-AVS (Package)/Somuleco-AVS (Package).wapproj`)
is a legacy-style Windows Application Packaging Project. **The `dotnet` CLI
cannot build a `.wapproj`** — this project type is not supported by the SDK-
style `dotnet build` command and requires full MSBuild (the one that ships
with Visual Studio):

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  "Somuleco-AVS\Somuleco-AVS (Package)\Somuleco-AVS (Package).wapproj" `
  -t:Build -p:Configuration=Debug -p:Platform=x64
```

Adjust the MSBuild path for your Visual Studio edition/version if different.
This produces a `.msix` under
`Somuleco-AVS\Somuleco-AVS (Package)\AppPackages\...`.

Building the `.wapproj` also builds the application project as a dependency,
so this single command is sufficient to produce both the app binaries and the
package.

## Running the app (unpackaged, from a plain build)

```powershell
cd "Somuleco-AVS\Somuleco-AVS\bin\x64\Debug\net8.0-windows10.0.19041.0"
.\Somuleco-AVS.exe
```

This is the fastest inner-loop way to run the app during development — no
MSIX deployment, no Visual Studio required. As of WIN-FND-01 this works
provided the Windows App SDK 2.4.0 runtime is installed for the current user
(see below); prior to WIN-FND-01, this crashed immediately with exit code
`-532462766` (`0xE0434352`) regardless of runtime installation, due to a
missing `WindowsPackageType=None` project setting — see
[`docs/adr/0001-windows-package-type.md`](adr/0001-windows-package-type.md)
for the full history, kept here for anyone debugging a similar-looking crash
in the future.

## If the app won't launch

1. **Check for a crash log first.** The app writes unhandled-exception
   details to `Logs\crash-*.log` next to the executable (developer-build
   only — see `Somuleco-AVS/Diagnostics/CrashLogger.cs`). This will usually
   tell you the exact exception without needing anything below.
2. **Check the Windows Application event log** for an `APPCRASH` entry, if
   no crash log was written (this can happen for a failure early enough that
   the crash logger's hooks aren't registered yet):
   ```powershell
   Get-WinEvent -FilterHashtable @{LogName='Application'; StartTime=(Get-Date).AddMinutes(-5)} |
     Where-Object { $_.Message -like '*Somuleco-AVS.exe*' } |
     Select-Object -First 1 -ExpandProperty Message
   ```
3. **`COMException: Class not registered (0x80040154)`** — the project is
   missing `WindowsPackageType=None`/`EnableMsixTooling=true`. Already fixed
   as of WIN-FND-01; if you see this again, check `Somuleco-AVS.csproj`.
4. **Exit code `0x80670016` ("Package dependency criteria could not be
   resolved")** — the Windows App SDK 2.4.0 runtime is not fully installed
   for the current user. Unpackaged WinUI/Windows App SDK apps require four
   MSIX package types registered per-user: Framework, Main, Singleton, and
   DDLM. Having only the Framework package (e.g. from a Visual Studio
   install) is not sufficient. Fix by running the official runtime
   installer for the exact version referenced by the project
   (`Microsoft.WindowsAppSDK` 2.4.0 in `Somuleco-AVS.csproj`):
   ```powershell
   # Download from https://aka.ms/windowsappsdk/2.4/2.4.0/windowsappruntimeinstall-x64.exe
   .\windowsappruntimeinstall-x64.exe --quiet
   ```
   Verify all four package types are present afterward:
   ```powershell
   Get-AppxPackage | Where-Object { $_.Name -like '*WindowsAppRuntime*' -or $_.Name -like '*WinAppRuntime*' } |
     Select-Object Name, Version, Architecture
   ```
   You should see entries for `Microsoft.WindowsAppRuntime.2` (Framework),
   `MicrosoftCorporationII.WinAppRuntime.Main.2`,
   `MicrosoftCorporationII.WinAppRuntime.Singleton`, and two
   `Microsoft.WinAppRuntime.DDLM.2.4.0.0-*` entries (one per architecture).
   Full background in
   [`docs/adr/0001-windows-package-type.md`](adr/0001-windows-package-type.md).

## Running the packaged app (MSIX)

Not yet exercised as of WIN-FND-01 — deploying the locally built MSIX
requires trusting a developer test certificate, which is a machine-state
change deferred to `WIN-FND-10` pending explicit approval (tracked as `OQ-2`
in the Windows Foundation Wave plan). Until then, use Visual Studio's own
F5/Ctrl+F5 deploy against the `.wapproj` if a packaged-activation test is
needed.
