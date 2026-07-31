---
name: game-ui-hud-flow-design
description: Design game HUD, menus, result screens, settings, onboarding, information hierarchy, interaction flows, and long-distance readability when players need to clearly see goals, state, feedback, rewards, and the next action, or when UI/HUD specs, flow diagrams, placeholder copy, resource needs, and Unity integration boundaries are needed before implementation.
---

# Game UI/HUD Flow Design

## Core Principles

This skill designs how players understand the game on screen, take actions, and receive feedback. It outputs UI/HUD specs and does not replace art asset production, Unity implementation, or post-playtest UX evaluation.

## When To Use

- You need to design HUD elements: health, score, countdown, skill cooldowns, resources, mission objectives, hint arrows, states, combos, or danger warnings.
- You need to design menu flows: main menu, pause, level select, character select, inventory, upgrades, store, settings, or confirmation dialogs.
- You need to design results screens: win/loss, rating, rewards, unlocks, failure reasons, restart, next level, or return paths.
- You need to design onboarding: first-time hints, contextual hints, wrong-action hints, teaching pacing, and skip/replay.
- You need to support long-distance readability: TV, projection, large screens, expos, party games, motion games, or same-screen multiplayer.
- UI/HUD affects gameplay understanding, asset production, Unity wiring, input methods, or acceptance criteria.

## When Not To Use

- You only need final art, icons, animation, audio, or import rules; use `game-art-audio-pipeline`.
- A playable version exists and you need to judge whether players actually understand it, can use it smoothly, or find it clear; use `game-playtest-ux-evaluation`.
- UI requirements do not yet have player goals, gameplay states, or feature boundaries; use `game-brief`, `game-gdd`, or `game-feature-slicer` first.
- UI code, Prefabs, scenes, or Unity implementation details are already clear; use `game-unity-implementation`.
- There are UI bugs, occlusion, broken clicks, resolution adaptation failures, or runtime exceptions; use `game-qa-debug`.

## Inputs

- GDD, feature spec, tasks, acceptance criteria, or feature-increment brief for an existing project.
- Platform, input method, screen size, target device, camera/view, multiplayer/single-player, camera distance, and accessibility requirements.
- Existing UI style, fonts, icons, asset list, Prefabs, scenes, or menu structure.
- For an existing project, read current UI behavior and flows that must not change.

## Workflow

1. Clarify the player's current context: in combat, preparing, paused, in results, upgrading, after failure, first entry, or returning entry.
2. Clarify the 1-3 most important things players need to know in that context: objective, danger, resources, state, available actions, rewards, or next step.
3. Establish the information hierarchy:
   - Persistent information: what players must monitor continuously.
   - Conditional information: appears only on state changes, danger, interactability, or threshold values.
   - Feedback information: action success/failure, damage taken, score, rewards, unlocks, or errors.
   - Low-priority information: can move into menus, detail pages, or pause screens.
4. Design the main flow: entry, default focus, back, confirm, cancel, restart, continue, exit, next level, and failure recovery.
5. Design input methods: mouse, touch, keyboard, controller, motion input, same-screen multiplayer; mark default buttons and misinput prevention.
6. Design HUD layout: position, safe area, occlusion risk, camera/character/enemy/VFX conflicts, and multiplayer screen regions.
7. Design readability: type scale, contrast, icon semantics, color dependence, animation duration, long-distance viewing, and small-screen adaptation.
8. Design guidance: teach only actions needed now; prefer contextual hints, avoid long explanations, and allow skipping, replaying, or learning through failure feedback.
9. Design results: explain outcome, rewards, and next step; on failure, provide improvement clues instead of only punishment.
10. List asset needs: icons, fonts, button states, panels, animations, audio, localization text keys, placeholder copy, and missing assets.
11. List Unity integration boundaries: Canvas/Prefab, state sources, event entry points, data fields, animation triggers, audio triggers, and acceptance method.
12. Output or update `references/UI-HUD-FLOW-SPEC.md`; for a small change, output equivalent fields in the conversation and explain the recommended landing path.

## Spec Template

When a persistent document is needed, use `references/UI-HUD-FLOW-SPEC.md` from this skill directory as the template.

Default project path:

```text
game-design/<game-slug>/ui/<feature-slug>/references/UI-HUD-FLOW-SPEC.md
```

## Output Format

```text
Design scope:
Player context:
Platform/input/view constraints:

Information hierarchy:
| Information | Type | When It Appears | Priority | Presentation Suggestion |

Flow:
| Context | Player Goal | Available Actions | Default Focus | Back/Failure Path |

HUD / Menu / Results / Settings / Guidance specs:

Long-distance readability:

Asset needs:
| Asset | Purpose | Status | Owner |

Unity integration boundaries:

Acceptance criteria:

Out of scope:

Follow-up handoff:
```

## Completion Criteria

- The player's goal, state, feedback, and next action are clear in every key context.
- The information hierarchy is explicit, and not everything is made into persistent HUD.
- Menu, results, settings, and guidance flows include back, cancel, failure recovery, and default focus.
- Long-distance, small-screen, same-screen multiplayer, or platform constraints are handled explicitly or marked as risks.
- Asset needs can be handed to `game-art-audio-pipeline`, and implementation boundaries can be handed to `game-unity-implementation`.
- Existing projects identify which UI behaviors must not change and which need regression testing.

## Quality Checks

- Do not use vague conclusions like "make it prettier" or "make it clearer" instead of information hierarchy and flow.
- Do not write tutorials as manuals; every guidance item must map to the player's current action.
- Do not let HUD cover core play space, enemies, characters, projectiles, interactables, or important feedback.
- Do not design only from static screenshots; account for state changes, transitions, failure, back paths, and repeated play.
- Do not use color as the only information channel; key states need shape, icon, text, animation, or audio support.
- Do not mix asset production and Unity implementation into the same artifact; write only specs and handoff boundaries.

## Prohibited

- Do not claim art, Prefabs, scenes, or code implementation are already complete.
- Do not force HUD design without gameplay state sources.
- Do not expand the current feature scope just to complete a full UI system.
