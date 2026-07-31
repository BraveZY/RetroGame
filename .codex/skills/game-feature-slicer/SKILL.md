---
name: game-feature-slicer
description: Split a GDD, feature spec, feature delta brief, or version goal into verifiable, schedulable, implementable game feature tasks.
---

# Game Feature Slicing

## When to Use

Use this skill when a confirmed GDD, feature specification, feature delta brief, or version goal needs to be split into executable feature tasks. In an existing project, slice only the new or changed scope; do not re-slice the entire project.

## When Not to Use

- The design has not been confirmed.
- The user only needs an asset list for one discipline.
- The current issue is a bug; use `game-qa-debug`.

## Inputs

- GDD, feature specification, feature delta brief, or version goal.
- Project task-management conventions.
- Engine and project constraints.
- Local Markdown task contract: `../game-setup-game-dev-skills/references/TASK-CONTRACT.md`.
- Pre-slicing input quality check: `references/FEATURE-SOURCE-READINESS.md`.

## Workflow

1. Read the design source and setup documents.
2. Run the pre-slicing input quality check according to `references/FEATURE-SOURCE-READINESS.md`; if gaps would cause incorrect slicing, fall back to the recommended upstream skill first.
3. Identify the player-visible result.
4. If the source is an existing-project feature delta brief, first lock the added/changed scope, unchanged existing behaviors, affected modules, and regression scope.
5. Extract implicit gameplay semantics from game verbs, actor responsibilities, and genre conventions, then break them into action chains.
6. First draft an epic-level task according to the task contract, describing the version goal, scope, acceptance baseline, and child task list.
7. Then draft feature slice tasks. Each task should connect gameplay, UI, data/config, resources, and validation as needed; the deliverable must be a verifiable vertical gameplay slice.
8. Existing-project child tasks must state the affected existing systems, unchanged existing behaviors, and regression checks.
9. Every child task must reference the epic and explain which part of the epic it covers.
10. Put prerequisite refactor or infrastructure tasks first only when they truly block later slices.
11. Use the "Slice Completeness Check" to filter tasks; delete or merge horizontal tasks that have no player-visible result.
12. Show the "epic + child task list" slicing plan and confirm whether the granularity and dependencies are reasonable.
13. After user confirmation, write to issues or local Markdown according to the task-system convention.

## Progression Modes

When moving from GDD into task slicing, distinguish between two progression modes:

| Mode | Behavior |
|---|---|
| Step-by-step confirmation | Show the slicing plan first, then wait for user confirmation before writing task files. |
| Automated continuous progression | If the user clearly wants the AI to continue downstream work, the slicing plan is reasonable, and no critical blocker is triggered, write the task files directly and hand the next step to `game-task-triage`. |

Boundaries for automated continuous progression:

- Still create `00-epic.md` and traceable child tasks; do not only list a plan in chat.
- Do not skip parent tracking, acceptance evidence, involved resources, or action chains because the flow is automated.
- Do not split system design into isolated designs for each child task; the next step should enter `game-unity-system-design` at feature/epic scope.
- Stop for confirmation only when the GDD lacks key gameplay decisions, the placeholder resource strategy is unknown, the task granularity is clearly unreasonable, or the work would be written to the wrong `<game-slug>`.

## Implicit Gameplay Semantics

Game feature specs often use short phrases that omit default action details. During slicing, core gameplay verbs must be broken into action chains, and the key links in the chain must be written into "features to implement" and "acceptance criteria".

Action chain template:

```text
Subject -> intent/objective -> preparation/aiming/warning -> spawn/execute -> trajectory/feedback -> target response -> result stats
```

General examples:

- "Enemy fires a projectile" cannot be written only as "spawn a projectile". The task should cover enemy selection/movement or target aiming, attack warning, projectile spawning from the enemy or firing point, projectile travel toward the target or target area, and target hit/dodge feedback.
- "Player avoids danger" cannot be written only as "character movement". The task should cover input changes, position mapping, boundary limits, danger warnings, successful dodge feedback, and stat changes.
- "Level results and restart" cannot be written only as "show results". The task should cover stopping active field objects, showing run data, and resetting time/lives/stats/remaining objects.

Handling rules:

- If genre convention is clear and small in scope, write it into the child task.
- If genre convention would expand scope or affect art/interaction plans, write it as a risk or open question; do not silently downgrade it.
- Do not replace acceptance for the full action chain with only the final result of the chain.

## Slice Completeness Check

A child task must be able to answer at least these questions:

| Check Item | Passing Standard |
|---|---|
| Player-visible result | The player can see, operate, feel, or verify a new experience result. |
| Gameplay loop | The task is not only code-layer, UI-layer, or asset-layer work; it connects to at least one runnable state. |
| Involved resources | Clearly states whether scenes, Prefabs, UI, sound effects, visual effects, configs, or placeholder resources are needed. |
| Existing-project impact | Existing projects must state affected existing systems, unchanged behavior, and regression checks. |
| Implicit gameplay semantics | Core gameplay verbs have been broken into action chains, and key links are written into functionality and acceptance. |
| Functional completion items | "Features to implement" uses checkboxes and can distinguish pending work from completed work. |
| Acceptance criteria | Covers verifiable results for the core action chain, with evidence type written inline for each item. |
| Parent tracking | Can be traced back to the version goal and child task index in `00-epic.md`. |

## Feature Slice Granularity

Feature slicing is not scheduling code, UI, assets, and tests separately. It means splitting a player-perceivable piece of experience until it can be verified independently.

Acceptable granularity:

- One task usually maps to a demonstrable result that can be completed in 0.5-2 days; split further if it exceeds that.
- One task should carry at most 1-2 core gameplay verbs; if it has more, prefer multiple continuous slices.
- After splitting, the result can still be seen, operated, or verified by the player; if only code-layer or asset-layer work remains, it has been split too far.
- Every acceptance criterion must state its evidence type inline, such as screenshot, recording, PlayMode, device, automated test, or manual flow.
- "Features to implement" is an implementation progress checklist, not proof of acceptance; completed implementation can check off feature items, but passing acceptance requires separate evidence.

Pre-slicing gates:

- The player-visible result is clear.
- MVP scope and explicit out-of-scope items are clear.
- Input, camera/view, platform, placeholder resource strategy, and major technical constraints are known.
- If these are not satisfied, output open questions first instead of forcing feature slicing.
- When an existing project uses a feature delta brief as its source, a full GDD rewrite is not required, but the change goal, impact scope, unchanged behavior, and regression scope must be clear.

Examples of poor slicing:

| Poor Task | Problem | Fix |
|---|---|---|
| Write all player code | Code layer only, with no player-visible result. | Change to "the player can move and is constrained by boundaries". |
| Build all UI | Horizontal UI task that cannot independently prove the gameplay is playable. | Change to "ready state, countdown, and HUD placeholders run correctly". |
| Produce all art assets | Asset delivery is not a gameplay loop. | Change to "complete a specific scene or feedback slice with placeholder assets". |
| Build all base framework | Too large and not acceptably verifiable. | Keep only the smallest prerequisite task that truly blocks later slices. |
| Punching bag system / player system / results system do not reference each other | Missing version-goal tracking. | Use the epic index to connect dependencies and the acceptance baseline. |
| Enemy fires projectiles: only says "spawn projectile and make it fly" | Missing enemy movement/aim/attack and projectile source. | Write it as "enemy moves/aims at target, warns, spawns projectile from enemy or firing point, and sends it toward the target area". |

## Completion Criteria

- Every task has a demonstrable or verifiable result.
- Each task's "features to implement" can track pending/completed work with checkboxes.
- There is an epic task that summarizes the version goal and references all child tasks.
- Every child task can be traced back to the epic.
- Dependencies are clear.
- Existing-project tasks cover only the added/changed scope and include regression checks.
- Horizontal tasks such as "finish all UI" or "write all code" do not appear unless they are genuine prerequisite tasks.

## Output Format

Use `references/TASK-EPIC.md` for the epic first, then use `references/VERTICAL-SLICE.md` for feature slice tasks. Field and checkbox sync rules are governed by `../game-setup-game-dev-skills/references/TASK-CONTRACT.md`.

Recommended local Markdown directory:

```text
game-design/<game-slug>/tasks/<feature-slug>/
  00-epic.md
  01-<slice>.md
  02-<slice>.md
```

`<game-slug>` must correspond to the current mini-game name. When one Unity project contains multiple mini-games, do not write tasks to a flat `game-design/tasks/` directory.

## Quality Checks

- Prefer thin but complete gameplay paths over horizontal layers.
- Confirm input quality before slicing; when the design source is insufficient, do not guess key gameplay, constraints, or regression scope.
- The epic only manages goals, scope, acceptance, and indexing; it does not carry concrete implementation details.
- Do not re-slice an entire existing project; slice only this added or changed scope.
- Every task must include acceptance criteria.
- Every task must state evidence type inside its acceptance criteria; do not separately repeat "acceptance evidence" and "suggested verification method".
- Every task's "features to implement" must be written as checkboxes; do not use ordinary stateless bullets.
- Every child task must state involved systems and involved resources; if there is no resource impact, still write "no new resources".
- Every core gameplay verb must be broken into an action chain; acceptance criteria cannot cover only the final object or number.
- Slices may use placeholder resources, but later replacement risk must be stated.
- Every existing-project child task must state old behavior protection and regression checks.

## Prohibited

- In step-by-step confirmation mode, do not publish tasks before the user confirms the slicing. In automated continuous progression mode, task files may be written directly after the plan passes self-check.
- Do not close or modify parent tasks.
- Do not create isolated child tasks without an epic reference.
- Do not split "code, UI, art, audio, testing" into horizontal tasks that do not produce player experience.
