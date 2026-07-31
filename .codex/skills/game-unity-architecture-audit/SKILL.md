---
name: game-unity-architecture-audit
description: Audit Unity project structure, system boundaries, scene/Prefab coupling, test seams, resource loading, long-term maintenance risks, and regular codebase health when the user asks for an architecture audit, engineering health check, technical-debt priorities, or the most valuable structural improvements.
---

# Unity Architecture Audit

## Use Cases

Use this skill when the user asks to inspect Unity project architecture, module boundaries, long-term maintenance risk, codebase health, technical debt priority, or when the project shows symptoms such as bloated Managers, tightly coupled scenes, confusing Prefab references, difficult testing, or ScriptableObject state misuse.

It also applies to requests such as "codebase health", "regular checkup", "where is the best place to improve", or "find structural problems like matt's improve-codebase-architecture."

## Non-Use Cases

- Only reviewing the current diff; use `game-unity-code-review`.
- The current issue is a specific bug, build failure, or performance regression; use `game-qa-debug` or `game-performance-build`.
- The current need is pre-implementation design of system boundaries, interfaces, or test seams; use `game-unity-system-design`.
- The user asks to directly implement a feature; use `game-unity-implementation`.
- The project is not Unity.

## Inputs

- Unity project directory.
- `game-design/<game-slug>/agents/*.md`, `game-design/<game-slug>/GAME_CONTEXT.md`, GDD, tasks, or version goals.
- `Assets/`, `Packages/`, `ProjectSettings/`, asmdef files, scenes, Prefabs, ScriptableObjects, and test directories.

## Audit Modes

| Mode | When to Use | Output Focus |
|---|---|---|
| Targeted audit | The user specifies a system, scene, Prefab, module, or feature flow. | Find specific architecture risks and the fix order within the scope. |
| Regular health check | The user asks for codebase health, technical debt ranking, engineering checkup, or the most worthwhile structural improvements. | Find the Top 1-3 improvement opportunities, ranked by benefit, risk, and verification cost. |

## Workflow

1. First read the project setup documentation and domain context, and confirm the target platform, task directories, asset conventions, test conventions, and boundaries that must not be touched.
2. Clarify the audit mode and scope: targeted audit or regular health check; whole project, one system, one scene, a group of Prefabs, or one feature flow.
3. Inspect Unity structure signals: `Assets/Scripts`, asmdef files, `Assets/Scenes`, Prefabs, Resources/Addressables, ScriptableObjects, Editor tools, Tests, and Project Settings.
4. When UnityCaptain is available and the audit involves scenes, Prefabs, components, serialized references, or Console state, prefer `unity-captain-skill-index` for structured evidence; explain the fallback when unavailable.
5. Record findings along audit axes:
   - System boundaries: whether gameplay, input, UI, assets, data, and state machines are mixed together.
   - MonoBehaviour responsibilities: whether lifecycle, input reading, state mutation, and presentation updates are overly concentrated.
   - Global state: whether Managers, Singletons, static variables, or DontDestroyOnLoad create implicit dependencies.
   - Scenes and Prefabs: whether references are fragile, Prefabs carry business state, or scenes are hard to reuse.
   - Data and configuration: whether ScriptableObjects are used for configuration or as implicit global mutable state.
   - Asset loading: whether Resources, Addressables, AssetBundles, or dynamic loading carry platform and memory risks.
   - Test seams: whether rule logic can be tested outside the Unity lifecycle, and whether PlayMode/EditMode boundaries are clear.
   - Performance and build: whether Update, GC, lookups, reflection, shaders/textures/audio, or platform settings carry architectural risks.
6. Express each finding as "evidence -> risk -> recommendation" and mark severity.
7. In health check mode, first aggregate candidate issues, then select only the Top 1-3 most worthwhile improvement opportunities; put the rest under "Not recommended now."
8. Break recommendations into small verifiable changes; do not output a one-shot large rewrite plan.

Unified standards:

- Use `references/UNITY-ENGINEERING-STANDARD.md` for Unity architecture judgments.
- Use `references/AUDIT-STANDARD.md` for severity, finding evidence, and verification methods.

## Completion Criteria

- The report cites evidence from specific files, scenes, Prefabs, components, or project settings.
- Real architecture risks are distinguished from personal preference.
- Priority, impact scope, recommended changes, and verification methods are provided.
- In health check mode, the report explains why the Top 1-3 improvement opportunities are worth doing now.
- Recommendations can be split into small tasks and do not require refactoring the whole project at once.

## Output Format

```text
Audit scope:
Audit mode:
Project facts:
High-priority findings:
Medium/low-priority findings:
Top 1-3 improvement opportunities:
| Improvement opportunity | Evidence | Benefit | Risk | Recommended small tasks | Verification method |
Recommended change order:
Items needing UnityCaptain/runtime verification:
Not recommended now:
```

## Quality Checks

- Do not copy Web/backend architecture patterns; recommendations must fit Unity, scenes, Prefabs, serialization, and the asset pipeline.
- Do not mark Managers, Singletons, or ScriptableObjects as wrong just because they exist; explain the specific risk.
- Do not recommend DI, ECS, event buses, state machine frameworks, or new packages unless pain points and benefits are clear.
- Do not treat directory cleanup as architecture improvement; it must reduce real coupling, regression, verification, or platform risk.
- Do not mix code style issues with runtime risks.
- In health check mode, do not output a long technical debt list; prioritize a few issues that truly improve delivery speed, regression risk, or verification cost.

## Prohibited

- Do not directly modify code, scenes, Prefabs, or assets.
- Do not rename or move large batches of files.
- Do not use "best practices" as a substitute for project context.
- Do not output generic recommendations without evidence.
- Do not turn a health check into a full refactoring plan.
