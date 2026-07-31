---
name: gameplay-balance-tuning
description: Use for tuning game values, difficulty curves, spawn rates, hit detection, fairness, economy, and rewards.
---

# Gameplay Balance and Difficulty Tuning

## When To Use

Use this skill when the task involves game parameters, difficulty curves, spawn frequency, hit detection, fairness, economy sources/sinks, rewards, cooldowns, health, scoring, failure rate, win rate, pacing, or player feedback such as "unfair / too hard / too slow / too fast / not fun."

## When Not To Use

- Rules are not yet settled; use `game-design-grilling` or `game-gdd` first.
- There is not yet a tunable feature task; use `game-feature-slicer` first.
- The current issue is a specific bug or regression; use `game-qa-debug`.
- The user asked to change code directly; use `game-unity-implementation`, then use this skill to review parameters if needed.

## Inputs

- GDD, tasks, acceptance items, and current parameters.
- Playtest notes, recordings, logs, test results, player feedback, or device behavior.
- Target player, target platform, input method, match length, and explicit out-of-scope items.

## Workflow

1. Read the GDD, tasks, acceptance items, current parameters, tests/recordings/logs/player feedback.
2. Clarify this tuning pass's goal:
   - Fairness
   - Tension
   - Readability
   - Win rate / failure rate
   - Pacing
   - Economy sources/sinks
   - New-player forgiveness
3. Build the parameter table according to `references/BALANCE-STANDARD.md`.
4. Choose validation metrics based on the gameplay.
5. Output the smallest tuning plan, prioritizing parameters, curves, and tolerance ranges without requiring major system changes.
6. Create or update a tuning record document so design, implementation, and QA can see current values, sources, and validation status.
7. If tuning requires Unity runtime, scene, recording, PlayMode, device, or UnityCaptain evidence, mark the validation entry point.
8. Recommend updating feature items or acceptance items in task docs only after validation; keep unverified parameters pending.

Shared standard:

- Use `references/BALANCE-STANDARD.md` for parameter records, validation metrics, and rules for unverified parameters.

## Tuning Record Location

Prefer writing to a trackable document inside the project instead of only outputting in the conversation.

Default path:

```text
game-design/<game-slug>/balance/<feature-slug>/<task-id>-<topic>-tuning.md
```

Example:

```text
game-design/motion-dodgeball/balance/motion-dodgeball-mvp/03-ai-throwers-and-sandbag-trajectory-tuning.md
```

If the project already has parameter tables, economy tables, or a balance directory, use the existing location. If not, create `game-design/<game-slug>/balance/`. The tuning record must state parameter sources, such as code fields, ScriptableObjects, Prefabs, Scene YAML, config tables, test logs, or manual observation.

## Output Format

```text
Tuning goal:
Current evidence:
Tuning record:
Parameter table:
Validation metrics:
Smallest tuning plan:
Runtime/device validation needed:
Still pending:
```

## Completion Criteria

- The tuning goal is clear, and vague phrases like "harder" or "more satisfying" are not used as substitutes for metrics.
- Every recommended parameter has a current value, recommended range, tuning rationale, and validation method.
- A tuning record document has been created or updated, and the document can trace the source of current values.
- It explains which conclusions come from evidence and which are only assumptions to validate.
- A single playtest or one piece of feedback is not treated as a final balance conclusion.

## Quality Checks

- Do not change numbers directly by feel.
- Do not finalize values without a target player, platform, or input method.
- Do not disguise bugs, latency, misjudgment, or performance issues as balance problems.
- Do not attribute economy, reward, or difficulty problems to only one parameter; list interaction effects when needed.
- Do not introduce a complex config system just for tuning; first versions should prefer tables and small parameter ranges.

## Prohibited

- Do not directly modify code, configs, scenes, or task status.
- Do not check off acceptance items early.
- Do not write unverified parameters as final values.
- Do not use a single subjective playtest conclusion as a substitute for reviewable evidence.
