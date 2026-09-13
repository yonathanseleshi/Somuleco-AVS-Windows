# ADR-0002: Retain net8.0-windows TFM; pin the .NET SDK exactly via global.json

**Status:** Accepted
**Date:** 2026-09-12
**Wave:** WIN-FND-01 (Executable Baseline and Repository Quality)

## Context

Only the .NET 10 SDK (10.0.400) is installed on this development machine,
while `Somuleco-AVS.csproj` targets `net8.0-windows10.0.19041.0`. With no
`global.json`, the .NET SDK resolver's default roll-forward behavior
(`latestMinor` when no `global.json` is present) silently picks whatever SDK
band happens to be installed, which could change build behavior across
machines or after a future SDK update with no visible signal.

## Decision 1: Retain the net8.0-windows TFM

`net8.0-windows10.0.19041.0` is kept unchanged. .NET 8 is still an actively
supported LTS release, and the `Microsoft.WindowsDesktop.App 8.0.30` runtime
is present and working on this machine (confirmed: the app builds and now
launches successfully under it — see ADR-0001). Foundational Waves Guide
§4.5 ("Minimal but Complete") counsels against speculative upgrades; nothing
in this wave's diagnosis required or benefited from moving to a `net10.0-
windows` TFM, so no such move was made. `TargetPlatformMinVersion`
(`10.0.17763.0`) is likewise left unchanged — it is a product decision about
minimum supported Windows version, not a technical blocker uncovered by this
wave, and is out of scope here.

## Decision 2: Pin the SDK via global.json with exact-match (disable) roll-forward

A `global.json` was added at the Git repository root:

```json
{
  "sdk": {
    "version": "10.0.400",
    "rollForward": "disable"
  }
}
```

### The tradeoff considered

Two interpretations of "pin the SDK" are both legitimate, but they are
mutually exclusive and were not both possible at once:

- **Exact reproducibility**: `rollForward: "disable"` (or `"patch"`) —
  `dotnet --version` resolves to precisely the pinned value, and the build
  fails loudly with a clear SDK-resolution error if that exact SDK is not
  installed, rather than silently substituting a different one.
- **Feature-band flexibility**: `rollForward: "latestFeature"` (or
  `"latestMinor"`) — the build automatically follows newer SDK installs
  without requiring a `global.json` edit, at the cost of `dotnet --version`
  no longer resolving to one guaranteed value over time.

These two goals contradict each other by construction: a floating
roll-forward policy and an exact-match verification test cannot both hold,
because a floating policy is defined by *not* resolving to one fixed value
forever.

### Decision made

This wave chooses **exact reproducibility** (`rollForward: "disable"`),
because the wave's own Acceptance Criterion 6 requires `dotnet --version` to
resolve to the exact pinned SDK version as a verifiable, automatable check —
and a build that silently drifted onto a different SDK band after some
future `dotnet-sdk` update on a developer's or CI machine is exactly the
kind of invisible-until-it-breaks failure this wave's Task 2 exists to
prevent. `"disable"` was chosen over `"patch"` specifically because
`"patch"` still permits `dotnet --version` to resolve to a *different*
value (e.g. `10.0.401`) after a routine patch update, which does not fully
satisfy "resolves to exactly 10.0.400."

### Rejected alternative

Feature-band or patch flexibility (`"latestFeature"`/`"latestPatch"`) is a
valid choice for a team that prioritizes low-maintenance automatic SDK
tracking over guaranteed exact reproducibility. It was not chosen here
because it would have required weakening Acceptance Criterion 6 to "resolves
to some 10.0.4xx-band SDK" instead of an exact value, and this wave's
purpose is specifically to remove ambiguity from the build, not relocate it.
If a later wave (e.g. during `WIN-FND-10`'s CI setup) finds that exact
pinning causes friction — for instance, if the CI image's preinstalled SDK
patch version does not exactly match `10.0.400` — the correct fix is to
either update this pinned version to match deliberately, or revisit this
decision explicitly, not to silently switch the roll-forward policy.

## Verification

```
> dotnet --version
10.0.400
```

Confirmed from the Git repository root after adding `global.json`, with no
other SDK bands installed on this machine to create ambiguity in the test.

## Consequences

- Any machine without exactly .NET SDK 10.0.400 installed will get a clear,
  actionable SDK-resolution error from `dotnet build`/`dotnet run`, rather
  than an unexplained difference in build behavior.
- Updating the pinned SDK version in the future (e.g. adopting a newer
  patch or feature band deliberately) requires an explicit `global.json`
  edit, which is the intended friction — it forces the change to be a
  visible, reviewable decision rather than an incidental side effect of
  updating Visual Studio or the SDK on a given machine.
