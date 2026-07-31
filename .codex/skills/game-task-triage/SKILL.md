---
name: game-task-triage
description: Before a game task or existing-project feature delta enters implementation, triage task maturity, impact area, regression scope, dependencies, resources, technical research, device validation, and blocker state.
---

# Game Task Triage

## When to Use

Use this skill when `game-design/<game-slug>/tasks/`, issues, a feature delta brief, GDD child tasks, or acceptance criteria already exist, but it is unclear whether the tasks can go directly into implementation. It is especially useful for task directories, a parent `00-epic.md`, multiple child tasks, new features in existing projects, and version goals with gameplay/resource/device dependencies.

## When Not to Use

- An existing project lacks a change goal, impact area, and regression scope; use `game-feature-iteration` or `game-brief` first to complete a feature delta brief.
- A 0-to-1 new game does not yet have a GDD or feature specification; use `game-brief` or `game-gdd` first.
- Feature tasks have not been sliced yet; use `game-feature-slicer` first.
- The user asks to fix a bug directly; use `game-qa-debug`.
- The user asks to review Unity project architecture; use `game-unity-architecture-audit`.

## Inputs

- Task directory, parent `00-epic.md`, individual task Markdown, or external issue.
- Feature delta brief, version goal, change request, or regression checklist for an existing project.
- GDD, `game-design/<game-slug>/GAME_CONTEXT.md`, `game-design/<game-slug>/agents/*.md`.
- Unity project structure, asset directories, target platform, and validation conventions.
- Local Markdown task contract: `../game-setup-game-dev-skills/references/TASK-CONTRACT.md`.

## Triage Statuses

| Status | Meaning | Next Step |
|---|---|---|
| Implementable | Scope, dependencies, resources, and validation method are clear enough. | Enter `game-unity-implementation`. |
| Needs design clarification | Player result, rules, states, failure conditions, or acceptance criteria are unclear. | Enter `game-design-grilling` or write back open questions. |
| Needs resources | Art, UI, audio, animation, Prefab, config, or placeholder strategy is unclear. | Enter `game-art-audio-pipeline`. |
| Needs system design | The task is implementable, but module boundaries, state ownership, MonoBehaviour/plain C# split, ScriptableObject strategy, or test seam is unclear. | Enter `game-unity-system-design`. |
| Needs balance tuning | Difficulty, pacing, fairness, hit zones, spawn frequency, economy, or reward parameters are unclear. | Enter `gameplay-balance-tuning`. |
| Needs regression scope | Existing-project change goal is clear, but old behaviors, affected modules, or regression validation scope are unclear. | Complete a feature delta brief or regression checklist, then continue triage. |
| Needs compatibility/migration confirmation | Save, config, remote config, player data, asset reference, or version compatibility risk is unclear. | Output compatibility checks or migration confirmation items; do not implement directly. |
| Needs release risk confirmation | Near release, already live, hotfix, or platform submission window, but release impact and rollback conditions are unclear. | Enter `game-release-liveops` or complete a release risk check. |
| Needs technical spike | Engine approach, plugin, input, save, networking, AI, toolchain, or performance risk is unclear. | Output a spike task; do not directly implement mainline functionality. |
| Needs device validation | Depends on camera, controller, remote, mobile device, Android TV box, performance, or platform differences. | Enter device/platform validation or `game-performance-build`. |
| Blocked | Missing a key decision, permission, resource, plugin, environment, or upstream task. | State the blocker and unblock conditions. |

## Workflow

1. Read the task source, parent `00-epic.md`, related GDD, project setup, and domain context.
2. Enumerate all tasks in the input scope; do not only inspect the first child task.
3. Check each task for:
   - Whether the player-visible result is clear.
   - Whether `features to implement` follows the checkable-list rules in the task contract.
   - Whether acceptance criteria are verifiable and state the evidence type inline according to the task contract.
   - Whether dependencies are closed.
   - Whether existing behaviors that must not change in an existing project are clear.
   - Whether affected modules, old feature entry points, UI, input, levels, configs, saves, and asset references are clear.
   - Whether regression scope maps to automated tests, UnityCaptain evidence, PlayMode, screenshots/recordings, device runs, or manual flows.
   - Near release or when already live, whether release window, rollback conditions, and hotfix risks are clear.
   - Whether resources, scenes, Prefabs, UI, audio, and configs are clear.
   - Whether Unity project entry points, test seams, UnityCaptain/build/scene validation paths are clear.
   - Whether system boundaries, module interfaces, state ownership, adapters, and test seams need design first.
   - Whether values, difficulty, pacing, fairness, hit zones, rewards, or economy parameters need tuning.
   - Whether device, platform, performance, input, or build risks need validation first.
4. Assign each task one primary status. If multiple statuses apply, choose the one that blocks implementation the most, and list secondary risks in notes.
5. For "Implementable" tasks, give the recommended execution order and the boundary for entering `game-unity-implementation`.
6. For tasks that are not "Implementable", output the smallest completion action; do not expand it into a full GDD rewrite.
7. Write statuses or open questions back only when the user explicitly asks to update local task documents; otherwise output only the triage report.

## Progression Modes

Triage must not fragment the same feature into a process where every child task waits for separate system-design confirmation.

| Mode | Behavior |
|---|---|
| Step-by-step confirmation | Output the triage report and wait for the user to confirm the next step. |
| Automated continuous progression | After outputting or recording the minimum triage conclusion, proceed directly to the next skill. If multiple tasks share missing system boundaries, mark the whole feature as "needs overall system design". |

Rules for automated continuous progression:

- When multiple child tasks belong to the same `00-epic.md` and share missing module boundaries, state ownership, Unity wiring, or test seams, do not block each one behind separate system design; give one next step: enter `game-unity-system-design` with feature/epic scope.
- Mark a single child task separately only when it has an independent external risk, such as real devices, camera plugins, performance builds, or resource gaps.
- Implementable tasks should include a continuous implementation order and implementation boundary, not only a recommendation for the first task.
- Block automated progression only when key design decisions, external resources, device permissions, plugin environment, or upstream tasks cannot be closed.

## Parallel Triage

When there are many tasks or independent risk axes, multiple subagents may evaluate in parallel, but the main agent must merge the conclusions and own the final judgment.

Recommended parallel perspectives:

| Subagent Perspective | Focus |
|---|---|
| Design/acceptance | Player result, rules, action chain, acceptance criteria, implicit gameplay semantics. |
| Unity/technical | Code entry points, scenes/Prefabs, test seams, UnityCaptain validation, plugins, and toolchain. |
| System design | Module boundaries, interfaces, state ownership, MonoBehaviour adapters, ScriptableObject, and testable rules layer. |
| Assets/content | Art, UI, audio, animation, placeholder assets, import, and naming. |
| Balance/difficulty | Parameters, curves, frequency, hit zones, fairness, rewards, and economy sources/sinks. |
| Device/platform | Input devices, target platforms, performance, build, real-device or big-screen validation. |

Parallel rules:

- Subagents only read and report; they do not directly modify tasks or project files.
- Each subagent outputs a status recommendation, evidence, and blockers.
- The main agent uses the highest-risk status when merging; for example, if a task has clear design but lacks device validation, mark it as "Needs device validation".
- If subagent conclusions conflict, list the reason for the conflict and the smallest action needed to obtain evidence.

## Output Format

```text
Triage scope:

Task status table:
| Task | Status | Main Reason | Next Step | Boundary for Entering Implementation | Regression Scope |

Immediately implementable:

Needs completion:

Recommended execution order:

Open questions:
```

## Standard References

- Task fields, feature items, and acceptance items are governed locally by `../game-setup-game-dev-skills/references/TASK-CONTRACT.md`.
- Implementability judgment and status priority use `references/TRIAGE-STANDARD.md`.

## Completion Criteria

- Every task has exactly one primary status.
- "Implementable" tasks have a clear implementation boundary and validation entry point.
- Existing-project tasks state unchanged existing behaviors, impact scope, and regression scope.
- Tasks that are not "Implementable" provide the smallest completion action.
- Tasks with unclear design, missing resources, or unknown devices are not forced into implementation.

## Quality Checks

- Do not turn task triage into task re-slicing; if slicing is invalid, only point out the need to return to `game-feature-slicer`.
- Do not use "feels complex" as a blocker reason; state the missing fact.
- Do not assume a task is implementable just because it has acceptance criteria; also check resources, dependencies, Unity entry points, and validation evidence.
- Do not skip old behavior, compatibility, and regression scope just because the change is "small".
- Do not block every task just because there is risk; if implementation can proceed while validating, state the boundary clearly.
- Do not split system design for the same feature into fragmented single-task designs unless the risk sources are genuinely independent.

## Prohibited

- Do not directly implement Unity features.
- Do not close, complete, or delete tasks without authorization.
- Do not bulk-edit task statuses unless the user explicitly asks for write-back.
- Do not let a subagent decide the final status alone.
