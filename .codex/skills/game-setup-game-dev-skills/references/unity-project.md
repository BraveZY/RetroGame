# Unity Project Identification Checklist

When using `game-setup-game-dev-skills` on a Unity project, identify project facts according to this file.

## Required Signals

At least two of the following signals must be present before judging the project to be a Unity project:

- `Assets/`
- `Packages/manifest.json`
- `ProjectSettings/ProjectVersion.txt`

## Unity Version

Read this first:

```text
ProjectSettings/ProjectVersion.txt
```

Record:

- `m_EditorVersion`
- `m_EditorVersionWithRevision`

If the file does not exist, record unknown; do not guess the version.

## Code Structure

Check common directories:

- `Assets/Scripts/`
- `Assets/Scripts/Runtime/`
- `Assets/Scripts/Editor/`
- `Assets/Scripts/Tests/`
- `Assets/Scripts/Tests/EditMode/`
- `Assets/Scripts/Tests/PlayMode/`

Check whether these exist:

- `.asmdef`
- Runtime assembly
- Editor assembly
- Test assembly

## Content Structure

Check common directories:

- `Assets/Scenes/`
- `Assets/Prefabs/`
- `Assets/Art/`
- `Assets/Audio/`
- `Assets/Animations/`
- `Assets/Materials/`
- `Assets/ScriptableObjects/`
- `Assets/Resources/`
- `Assets/StreamingAssets/`

Do not create directories because they are missing; only record existing content and suggestions.

## Packages and Capabilities

Read `Packages/manifest.json` and record packages relevant to the current work:

- `com.unity.test-framework`
- UnityCaptain / UnityMCP, such as `com.njljh.unitymcp` or a package address containing `UnityCaptain`
- Input System
- Addressables
- URP/HDRP
- Cinemachine
- Timeline
- Ads/IAP/Analytics

Do not assume unavailable packages exist.

## UnityCaptain / UnityMCP

If the project has UnityCaptain / UnityMCP installed, record it as a project capability, not as a guarantee that the current connection is available.

Check:

- Whether `Packages/manifest.json` contains `com.njljh.unitymcp`, `UnityCaptain`, or a team-custom Unity MCP package.
- Whether `Assets/UnityCaptain/Resources/UnityCaptainSettings.asset` exists.
- `TcpPort`, `HttpPort`, `StartupServer`, and `ToolsListDefaultProfile` recorded in settings.
- Whether the current Agent session can see `unity-captain-skill-index` or `mcp__unitymcp` tools.

Recording rules:

- Installed in project but current MCP unavailable: write "installed, current connection unknown / unavailable"; run a connection check before later Unity writes.
- Current MCP available: write "installed and available in the current session"; dispatch later Unity writes through `unity-captain-skill-index` first.
- Not installed: write "not detected"; later use files, Unity batchmode, project scripts, or manual Editor verification.

## Verification Methods

Check whether the project supports:

- C# compilation
- EditMode tests
- PlayMode tests
- batchmode commands
- CI scripts
- Build scripts
- UnityCaptain / UnityMCP verification

If test result output is abnormal, record it only as a risk during setup and hand it to `game-qa-debug`.
