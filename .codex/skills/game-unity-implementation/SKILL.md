---
name: game-unity-implementation
description: Implement Unity features from a GDD, task, feature delta brief, or confirmed vertical slice within the existing Unity project structure while protecting existing behavior.
---

# Unity Feature Implementation

## Use Cases

Use this skill when the user asks to implement a Unity feature based on a confirmed task, GDD section, feature increment brief, or vertical slice. Implementation in an existing project must follow the existing structure, minimize the blast radius, and leave regression evidence.

## Non-Use Cases

- Requirements are still vague.
- System boundaries, module responsibilities, MonoBehaviour/plain C# division of labor, or test seams are unclear; use `game-unity-system-design` first.
- The current issue is a bug without a reproduction path; use `game-qa-debug` first.
- The current project is not Unity.

## Inputs

- GDD, feature increment brief, task, or acceptance criteria.
- Project setup documentation.
- Unity project structure.
- Local Markdown task contract: `../game-setup-game-dev-skills/references/TASK-CONTRACT.md`.

## Workflow

1. Confirm the current small game's `game-design/<game-slug>/`, read `game-design/<game-slug>/agents/*.md`, and confirm project structure, task rules, asset rules, and verification priority.
2. Read `GAME_CONTEXT.md` in the same directory and use that small game's terminology to understand the task.
3. Read the relevant GDD, parent `00-epic.md`, and current subtask under the same `game-design/<game-slug>/`; if the subtask references the parent epic, the parent epic must be read first.
4. Determine whether the user provided a single task file, a parent epic, or a whole task directory:
   - Single task file: implement only the scope covered by that file.
   - `00-epic.md` or task directory: first enumerate all subtasks in the directory, then continuously execute the implementable scope in dependency order; do not default to executing only the first subtask.
   - If the scope is too large or has high-risk external dependencies, first clarify "which subtasks will land in this automated round, and which are scheduled for device/asset/performance verification" before starting Unity changes.
5. Check the current subtask's coverage, dependencies, required features, acceptance criteria, and explicit out-of-scope items against the task contract. Build a tracking checklist of "feature item -> implementation evidence" and "acceptance item -> verification evidence" to avoid implementing later slices or missing implicit functionality. When values, difficulty, pacing, fairness, or economy parameters are involved, record the parameter table and hand it to `gameplay-balance-tuning` for review.
6. For existing projects, first list the change type: add, extend, replace, delete, stabilize, or hotfix; also list existing behavior that must not change, affected modules, and regression scope.
7. If the current feature spans gameplay rules, input, UI, scenes, Prefabs, assets, saves, or devices, and the system boundary is unclear, use `game-unity-system-design` first to output a minimal system design draft.
8. Inspect Unity structure: `Assets/`, `Packages/`, `ProjectSettings/`, asmdef files, tests, scenes, Prefabs, and ScriptableObjects.
9. Define the minimal vertical slice to implement and the minimal regression verification.
10. If this round needs Unity Editor evidence, scene/Prefab/asset writes, compile state, test results, or runtime verification, use `unity-captain-skill-index` as the UnityCaptain dispatch entry point.
11. Use "feedback method selection" to decide whether to write tests first, do scene verification first, or prepare a manual/device acceptance checklist first.
12. Before editing, state whether scenes, Prefabs, assets, configuration, save data, or old behavior will be affected.
13. Implement according to existing project patterns; do not opportunistically refactor unrelated systems for this increment.
14. Prefer placing gameplay rules and state logic in testable locations.
15. Scene, HUD, Prefab, or visual readability changes must use UnityCaptain screenshots, scene hierarchy, or component readback for visual acceptance; do not claim completion when screenshots are visibly chaotic, obstructed, or unreadable.
16. When verifying gameplay behavior, compare line by line against the task's "player-visible outcomes" and acceptance criteria. Do not only check whether a result object appears; also confirm that the source object, action feedback, spawn position, and causal relationship are correct. For example, "the thrower throws a sandbag" must not be downgraded to "a sandbag appears on the field."
17. For existing projects, also compare against the regression scope line by line to prove old behavior still holds or explain why it cannot be verified.
18. Run available checks: UnityCaptain verification, compile, EditMode tests, PlayMode tests, scene playtest, screenshots/screen recordings, device verification, or project commands.
19. Write task status back to Markdown according to the "task status synchronization rules": check feature items only when functionality is complete, and check acceptance items only when verification passes.
20. Summarize changed files, verification results, regression results, completed/pending feature items, and checked/unchecked acceptance items.

## Progression Modes

| Mode | Behavior |
|---|---|
| Step-by-step confirmation | Implement only the single task specified by the user or the scope confirmed for this round, then wait for confirmation. |
| Automated continuous progression | When the user provides a task directory, `00-epic.md`, or explicitly asks to "leave the rest to AI", continuously implement all implementable subtasks in dependency order. |

Automated continuous progression rules:

- Before implementation, the feature-level system design document must be read; if it is missing, first use `game-unity-system-design` to add an overall design for the whole feature / epic.
- During continuous implementation, still synchronize feature items and acceptance items one subtask at a time; do not batch-check them at the end.
- When external conditions such as real devices, cameras, platform builds, performance workstreams, or final asset production are unavailable, leave the corresponding tasks incomplete and explain the unblock conditions; do not block the preceding runnable loop.
- In automated mode, do not stop merely because "the scope includes multiple subtasks"; stop only when key decisions are missing, verification cannot be performed, external permissions/assets are missing, or changes would irreversibly affect scenes/assets.

## Feedback Method Selection

Do not mechanically treat Unity development as "write tests first for everything." First choose the feedback loop that best fits this round's deliverable.

Unified standards:

- Use `references/UNITY-ENGINEERING-STANDARD.md` for Unity engineering layering, MonoBehaviour, ScriptableObject, and seam judgments.
- Use `references/ARTIFACT-STANDARD.md` and `../game-setup-game-dev-skills/references/TASK-CONTRACT.md` for feature items, acceptance items, and output fields.
- Use `references/VERIFICATION-STANDARD.md` for verification evidence levels and record format.

| Change Type | Preferred Feedback Method |
|---|---|
| Deterministic rules, state machines, numeric calculations, cooldowns, scoring, health, countdowns | Write EditMode / unit tests first, confirm the red state, then implement. |
| MonoBehaviour wiring, input bridging, lifecycle coordination | Push testable logic down into plain classes; verify wiring with PlayMode, scene playtest, or component readback. |
| Scene layout, Prefab binding, HUD placeholders, asset references | UnityCaptain scene/component/prefab evidence, scene playtest, screenshots, or manual checklist. |
| Game feel, animation timing, visual readability, audio experience | PlayMode playtest, screen recording, target device, or manual acceptance; do not force TDD. |
| Cameras, motion input, performance, package size, device differences | Real device/device verification, Profiler, logs, screen recordings, or dedicated test scenes. |
| Values, difficulty curves, spawn frequency, hit areas, rewards, or economy | First record current parameters and verification metrics, then review with `gameplay-balance-tuning`. |

Requirements:

- For rule logic with a clear test seam, prefer test-first.
- For experiential issues without a stable test seam, do not write brittle tests for formality.
- Whether or not TDD is used, verification evidence or a clear blocking reason must be left.

## UnityCaptain Relationship

`game-unity-implementation` is only responsible for process judgment from game task to Unity project implementation. It does not maintain Unity MCP tool-routing details.

When this round needs Unity Editor evidence, enter `unity-captain-skill-index`; that skill owns specific tool routing, connection diagnosis, and fallback, and this skill does not duplicate it.

Applicable triggers:

- Need to read the current Scene, Selection, Prefab, components, Console, or compile state.
- Need to modify Scene, Prefab, ScriptableObject, asset import settings, or Unity Project Settings.
- Need to verify compilation, EditMode / PlayMode tests, runtime behavior, or scene state.
- Need to judge Unity serialization, references, Prefab bindings, asset impact, or Undo risk.

## Task Status Synchronization Rules

Local Markdown tasks use two checkbox layers: the feature implementation section indicates implementation status, and acceptance criteria or acceptance criteria with evidence indicate verification status. Field and evidence rules are authoritative in `../game-setup-game-dev-skills/references/TASK-CONTRACT.md`; "code is written" or "compile passed" must not replace acceptance item verification.

Execution requirements:

1. Before implementation, extract all `- [ ]` / `- [x]` feature items from the current subtask's feature implementation section.
2. Before implementation, extract all relevant `- [ ]` / `- [x]` acceptance items from the current subtask and parent `00-epic.md`.
3. Change a feature item from `- [ ]` to `- [x]` only when the corresponding code, scene, Prefab, asset, or configuration is complete and the modified location can be identified.
4. Every acceptance item must map to at least one evidence type: UnityCaptain compile/test/scene state, screenshot/screen recording, PlayMode/EditMode, device verification, manual flow, or clear blocking explanation.
5. Change an acceptance item from `- [ ]` to `- [x]` only when actual verification passes and the evidence source can be explained.
6. Items that are incomplete, unverified, partially passing, unable to run, or questionable must remain `- [ ]` and be listed in the final response.
7. If an acceptance item lacks an evidence type, add it after that acceptance item first; do not only check the checkbox.
8. Update the corresponding overall acceptance item or subtask index status in the parent `00-epic.md` only after all relevant feature items and acceptance items in the subtask pass.
9. When the input is a task directory or epic, synchronize one subtask at a time; do not complete the first task and then claim the directory or epic is complete.

Prohibited behavior:

- Do not automatically check acceptance items because feature items are complete.
- Do not batch-check gameplay acceptance because compilation passed.
- Do not check action-chain acceptance because an object exists; source, action, feedback, and causal relationship must be confirmed.
- Do not change unverified items to complete and then say "pending verification" in risks.

## Completion Criteria

- The requested behavior has been implemented.
- The relationship between the current task and parent `00-epic.md` has been confirmed.
- If the input is a task directory or epic, the full subtask scope has been explained and handled instead of only the first file.
- In automated continuous progression mode, all implementable subtasks have been handled in dependency order, and incomplete external verification or blocked tasks are clearly listed.
- Feature items and acceptance items in the current task Markdown have been synchronized according to evidence; unchecked items are explicitly listed.
- No future task beyond the current slice has been implemented.
- For existing projects, unchanged existing behavior, regression verification results, and unverifiable items are explained.
- Scene, Prefab, and asset impacts are clearly stated.
- Scenes/HUD/Prefabs have readable screenshots or structured UnityCaptain evidence.
- Gameplay behavior evidence covers the source, action, and causal relationship in the task text, not only result numbers or object existence.
- Verification has been run; if it cannot be run, the reason is explained.

## Output Format

```text
Implemented:
Task basis:
Modified files:
Scene/Prefab/asset impact:
Verification:
Regression verification:
Remaining risks:
```

## Quality Checks

- Use `references/UNITY-CHECKLIST.md`.
- Keep changes focused.
- Keep existing-project changes incremental, and do not opportunistically refactor unrelated old systems.
- Do not silently change old gameplay, old UI, old input, old levels, old configuration, save data, or asset references.
- Do not add a global Manager or global state without a clear reason.
- When UnityCaptain is available, do not bypass its Skill Store and tool routing to guess Unity state directly.
- When the current subtask has a parent epic, do not start implementation after reading only the subtask.
- When the user provides a task directory or epic path, do not implement only the first subtask and claim the directory is complete.
- Do not treat TDD as the only quality gate; game-experience changes must use an appropriate Unity/device/manual feedback loop.
- Feature items and acceptance items must be written back to task Markdown; feature `[x]` must trace to an implementation location, and acceptance `[x]` must have verification evidence.

## Prohibited

- Do not commit to git by default.
- Do not casually move large asset directories.
- Do not silently edit binary assets.
- Do not merge prototype code without review.
- Do not copy UnityCaptain's full tool list, route table, or parameter details into this skill.
