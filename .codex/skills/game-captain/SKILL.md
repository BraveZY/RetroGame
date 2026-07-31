---
name: game-captain
description: Default entry point for the game-development skill library. Use it when the user does not know which specialist skill to choose; it routes the request into the right downstream skill flow.
---

# Game Development Captain

## When To Use

Use this skill when the user makes a game development request but it is not yet clear whether the work should enter design, implementation, assets, debugging, build, or release flow.

This skill is the lowest-memory-cost default entry point for the user: the user can call only `game-captain`, and it decides which specialist skills to use next. Other skills keep their independent responsibilities, but they are primarily recommended or handed off as downstream flows from this skill.

## When Not To Use

- The user has already explicitly invoked a specific game skill, and that skill fully covers the current request.
- The request is already very clear and only needs a small scoped change to specified files.

## Inputs

- The user's current request.
- Current project context.
- Existing `game-design/<game-slug>/agents/*.md`, `game-design/<game-slug>/GAME_CONTEXT.md`, GDD, task documents, or Unity project files.

## Workflow

1. First judge whether the request already has executable context:
   - Needs research on competitors, market, platform rules, SDKs/plugins, technical direction, asset references, or current external information: prioritize `game-research`, then route to design, slicing, implementation, assets, or release skills based on findings.
   - Needs creative divergence, more ideas, stronger hooks, mechanic novelty, meme-to-game judgment, body-input concept exploration, or explicit brainstorm wording: prioritize `game-brainstorm`.
   - Existing game or Unity project that needs incremental features, mid/late-stage small changes, stabilization, hotfixes, or content/value iteration: prioritize `game-feature-iteration`.
   - Existing GDD or version goal, but levels, tasks, enemy/obstacle combinations, pacing curves, or content reuse are unclear: route to `game-level-content-design`.
   - Existing questions about progression, rewards, sinks, unlocks, resource flow, retention motivation, monetization, or ad boundaries: route to `game-economy-progression-design`.
   - Existing questions about first-time experience, onboarding, first-run learning path, error prompts, or failure recovery: route to `game-onboarding-tutorial-design`.
   - Existing GDD / feature spec: may route to `game-feature-slicer`; if it is only a local addition or change inside an existing project, route to `game-feature-iteration` first.
   - Existing parent tasks, child tasks, or acceptance criteria, but maturity is uncertain: route to `game-task-triage` first.
   - Existing implementable task, but system boundaries, module responsibilities, MonoBehaviour/plain C# split, or test seams are unclear: route to `game-unity-system-design`.
   - Existing implementable task, but HUD, menus, results screens, settings, onboarding, information hierarchy, interaction flow, or long-distance readability is unclear: route to `game-ui-hud-flow-design`.
   - Clear implementable task and acceptance criteria already exist: may route to `game-unity-implementation`.
   - Existing playable build, recording, or player feedback where the issue is confusion, awkward controls, weak feedback, unclear HUD, weak pacing engagement, or "not satisfying enough": route to `game-playtest-ux-evaluation`.
   - Existing questions about values, difficulty, fairness, pacing, economy, or rewards: route to `gameplay-balance-tuning`.
   - Existing bug symptoms, logs, test failures, or device issues: route to `game-qa-debug` or `game-performance-build`.
   - Only a vague idea: if the user wants breadth, novelty, or mechanic exploration, route to `game-brainstorm`; if the user wants scoping or documentation, route to `game-brief`; if project setup is unknown and the user wants saved project conventions, route to `game-setup-game-dev-skills` first.
2. Judge the request type:
   - New game idea
   - Creative brainstorm, mechanic exploration, meme-to-game validation, or body-input concept search
   - Game research, competitors, platform rules, or technical direction
   - Existing-project feature iteration
   - Mid/late-stage stabilization or small change
   - GDD or design document
   - Levels, tasks, enemy combinations, content pacing, or content reuse
   - Progression, rewards, sinks, unlocks, economy, or monetization boundaries
   - Onboarding, first-time experience, tutorial, or failure recovery
   - Feature slicing
   - Task triage
   - Unity system design
   - UI/HUD, menus, results, settings, onboarding, or long-distance readability design
   - Gameplay values or difficulty tuning
   - Playtest experience, UX, HUD readability, or player feedback evaluation
   - Accessibility, localization, subtitles, color blindness, input adaptation, or multilingual text expansion check
   - Unity feature implementation
   - Art, audio, or asset pipeline
   - Bug, build, or performance issue
   - Release or live operations
   - Cross-session handoff
3. Check whether the project has completed skill-library setup. Prefer checking `game-design/<game-slug>/agents/`, `AGENTS.md`, `CLAUDE.md`, and `game-design/<game-slug>/GAME_CONTEXT.md`. If project conventions are unknown, the recommended path must include `game-setup-game-dev-skills` first.
4. If multiple minigame directories exist under `game-design/`, lock onto the `<game-slug>` for the current request before recommending or executing downstream skills.
5. Recommend one short skill path instead of listing every available skill. Default to at most 1-3 skills unless the request itself is a full version workflow.
6. If the requirement is still unclear, first judge whether it is a vague new-game idea or an existing-project incremental change. For a new game, route to `game-brainstorm` when the user asks for ideas, novelty, directions, hooks, brainstorming, divergence, or more interesting gameplay; route to `game-brief` when the user asks to scope, summarize, document, or save a project brief. For an existing project, recommend `game-feature-iteration` to fill in a feature delta brief. Only prioritize `game-design-grilling` when the user explicitly asks to "discuss first", "ask me probing questions", "grill", "probe with documents", or "clarify while recording context"; if confirmed terminology, system boundaries, or long-term decisions need recording, then add `game-domain-modeling`.
7. If the user describes an exception, error, performance drop, or build failure, recommend `game-qa-debug`; if the focus is FPS, GC, memory, package size, loading, or platform build, recommend `game-performance-build`, then route to `game-qa-debug` if needed.
8. If the user describes unfair difficulty, unfun values, pacing too fast/slow, disputed hit detection, or reward/economy imbalance, recommend `gameplay-balance-tuning`; if the symptoms look like a bug or performance problem, route first to `game-qa-debug` or `game-performance-build`.
9. If the user wants to design levels, tasks, enemy/obstacle combinations, content packs, pacing curves, difficulty steps, or content reuse, recommend `game-level-content-design`; route to `gameplay-balance-tuning` later when parameters need fine tuning.
10. If the user wants to design progression, rewards, sinks, unlocks, resource sources/sinks, retention motivation, monetization, or ad boundaries, recommend `game-economy-progression-design`; if platform policy or SDKs are involved, route first to `game-research`.
11. If the user wants to design onboarding, first-time experience, first-run learning path, teaching triggers, error prompts, or failure recovery, recommend `game-onboarding-tutorial-design`; if prompt layout is involved, route later to `game-ui-hud-flow-design`.
12. If the user wants to design HUD, menus, pause, settings, results, information hierarchy, button flow, copy placeholders, or long-distance readability, recommend `game-ui-hud-flow-design`; if final visual assets, icons, animation, SFX, or import rules are needed, route later to `game-art-audio-pipeline`.
13. If the user describes playtest feedback, player confusion, awkward controls, unclear feedback, poor HUD/results/onboarding readability, or "not satisfying enough", recommend `game-playtest-ux-evaluation`; route suspected bugs to `game-qa-debug`, suspected value/fairness issues to `gameplay-balance-tuning`, suspected missing UI/HUD spec to `game-ui-hud-flow-design`, and suspected content/tutorial/economy structure issues to the relevant design skill.
14. If the user wants to check readability, color blindness, subtitles, input adaptation, vibration/flashing, multilingual text expansion, font character sets, or pre-release language risk, recommend `game-accessibility-localization-check`; route to `game-research` or `game-release-liveops` when platform policy conclusions are needed.
15. If the user asks for implementation, first confirm whether a task, GDD, or acceptance criteria already exist. If missing, fill in the nearest upstream flow instead of forcing a return to a full GDD. If task maturity is unclear, recommend `game-task-triage` first; if the task is mature but system boundaries or test seams are unclear, recommend `game-unity-system-design` first; if the task involves UI/HUD but information hierarchy or flow is unclear, recommend `game-ui-hud-flow-design` first.
16. If gameplay is uncertain, do not recommend an independent prototype skill. Recommend `game-brainstorm` when the core hook or player verbs are weak, `game-brief` when a direction is selected and needs scope, or `game-feature-slicer` when a confirmed design needs the smallest playable slice.

## Progression Mode Selection

When a GDD already exists and the user says "continue", "next step", "leave the rest to AI", "keep going", or asks to move from slicing into implementation, first give the user one progression mode choice:

| Mode | When To Use | Agent Behavior |
|---|---|---|
| Step-by-step confirmation | The user wants to review design, tasks, system design, and implementation boundaries stage by stage. | Wait for confirmation after each stage output before entering the next skill. |
| Automated continuous progression | The user wants AI to continue completing downstream work after the GDD. | Proceed continuously through `game-level-content-design(when levels/content are involved) -> game-economy-progression-design(when progression/rewards are involved) -> game-onboarding-tutorial-design(when first-time experience is involved) -> game-feature-slicer -> game-task-triage -> game-unity-system-design -> game-ui-hud-flow-design(when UI/HUD is involved) -> game-unity-implementation`; stop only when key design decisions, device/permissions, external assets, or broad irreversible changes are missing. |

Selection rules:

- Recommend "automated continuous progression" by default, but make sure the user knows they can switch to "step-by-step confirmation".
- If the user has already explicitly said "automate it", "let AI finish it", "do not ask me at every step", or "keep progressing", do not ask again.
- Automated continuous progression must still follow task contracts, system design, verification, and task write-back rules. It does not skip quality gates.
- In automated mode, `game-unity-system-design` should design the whole feature / epic once, not split design separately for every child task.

## Routing Reference

| User Intent | Recommended Path |
|---|---|
| Needs gameplay, competitor, market, platform rule, SDK, or technical direction research | `game-research -> game-brief` or `game-research -> game-task-triage` |
| Needs clarification first and wants terminology or long-term decisions captured during discussion | `game-design-grilling -> game-domain-modeling` |
| Needs creative divergence, better hooks, playable concept options, meme-to-game judgment, or body-input concept exploration | `game-brainstorm -> game-brief` |
| Vague new idea, user asks for a brief or names `game-brief` | `game-brief` |
| Vague new idea, setup unknown or uninitialized, and user wants saved project conventions | `game-setup-game-dev-skills -> game-brainstorm -> game-brief -> game-gdd -> game-feature-slicer` |
| Vague new idea, setup initialized | `game-brainstorm -> game-brief -> game-gdd -> game-feature-slicer` |
| Existing design, needs GDD | `game-gdd` |
| Existing game, needs to add or change a feature | `game-feature-iteration -> game-task-triage` |
| Mid/late-development small change, stabilization, or polish | `game-feature-iteration -> game-task-triage -> game-unity-implementation` |
| Live or near-release hotfix | `game-feature-iteration -> game-qa-debug -> game-unity-implementation -> game-release-liveops` |
| Existing game, only value, asset, level, or content iteration | `game-feature-iteration -> gameplay-balance-tuning` or `game-feature-iteration -> game-art-audio-pipeline` |
| Needs level, task, enemy/obstacle combination, content pacing, or content reuse design | `game-level-content-design -> game-feature-slicer` |
| Needs progression, rewards, sinks, unlocks, resource flow, monetization, or ad boundary design | `game-economy-progression-design -> gameplay-balance-tuning` |
| Needs onboarding, first-time experience, tutorial, or failure recovery design | `game-onboarding-tutorial-design -> game-ui-hud-flow-design` |
| Existing GDD, needs feature slicing/subdivision | `game-feature-slicer` |
| Existing task directory, but implementability is uncertain | `game-task-triage` |
| Existing task, but system boundaries, module interfaces, MonoBehaviour responsibilities, or test seams are unclear | `game-unity-system-design` |
| Needs HUD, menu, results, settings, onboarding, information hierarchy, button flow, or long-distance readability design | `game-ui-hud-flow-design` |
| Difficulty unfair, values unfun, pacing too fast/slow, hit detection disputed | `gameplay-balance-tuning` |
| Playable build feels bad, players are confused, controls are awkward, feedback is weak, or HUD/results are unclear | `game-playtest-ux-evaluation -> game-ui-hud-flow-design` |
| Needs readability, color blindness, subtitles, input adaptation, multilingual text expansion, or localization risk check | `game-accessibility-localization-check` |
| Existing task or acceptance criteria, needs Unity feature implementation | `game-unity-implementation` |
| Needs UI visual assets, icons, animation, audio, naming, or import rules | `game-art-audio-pipeline` |
| Bug, test failure, runtime exception, or hard-to-reproduce issue | `game-qa-debug` |
| FPS, GC, memory, package size, loading, build failure, or platform readiness | `game-performance-build -> game-qa-debug` |
| Unity changes need review | `game-unity-code-review` |
| Unity project structure, coupling, or long-term maintenance risk needs audit | `game-unity-architecture-audit` |
| Preparing beta, store submission, hotfix, or liveops activity | `game-release-liveops` |
| Need to continue in another session | `game-handoff-game-context` |

## User Memory Rule

The user only needs to remember:

```text
game-captain
```

If the user mainly works on incremental features for existing projects, they may also remember:

```text
game-feature-iteration
```

Recommended user-facing wording:

```text
I am not sure which game-development skill to use; hand this to game-captain to choose and execute the next step.
```

The agent must:

- First judge the current request type and existing context.
- Give only one main path. Do not list all 18 skills for the user.
- Route existing-project incremental changes to `game-feature-iteration` first to establish impact area and regression scope, instead of defaulting back to a full GDD.
- If a downstream skill can be entered directly, explain why and continue. Do not make the user manually choose again.
- After a GDD, ask the user to choose "step-by-step confirmation / automated continuous progression" before downstream work. Once automated mode is chosen, downstream skills deliver continuously by feature / epic.
- If the user names a downstream skill but it clearly does not fit, point out the risk first, then give the correct path judged by `game-captain`.
- If a downstream skill is not installed or unavailable, clearly state the gap. Do not pretend it has been executed.

## Completion Criteria

- The user knows which skill or flow to use next.
- Missing prerequisite context is clearly identified.
- The recommended path explains why.
- The user can continue the task without remembering skill names other than `game-captain`.

## Output Format

```text
Recommended flow:
<skill> -> <skill> -> <skill>

Reason:
<short explanation>

Needed before starting:
<missing context, or "none">
```

## Quality Checks

- Do not complete concrete downstream work in place of downstream skills.
- Do not skip setup when project conventions are unknown.
- Keep the recommended path short and immediately executable.
- Recommend only skills that currently exist in this skill library.
- When time-sensitive external information is involved, recommend `game-research` first. Do not make platform, SDK, market, or competitor decisions from old knowledge.
- Do not slice level/task/content combinations directly into implementation tasks. First use `game-level-content-design` to confirm content goals, pacing, combinations, and reuse.
- Do not mix progression, rewards, resource flow, and monetization boundaries into ordinary value tuning. First use `game-economy-progression-design` to clarify the economy structure.
- Onboarding is not a subordinate part of UI layout. First use `game-onboarding-tutorial-design` to clarify learning goals, triggers, and failure recovery, then route to UI or implementation.
- Do not hand UI/HUD specs, information hierarchy, and flow design directly to the asset pipeline or implementation. First use `game-ui-hud-flow-design` to clarify what the player sees, when they see it, how they operate, and how it is accepted.
- Do not treat playtest experience issues directly as bugs or value problems. First use `game-playtest-ux-evaluation` to classify evidence, then route downstream.
- Do not wait until the final release moment to check accessibility and localization risks. Recommend `game-accessibility-localization-check` when UI, tutorials, subtitles, multiple inputs, or multiple languages are involved.
- Do not pull every request back into a full GDD. When tasks already exist, go directly to implementation or debugging.
- Do not misclassify a new feature in an existing project as a zero-to-one new game. When context is missing, fill in a feature delta brief and regression scope.

## Prohibited

- Do not write code.
- Do not directly create a full GDD.
- Do not edit project files unless the user explicitly asks.
- Do not recommend `gameplay-prototype`, `game-feedback-loop`, `game-vertical-slice`, `unity-architecture-review`, or `skill-authoring-rules` as independent skills.
