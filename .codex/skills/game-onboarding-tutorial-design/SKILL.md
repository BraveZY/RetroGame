---
name: game-onboarding-tutorial-design
description: Design onboarding, first-time experience, first-session learning paths, teaching triggers, error prompts, failure recovery, and early retention so players can understand goals, learn controls, experience first success/failure, and want to continue without reading a manual.
---

# Game Onboarding and First-Time Experience Design

## Core Principles

This skill designs how players learn the game for the first time. It does not replace UI/HUD layout, level content design, playtest evaluation, or implementation; it outputs FTUE and tutorial specs that make teaching serve real play instead of interrupting it.

## When To Use

- You need to design the first 30 seconds, first match, first three matches, first failure, first victory, and first reward.
- You need to break complex rules into progressive teaching, contextual hints, mission objectives, and safe practice.
- You need to design wrong-action hints, failure recovery, retry, skip, replay, and returning-player prompts.
- An existing project has churn, confusion, unclear goals, tutorials that are too long, or insufficient teaching.
- A new feature, event, mode, or control scheme needs first-time guidance.

## When Not To Use

- HUD, menu, results, or button layout is itself unclear; use `game-ui-hud-flow-design`.
- Levels, missions, enemy combinations, or content pacing are the main concern; use `game-level-content-design`.
- A playable version exists and you need to verify whether players learned; use `game-playtest-ux-evaluation`.
- You are only fixing broken clicks, stuck tutorials, or state bugs; use `game-qa-debug`.
- You need accessibility, text expansion, multilingual, or input adaptation checks; use `game-accessibility-localization-check`.

## Inputs

- GDD, core loop, target player, platform, input method, UI/HUD spec, and first-match content.
- Actions, rules, feedback, failure conditions, and rewards the player must learn.
- Existing tutorials, missions, popups, guide scripts, analytics, churn points, or playtest feedback.
- For an existing project, read the current first-time experience and veteran entry points that must not be broken.

## Workflow

1. Define learning goals: what players must learn, what can be learned later, and what should not be taught in the first match.
2. Design the first-time experience promise: the goal, action, feedback, and emotion players see within 30 seconds.
3. Establish the teaching order: introduce one key concept at a time, action before rules, feedback before systems.
4. Choose teaching forms: no explicit tutorial, contextual hints, mission objectives, short popups, demonstrations, ghost hands, practice levels, or failure feedback.
5. Design triggers: first entry, first approach, first mistake, repeated failure, first success, long inactivity, or returning-player entry.
6. Design failure recovery: failure reason, immediate retry, reduced pressure, retained progress, hints, and avoiding punitive shaming.
7. Design skip and replay paths: veteran players, returning players, and players who skipped by mistake all need paths.
8. Plan text and localization: short, replaceable copy with little slang; key teaching should not rely only on text.
9. Plan analytics and validation: stuck points, skip rate, failure count, first-match completion rate, time to first success, and hint trigger rate.
10. Output or update `references/ONBOARDING-TUTORIAL-SPEC.md`, then hand off UI, implementation, and playtest validation downstream.

## Spec Template

When a persistent document is needed, use `references/ONBOARDING-TUTORIAL-SPEC.md` from this skill directory.

Default project path:

```text
game-design/<game-slug>/onboarding/<flow-slug>/references/ONBOARDING-TUTORIAL-SPEC.md
```

## Output Format

```text
First-time experience goal:
Learning goals:
Teaching order:
Triggers and hints:
Failure recovery:
Skip/replay:
Analytics and acceptance:
Handoff:
```

## Completion Criteria

- The first match teaches only what must be learned, with a delayed-teaching plan for later content.
- Every hint has trigger conditions, exit conditions, and success criteria.
- The first failure has a recovery path and does not make players think the game is not for them.
- Teaching does not rely only on text; key actions have visual, interactive, or feedback support.
- Playtest validation metrics exist and can be handed to `game-playtest-ux-evaluation`.

## Quality Checks

- Do not turn the tutorial into a manual; players must learn through action.
- Do not explain systems before players need them.
- Do not block veteran or returning players with long unskippable flows.
- Do not blame players for not understanding; check goals, feedback, input, and hints first.
- Do not claim guide scripts or UI have been implemented; output only specs and handoff boundaries.
