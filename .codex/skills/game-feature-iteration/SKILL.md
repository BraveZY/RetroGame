---
name: game-feature-iteration
description: Use when an existing game or Unity project needs a new feature, feature iteration, mid/late-stage stabilization, hotfix triage, or content/value adjustment without restarting the full brief/GDD workflow.
---

# Game Feature Iteration

## Core Principle

This is an incremental change to an existing project, not a redesign of the game. First confirm the existing context, impact area, and regression scope, then decide whether to add a feature delta brief, triage tasks, design systems, implement, review, or run QA.

This skill is a thin entry point. It only identifies the project stage, fills in incremental context, and routes to downstream skills. It does not replace `game-captain` or any specialist skill.

## When To Use

Use this skill when the user describes incremental work on an existing game, developed project, work in progress, near-release build, live version, or existing Unity project.

Typical triggers:

- Add or change a feature in an existing game, or connect work to a version goal.
- Mid- or late-development small changes, stabilization, polish, and regression.
- Change control before release, beta testing, or store submission.
- Live or near-release hotfixes.
- Tune only values, assets, levels, UI, audio, or content without reworking core design.

## When Not To Use

- A brand-new game idea whose core experience is not clear; use `game-captain` or `game-brief`.
- Clear bug reproduction and diagnosis; use `game-qa-debug` directly.
- The user only asks for a diff review; use `game-unity-code-review` directly.
- The user has provided a mature task, asks for direct implementation, and the impact area is very small; you may go directly to `game-unity-implementation`.

## Inputs

- The user's current change request, issue, version goal, or task.
- Existing `game-design/<game-slug>/GAME_CONTEXT.md`, GDD, tasks, feature delta briefs, version notes, or QA records.
- Unity project structure, code, scenes, Prefabs, assets, config, saves, and target platforms.

## Stage Judgment

| Stage | Signals | Main Path |
|---|---|---|
| Feature iteration | Existing game; add or change a feature. | `game-task-triage -> game-unity-system-design(if needed) -> game-unity-implementation` |
| Stabilization | Features are mostly complete; focus is pre-release small changes, polish, and regression. | `game-task-triage -> game-unity-implementation -> game-unity-code-review -> game-qa-debug(if needed)` |
| Hotfix | Live, near release, or the issue has clear symptoms. | `game-qa-debug -> game-unity-implementation -> game-release-liveops` |
| Content / value iteration | Only tune values, difficulty, assets, levels, audio, or content. | `gameplay-balance-tuning` or `game-art-audio-pipeline`, then triage/implementation as needed |

## Workflow

1. First confirm whether this is an incremental change to an existing project. If it is, do not default back to a full GDD.
2. Read existing project context: setup, `GAME_CONTEXT.md`, related GDD/tasks/issues, code entry points, scenes, Prefabs, assets, and config.
3. Judge the project stage and provide one short path.
4. Check whether enough incremental context already exists:
   - The player outcome to add or change.
   - Existing behavior that must not change.
   - Affected modules, assets, scenes, Prefabs, config, saves, and platforms.
   - Regression scope and verification evidence.
5. If context is insufficient, first create or fill in a feature delta brief. Do not expand it into a full project brief.
6. If task maturity is unclear, route to `game-task-triage`.
7. If module boundaries, state ownership, MonoBehaviour/plain C# split, or test seams are unclear, route to `game-unity-system-design`.
8. If the work is already implementable, route to `game-unity-implementation`.
9. For mid/late-stage work, near-release work, or changes that affect old behavior, require `game-unity-code-review` and regression evidence by default.
10. For hotfixes, start from symptoms and reproduction through `game-qa-debug`, then run release/liveops checks after the fix.

## Feature Delta Brief

When the user only says something like "add a feature" or "change this part" but impact area and acceptance are missing, output or create a lightweight delta brief:

```markdown
# <feature-or-change-name> Feature Delta Brief

| Item | Content |
|---|---|
| Current game/version state |  |
| Player outcome to add or change |  |
| Existing behavior that must not change |  |
| Affected modules |  |
| Scene/Prefab/asset/config impact |  |
| Save/data/compatibility impact |  |
| Explicitly out of scope |  |
| Regression scope |  |
| Acceptance direction |  |
| Recommended next skill |  |
```

When saving, prefer:

```text
game-design/<game-slug>/briefs/YYYY-MM-DD-<feature>-delta.md
```

## Regression Scope

Every incremental change must at least clarify:

- Which behaviors in old features must remain unchanged.
- Which scenes, Prefabs, UI, input, config, save, or platform paths may be affected.
- Which automated tests, UnityCaptain evidence, PlayMode checks, screenshots/videos, devices, or manual flows can prove there was no regression.
- Which regressions cannot be verified yet, and what would unblock verification.

## Output Format

```text
Stage judgment:
Recommended flow:
Feature delta brief to fill in:
Regression scope:
Gaps before starting:
```

## Completion Criteria

- It is clear whether this is an existing-project incremental change or needs to return to zero-to-one design.
- One short skill path has been provided.
- It is clear whether a feature delta brief is needed.
- Regression scope, impact area, and key gaps have been listed.
- The existing project was not defaulted back to a full GDD.

## Quality Checks

- Do not perform implementation, review, or debugging in place of downstream skills.
- Do not ignore old behavior and regression just because the user says "add a feature".
- Do not expand mid/late-stage small changes into a full redesign.
- Do not turn a hotfix into a design discussion first; hotfixes prioritize reproduction, fix, regression, and release risk.
