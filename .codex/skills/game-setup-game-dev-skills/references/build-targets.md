# Build and Verification Target Recording Rules

During setup, record how the current project is verified and built; do not fix build problems in this stage.

## Target Platforms

Record the platforms the project explicitly supports or plans to support:

- Unity Editor
- macOS / Windows / Linux
- Android
- iOS
- WebGL
- Console platforms

If target platforms are not clearly stated, record them as unknown or to be confirmed.

## Verification Priority

Record the project's currently available verification methods, sorted by reliability:

1. C# compilation.
2. EditMode tests.
3. PlayMode tests.
4. Dedicated test Scene.
5. Manual Play Mode checklist.
6. batchmode test or build command.
7. CI.
8. Device testing.

## Commands

If the project already has commands, record the full commands.

Common Unity command format:

```text
Unity -batchmode -quit -projectPath <path> -runTests -testPlatform EditMode -testResults <path>
```

Do not invent an unverified command and mark it as "available." Unverified commands should be marked as "suggested command."

## Risks

Record:

- Whether the test command can generate a result file.
- Whether build scripts exist.
- Whether Unity Hub login or a license is required.
- Whether a specific platform module is required.
- Whether any Scene or ProjectSettings files are collaboration hotspots.
