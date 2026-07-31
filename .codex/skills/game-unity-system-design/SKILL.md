---
name: game-unity-system-design
description: Use before Unity implementation when system boundaries, module interfaces, MonoBehaviour responsibilities, ScriptableObject data, input/UI/resource adaptation, or test seams are unclear.
---

# Unity System Framework Design

## Core Principle

This skill is not meant to split systems into tiny pieces. Its goal is to find "just enough" Unity boundaries before implementation. For small-game MVPs, compress modules by default and split them only when real complexity appears.

## Use Cases

Use this skill when a GDD, task, or acceptance criteria already exists, but Unity system boundaries, module responsibilities, interfaces, adapters, test seams, scene/Prefab wiring, or data configuration are still unclear before entering `game-unity-implementation`.

Typical triggers:

- A feature spans gameplay rules, input, UI, scenes, Prefabs, audio, assets, or saves.
- It is unclear which logic belongs in plain C# and which belongs in MonoBehaviour.
- Rule logic needs to be EditMode-testable while Unity scene presentation remains intact.
- A new Manager, Singleton, event bus, ScriptableObject state, or global service is being considered, but its benefits and risks are unclear.
- The same gameplay may later connect to player input, AI, motion input, screen-recording validation, or device input.

## Non-Use Cases

- Rules, player outcomes, or acceptance criteria are not decided yet; use `game-design-grilling`, `game-gdd`, or `game-feature-slicer` first.
- Task maturity is uncertain; use `game-task-triage` first.
- The task only adjusts values, difficulty, or economy parameters; use `gameplay-balance-tuning`.
- Existing implementation or a local diff needs review; use `game-unity-code-review`.
- An existing project needs a long-term maintenance risk audit; use `game-unity-architecture-audit`.
- The user asks to directly implement code; use `game-unity-implementation`, with a short design pass from this skill first when needed.

## Inputs

- GDD, task, parent `00-epic.md`, acceptance criteria, and explicit out-of-scope items.
- `game-design/<game-slug>/agents/*.md`, `game-design/<game-slug>/GAME_CONTEXT.md`, existing project directories, and naming conventions.
- Unity project structure: `Assets/Scripts`, asmdef files, scenes, Prefabs, ScriptableObjects, test directories, and asset loading method.

## Design Vocabulary

| Term | Game/Unity Meaning |
|---|---|
| Module | A system that can be understood and verified independently, such as throw rules, hit detection, input mapping, or settlement scoring. |
| Interface | The minimal entry point the caller needs to know; this does not necessarily mean writing a C# `interface`. |
| Seam | A boundary that can be replaced, tested, or verified, such as an input source, random number source, time source, throw strategy, or asset loading. |
| Adapter | A thin layer that connects Unity lifecycle, scene objects, device input, UI, audio, or asset systems to rule modules. |
| Deep module | A module with a small external entry point and real internal complexity, so callers do not need to know the details. |

## Workflow

1. Read the task, parent epic, GDD, project setup, domain context, and existing Unity structure.
2. Determine whether the input scope is a single task, `00-epic.md`, or a task directory; default to feature / epic as the system design scope unless the user explicitly asks to design only one task.
3. Clarify player-visible outcomes and out-of-scope items for this round to avoid designing beyond the current feature / epic.
4. First determine design depth; do not jump straight into a full module breakdown:
   - Lightweight mode: small-game MVP, single-scene prototype, implementable in 1-2 days, with no complex save/network/platform-specific work. By default, split into only 3-5 implementation modules and clearly state which concepts are merged for implementation.
   - Standard mode: a complete feature / epic spanning rules, input, UI, assets, and verification. Output a system tree, module boundaries, Unity wiring, and task coverage matrix.
   - Verification-first mode: external dependencies or uncertainties such as real cameras, third-party SDKs, platform builds, performance, saves, networking, or multiplayer sync. Split only the uncertain parts into spikes or verification designs; handle the rest in lightweight/standard mode.
5. Identify candidate modules. In lightweight mode, do not mechanically split modules by "rules/state/input/presentation/assets/configuration/persistence/verification"; prefer merging them into 3-5 implementable modules.
6. Split into an independent module only when at least one condition applies: independent runtime state, independent lifecycle, independent EditMode test value, clear replacement risk, or continued merging would make a MonoBehaviour obviously bloated and hard to verify.
7. For each module, list the necessary plain C#, MonoBehaviour components, Prefabs, ScriptableObjects, scene objects, or test doubles underneath it. In lightweight mode, use at most two levels and do not list every class.
8. Decide which logic belongs in plain C# and which only needs a MonoBehaviour adapter.
9. Define each module's minimal entry point: what input the caller provides, what output it receives, and who owns state.
10. Mark necessary seams: input, time, randomness, difficulty parameters, assets, devices, UI, scene objects, and test doubles; do not create seams where there is no real replacement or verification benefit.
11. Plan Unity wiring: scene objects, Prefabs, ScriptableObjects, Inspector fields, events, or lifecycle entry points.
12. Plan verification methods: EditMode, PlayMode, UnityCaptain scene/component readback, screenshots/screen recordings, device verification, or manual acceptance.
13. Create or update the system framework design document so module boundaries, Unity wiring, and verification paths are visible before implementation.
14. Output the overall system design draft and the continuous implementation boundary for entering `game-unity-implementation`; do not directly edit code.

## Progression Modes

| Mode | Behavior |
|---|---|
| Step-by-step confirmation | After generating the overall system design document, wait for user confirmation before implementation. |
| Automated continuous progression | After generating the overall system design document, hand the implementable scope directly to `game-unity-implementation` and complete tasks continuously in dependency order. |

Design depth and progression mode are separate decisions:

- Short design mode: when the feature is a small-game MVP, single-scene prototype, or the user is concerned about complexity, output only the complexity judgment, 3-5 modules, key seams, Unity wiring, and implementation order.
- Full design mode: output the full module tree and asset plan only when the feature spans multiple systems, external dependencies are uncertain, or long-term maintenance boundaries are unclear.

Automated continuous progression rules:

- By default, write one overall system design for a `feature-slug`, for example `game-design/<game-slug>/system-design/<feature-slug>/system-design.md`.
- Use a "task coverage matrix" inside the document to show which modules cover subtasks such as 01, 02, and 03.
- Split out separate spikes or verification designs only for independent uncertainties such as device verification, third-party plugins, real cameras, platform builds, or performance workstreams.
- Do not write disconnected system designs for 01, 02, and 03 separately; the same gameplay loop must share the state machine, input seams, UI wiring, and verification plan.

## System Framework Document Location

Prefer writing a traceable in-project document instead of only outputting in chat.

Default path:

```text
game-design/<game-slug>/system-design/<feature-slug>/system-design.md
```

Example:

```text
game-design/motion-dodgeball/system-design/motion-dodgeball-mvp/system-design.md
```

If the project already has an architecture, system design, or technical proposal directory, reuse the existing location; otherwise, create `game-design/<game-slug>/system-design/`.

## Unity Design Rules

- Use `references/UNITY-ENGINEERING-STANDARD.md` as the unified standard for Unity layering, MonoBehaviour, ScriptableObject, seams, and anti-overdesign rules.
- Use `references/VERIFICATION-STANDARD.md` as the unified entry point for verification planning.
- The design must map to the current task acceptance items and must not pre-build architecture for an imagined future large system.

## Output Format

```text
Design scope:
Task basis:
System framework document:

Complexity judgment:
- Mode: lightweight / standard / verification-first
- Reason:
- Maximum recommended module count for this round:

MVP compressed implementation shape:
| Implementation module | Included responsibilities | Reason not to split further for now |

System framework tree:
System
├── Module
│   ├── Class / MonoBehaviour / Prefab / ScriptableObject / scene object
│   └── Test double or verification entry point
└── Module

Module boundaries:
| Module | Responsibilities | External entry point | State owner | Unity wiring |

Key seams / adapters:
| Boundary | Purpose | Replacement/verification method |

ScriptableObject / Prefab / Scene plan:

Test and verification plan:

Task coverage matrix:
| Subtask | Covered modules | Implementation order | Special risks |

Boundary for entering game-unity-implementation:

Not recommended now:
```

In lightweight mode, the "system framework tree" should have at most two levels. Prioritize the compressed module table, Unity wiring, and key seams. Expand the full "system -> module -> class/component/asset" tree only in standard/verification-first mode.

## Completion Criteria

- The design can directly guide `game-unity-implementation`; it is not an abstract architecture description.
- The system framework design document has been created or updated so future implementers can read it directly.
- The current task has been classified as lightweight, standard, or verification-first design depth.
- In lightweight mode, the "MVP compressed implementation shape" is provided and clearly states which concepts should be merged for implementation.
- In standard/verification-first mode, the document includes a "system -> module -> class/component/asset" tree and does not stop at module names.
- Each module has responsibilities, entry point, state ownership, and Unity wiring notes.
- Key seams and verification methods are identified.
- The document uses feature / epic as the main scope and maps to subtasks through the task coverage matrix.
- It is clear which recommendations come from the current project structure and which are assumptions to verify.
- No new framework or large refactor beyond the current slice is introduced.
- Classes, C# interfaces, ScriptableObjects, and Prefabs list only what the current slice needs to create or explicitly reserve; systems that may be needed in the future are not written as architecture that must be implemented now.

## Quality Checks

- Do not copy Web/backend architecture terminology; recommendations must fit Unity, scenes, Prefabs, serialization, and the asset pipeline.
- Do not mistake "directory layering" for system design; explain call relationships, state ownership, and verification boundaries.
- Do not prohibit Managers, Singletons, ScriptableObjects, or events just because they appear; explain specific risks and alternatives.
- Do not abstract all logic into interfaces; create seams only when replacement, testing, device adaptation, or cross-module isolation is needed.
- Do not design implementation details so tightly that they cannot change; keep the smallest current-slice plan that can land.
- Do not split the system design for the same feature by subtask unless the user explicitly asks or the risks are independent.
- Do not treat "one class per noun" as system design; for small games, prioritize merging modules around the player loop and implementation speed.
- Do not turn every verification dimension into an independent module; split only when it changes implementation boundaries or acceptance paths.
- Do not create C# interfaces where there is no real replacement need.
- If the module count exceeds 5, explain why the modules cannot be merged; in lightweight mode, more than 5 modules should usually be rewritten.

## Prohibited

- Do not directly modify code, scenes, Prefabs, assets, or task status.
- Do not output a one-shot large rewrite plan.
- Do not expand the current task scope for the sake of "architecture correctness."
- Do not write unverified design judgments as completed facts.
