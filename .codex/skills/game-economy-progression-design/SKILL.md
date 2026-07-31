---
name: game-economy-progression-design
description: Design progression, rewards, sinks, unlocks, resource sources/sinks, progress curves, retention motivation, and monetization/ad boundaries when the project needs to answer why players keep playing, how rewards are earned, how content unlocks, whether the economy is healthy, or whether monetization harms the experience.
---

# Game Economy and Progression Design

## Core Principles

This skill designs why players keep playing and how resources move. It does not replace combat balance tuning, level content design, store asset production, or platform compliance checks; it outputs traceable progression, reward, sink, and unlock specifications.

## When To Use

- You need to design character progression, ability unlocks, level unlocks, equipment/items, mission rewards, currencies, experience, stamina, or materials.
- You need to plan short-term, mid-term, and long-term goals and player return motivation.
- You need to judge whether rewards are meaningful, sinks are reasonable, resources are inflating, or progress is blocked.
- You need to define ads, IAP, subscriptions, Battle Passes, event rewards, or monetization boundaries.
- An existing project needs a new economy system, event rewards, store, unlock path, progression path, or anti-inflation adjustment.

## When Not To Use

- Per-match hit detection, speed, damage, spawn frequency, or difficulty fairness; use `gameplay-balance-tuning`.
- Levels, missions, enemy combinations, or content pacing; use `game-level-content-design`.
- UI/HUD, store pages, reward presentation, or results layout; use `game-ui-hud-flow-design`.
- Platform stores, SDKs, ad networks, or current policy judgment; use `game-research` first.
- Bugs, duplicate rewards, save corruption, or payment failures; use `game-qa-debug`.

## Inputs

- Game genre, target platform, business model, player stage, session length, and retention goals.
- GDD, tasks, content design, existing economy tables, reward tables, store tables, event tables, and save structure.
- Whether the game targets children, includes ads/payment, has offline earnings, social trading, or random draws.
- For an existing project, read current resource sources, sinks, existing player assets, and progress that must not be broken.

## Workflow

1. Clarify the economy goal: motivation, pacing, retention, expression, collection, mastery, social play, monetization, or content consumption.
2. List resources: hard currency, soft currency, experience, materials, stamina, keys, tickets, shards, items, ad views, and paid entitlements.
3. Establish sources and sinks: where each resource comes from, where it is spent, whether it can be hoarded, purchased, or substituted.
4. Design the progression curve: first match, first day, first three days, first week, and mid/long term; mark key unlocks and reward peaks.
5. Design reward layers: immediate feedback, match results, mission rewards, achievements, chapter rewards, event rewards, and return rewards.
6. Design spending and recovery: upgrades, unlocks, refreshes, failure recovery, cosmetics, and convenience; avoid forcing meaningless spending.
7. Evaluate fairness and trust: paid/ad systems must not break the core promise; random rewards need probability, pity, or alternative paths.
8. Design economy protections: inflation, resource farming, paywalls, deadlocks, negative feedback, veteran overflow, and newcomer catch-up.
9. Hand off to content, UI, tuning, and implementation: reward presentation, data fields, table structure, analytics, saves, remote config, and acceptance.
10. Output or update `references/ECONOMY-PROGRESSION-SPEC.md`; complex economies must list assumptions and metrics that still need validation.

## Spec Template

When a persistent document is needed, use `references/ECONOMY-PROGRESSION-SPEC.md` from this skill directory.

Default project path:

```text
game-design/<game-slug>/economy/<system-slug>/references/ECONOMY-PROGRESSION-SPEC.md
```

## Output Format

```text
Economy goal:
Monetization boundary:
Player stage:

Resource flow:
| Resource | Source | Sink | Cap | Risk |

Progression/unlock curve:

Rewards and sinks:

Risks and protections:

Handoff:
```

## Completion Criteria

- Every resource has sources, sinks, cap/hoarding rules, and risks.
- The progression curve explains short-term, mid-term, and long-term player goals.
- Rewards explain why players care, rather than only giving larger numbers.
- Payment/ad boundaries do not break the core fairness promise, and compliance/research needs are marked.
- Implementation fields, UI presentation, analytics, and validation metrics are listed.

## Quality Checks

- Do not use "more satisfying" or "richer" as substitutes for resource flow and player goals.
- Do not design rewards as pure number inflation; distinguish power, content, expression, and convenience.
- Do not add payment or ads by default; first explain the experience boundary and target audience.
- Do not treat economy adjustments as balance tuning by default; combat/level fairness still routes to `gameplay-balance-tuning`.
- Do not claim the economy is healthy without data or explicitly marked assumptions.
