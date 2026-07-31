# Feature Source Readiness Check

Before creating `00-epic.md` or child tasks, use this file to decide whether the design source is ready for `game-feature-slicer`. The goal is not to fill in a complete design, but to avoid slicing a vague idea into the wrong tasks.

## Check Method

Mark each item as:

| Status | Meaning |
|---|---|
| Ready | The information is enough to support slicing. |
| Continue with risk | The gap will not change slice boundaries and can be recorded as a risk or open question. |
| Blocked | The gap would cause incorrect slices, incorrect acceptance, or an incorrect project path. |

Only stop slicing when an item is `Blocked`; do not repeatedly interrupt the user for small gaps.

## Required Checks

| Check | Ready Standard | Fallback When Blocked |
|---|---|---|
| Player-visible outcome | The player can see, operate, feel, or verify the new result produced by this feature. | `game-brief` |
| Core gameplay verb | The actor, target action, and result are clear, such as move, aim, throw, erase, settle, or upgrade. | `game-design-grilling` |
| Action-chain basis | The core gameplay verb can at least imply key steps such as prep/warning -> execution -> feedback -> result tracking. | `game-design-grilling` |
| MVP scope | What is in scope and out of scope for this pass is clear, and future full-system work is not mixed into the current slice. | `game-brief` or `game-gdd` |
| UI / feedback constraints | Key HUD, prompts, settlement, failure feedback, or readability requirements are known; it is also explicit when no new UI is needed. | `game-gdd` or `game-design-grilling` |
| Asset strategy | Required scenes, Prefabs, characters, audio, VFX, configuration, or placeholder asset strategy is known. | `game-brief` or `game-art-audio-pipeline` |
| Input / camera / platform | Input method, camera/view, target platform, and device constraints will not change task boundaries. | `game-brief` |
| Technical constraints | Engine, project structure, main existing systems, and no-touch boundaries are known. | `game-setup-game-dev-skills` or `game-task-triage` |
| Acceptance direction | The main evidence type is at least known: automated test, PlayMode, screenshot, recording, device verification, or manual flow. | `game-design-grilling` |

## Required Checks for Existing-Project Increments

If the source is a feature-increment brief, issue, or verbal change request, also check:

| Check | Ready Standard | Fallback When Blocked |
|---|---|---|
| Added / changed scope | Slice only the player outcome added or changed in this pass; do not reslice the whole game. | `game-brief` |
| Existing behavior that must not change | It is explicit which old flows, feel, data, or assets must not be broken. | `game-brief` |
| Impact surface | Affected scenes, Prefabs, UI, input, assets, configuration, saves, or build targets are clear. | `game-task-triage` |
| Regression scope | It is known which old tasks, scenes, or flows need regression checks. | `game-task-triage` |

## Routing Rules

- Missing player outcome, MVP scope, platform, input, or out-of-scope boundaries: return to `game-brief`.
- Overall design direction exists, but systems, UI, assets, or acceptance are still not enough to support task slicing: return to `game-gdd`.
- Terminology, gameplay verbs, action chain, boundary tradeoffs, or genre semantic conflicts: use `game-design-grilling`.
- Existing project is missing unchanged behavior, impact surface, or regression scope: first use `game-brief` to produce a feature-increment brief; if a brief exists but engineering risk is unclear, use `game-task-triage`.
- Only asset naming, style, import settings, or AI asset prompts are missing: do not block slicing; mark the asset risk and hand asset detailing to `game-art-audio-pipeline`.

## Output Requirements

If continuing with slicing, briefly write this before the slicing plan:

```text
Input quality conclusion: Ready / Continue with risk
Main basis:
Remaining risks:
```

If stopping slicing, output:

```text
Input quality conclusion: Blocked
Blocking gaps:
Recommended fallback skill:
Minimum information needed:
```
