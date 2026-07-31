# Local Markdown Task Contract

This file is the authoritative source for the local Markdown task format in this skill library. `game-feature-slicer`, `game-task-triage`, `game-unity-implementation`, and project-level `game-design/<game-slug>/agents/task-workflow.md` only add stage-specific rules; they do not maintain a second field definition.

## Epic Tasks and Slice Tasks

| Type | Purpose | Default File |
|---|---|---|
| Epic task / Epic | Records the version goal, MVP scope, explicit out-of-scope items, overall acceptance, and child task index. | `game-design/<game-slug>/tasks/<feature-slug>/00-epic.md` |
| Child task / Slice | Records one implementable, verifiable, player-visible vertical slice. | `game-design/<game-slug>/tasks/<feature-slug>/<nn>-<slice>.md` |

A child task must reference its parent epic and state which part of the epic it covers. An isolated task without parent tracking cannot be used as evidence that a version loop is complete.

## Required Child Task Fields

| Field | Requirement |
|---|---|
| Parent epic | Write the epic name, child task ID, and coverage. |
| Player-visible outcome | Describe the result in terms the player can see, operate, feel, or verify. |
| Features to implement | Use `- [ ]` / `- [x]` checkboxes to track implementation state. |
| Implicit gameplay semantics | Core gameplay verbs must be broken into an action chain; if none exist, write "No additional implicit semantics." |
| Systems involved | Write gameplay, input, UI, state, assets, configuration, tests, or platform-related systems. |
| Assets involved | Write scenes, Prefabs, UI, audio, VFX, configuration, or placeholder assets; if there are no new assets, write "No new assets." |
| Acceptance criteria and evidence | Use `- [ ]` / `- [x]` checkboxes, with the evidence type inline in each item. |
| Dependencies | Write prerequisite tasks, assets, decisions, or device conditions. |
| Risks | Write risks that affect acceptance, rollback, or later implementation. |

## Acceptance Evidence Rules

Acceptance criteria must be written as reviewable results, with evidence type inline:

```text
- [ ] <verifiable result> (Evidence: screenshot/recording/PlayMode/device/automated test/manual flow)
```

Allowed evidence types include:

- Code or configuration check
- UnityCaptain scene, Prefab, component, Console, or screenshot evidence
- EditMode / PlayMode test
- Recording or screenshot
- Profiler, build log, or device record
- Manual flow, but the operation and expected behavior must be clear

Do not replace gameplay acceptance with only "code written", "object exists", or "compiles". Feature completion is not the same as acceptance passing.

## Checkbox Sync Rules

| Area | When It Can Be Checked |
|---|---|
| `Features to implement` | The corresponding code, scene, Prefab, asset, or configuration is complete, and the changed location can be identified. |
| `Acceptance criteria and evidence` | The item has passed verification using the evidence type written in that item, and the evidence source can be explained. |
| Parent epic overall acceptance | Update the matching parent item only after the related child task's functionality and acceptance are both complete. |

Items that are incomplete, unverified, partially passing, not runnable, or still uncertain must remain `- [ ]`, with the blocker or manual verification path stated.

## Slice Completeness

A valid child task must satisfy:

- It has a player-visible outcome.
- It connects to at least one runnable or inspectable state.
- It spans the necessary gameplay, UI, data/configuration, assets, and verification.
- The key action chain for each core gameplay verb appears in the feature work or acceptance.
- The usual granularity is 0.5-2 days and 1-2 core gameplay verbs.
- It is not split horizontally as "all code / all UI / all art / all tests."
