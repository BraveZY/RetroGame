---
name: game-level-content-design
description: Design levels, missions, enemy/obstacle combinations, pacing curves, difficulty steps, content reuse, and level acceptance criteria when a GDD or version goal needs to become playable level content, mission structures, encounter configs, content packs, or reusable level modules.
---

# Game Level and Content Design

## Core Principles

This skill designs the structure of the content players actually play. It does not replace balance tuning, UI/HUD design, art production, or Unity implementation; it outputs level, mission, and encounter specifications that feature slicing, asset pipelines, and implementation can consume.

## When To Use

- You need to design levels, mission chains, rooms, waves, enemy mixes, obstacle mixes, routes, or challenge structures.
- You need to turn the GDD gameplay loop into multiple buildable, reusable, and verifiable content units.
- You need to plan difficulty steps, pacing curves, tutorial levels, climax levels, boss/elite levels, or variant content.
- You need to organize content reuse: modules, templates, parameter variants, enemy pools, objective pools, reward pools, and level tags.
- An existing project needs new levels, event content, daily challenges, mission packs, or content expansions.

## When Not To Use

- The core gameplay, player goals, or failure conditions are still unclear; use `game-brief`, `game-gdd`, or `game-design-grilling` first.
- You are only tuning stat strength, drop rates, spawn frequency, or fairness; use `gameplay-balance-tuning`.
- The main concern is progression, unlocks, rewards, or resource sink/source loops; use `game-economy-progression-design`.
- The main concern is HUD, menus, results screens, settings, or tutorial UI; use `game-ui-hud-flow-design`.
- A playable version exists and experience issues need evaluation; use `game-playtest-ux-evaluation`.

## Inputs

- GDD, feature spec, version goals, player profile, core loop, and out-of-scope items.
- Target platform, match length, session length, input method, camera/view, device limits, and performance limits.
- Existing enemies, items, mechanics, assets, levels, missions, Prefabs, scenes, and content naming rules.
- For an existing project, read the existing level behavior, content reuse approach, and regression scope.

## Workflow

1. Clarify the content goal: teaching, practice, mastery, variation, pressure, reward, narrative, social play, retention, or event content.
2. Define each content unit's player goal, primary actions, failure conditions, rewards, and exit paths.
3. Establish content atoms: mission objectives, enemies/obstacles, terrain/routes, interactables, rewards, constraints, time pressure, and special rules.
4. Design the pacing curve: introduction, development, variation, pressure peaks, recovery, and resolution; avoid one flat intensity from start to finish.
5. Design difficulty steps: introduce only 1-2 new cognitive loads at a time; distinguish mechanical, strategic, informational, and execution difficulty.
6. Plan content reuse: what is a template, what is a parameter variant, and what must be handmade; mark reuse risks and fatigue points.
7. Design the content matrix: levels/missions, objectives, combinations, pacing, difficulty, rewards, asset needs, and validation method.
8. Mark dependencies: new mechanics, UI, assets, audio, tuning, AI, scenes, Prefabs, and performance budget.
9. Mark out-of-scope items: enemies not being added, unsupported platforms, content scale not included, and procedural generation not promised.
10. Output or update `references/LEVEL-CONTENT-SPEC.md`; when needed, hand the content pack to `game-feature-slicer` for task slicing.

## Spec Template

When a persistent document is needed, use `references/LEVEL-CONTENT-SPEC.md` from this skill directory.

Default project path:

```text
game-design/<game-slug>/content/<content-pack-slug>/references/LEVEL-CONTENT-SPEC.md
```

## Output Format

```text
Content goal:
Applicable version / player stage:
Content constraints:

Content pacing:
| Phase | Player Goal | Pressure | New Variable | Release/Reward |

Content matrix:
| Content Unit | Objective | Combination | Difficulty | Reuse Method | Acceptance |

Dependencies and handoff:

Out of scope:
```

## Completion Criteria

- Every level, mission, or encounter has a player goal, challenge combination, pacing position, reward, or exit path.
- The difficulty curve explains why each piece of content appears where it does, instead of only listing level names.
- The content reuse strategy is clear, reduces production cost, and does not create repetitive fatigue.
- Asset, UI, tuning, implementation, and validation dependencies are marked.
- Existing projects identify existing content behavior that must not change and the required regression scope.

## Quality Checks

- Do not treat "more enemies / higher numbers" as the only source of difficulty.
- Do not introduce too many new mechanics at once; teaching, variation, and pressure should be layered.
- Do not write the content table as a wishlist; every item must have production boundaries and an acceptance method.
- Do not treat procedural generation as the default; recommend it only when reuse value and control methods are clear.
- Do not claim that art, level scenes, or code are already complete.
