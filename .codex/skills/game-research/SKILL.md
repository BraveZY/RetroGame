---
name: game-research
description: Use for gameplay, competitor, platform-rule, technical-solution, SDK/plugin, market, and asset-reference research when design, slicing, implementation, resource, or release decisions need external evidence, current information, or traceable sources.
---

# Game Research and Evidence

## Core Principle

This skill turns uncertain questions into traceable evidence. It does not replace design judgment. Research findings must support later game design, task slicing, implementation, asset pipeline, or release decisions.

## When To Use

Use this skill when the request involves the following topics and project-internal documents are not enough to make a decision:

- Gameplay, genre, competitors, platforms, player expectations, or monetization references.
- Platform rules, store assets, SDKs, plugins, engine versions, device capabilities, or third-party technical approaches.
- External references for art, UI, audio, interaction, camera, levels, economy, or live operations.
- Evidence that must be gathered before `game-brief`, `game-gdd`, `game-feature-slicer`, `game-unity-system-design`, or `game-release-liveops`.

## When Not To Use

- The project already has clear decisions and acceptance criteria; use the relevant execution skill.
- The user only needs question-by-question clarification of their own idea; use `game-design-grilling`.
- The current issue is a bug, performance problem, or build failure; use `game-qa-debug` or `game-performance-build`.
- You are only organizing existing project material and do not need external evidence; use the relevant design or handoff skill.

## Inputs

- Research question, target decision, and usage context.
- Current game type, platform, engine, target users, production constraints, and existing design documents.
- The decision to support: continue design, slice tasks, choose technology, connect assets, tune values, prepare release, or avoid risk.

## Workflow

1. First clarify the research purpose: which game decision this research supports. Do not search broadly without a decision target.
2. Read the project-internal context and confirm known facts, terminology, and constraints.
3. Break the research question into 2-5 evidence questions, such as player expectations, competitor practices, platform limits, technical feasibility, asset cost, or risk.
4. If the question may be time-sensitive, verify it online. For technology, platforms, SDKs, and store rules, prefer official docs, release notes, or primary sources.
5. For competitor or gameplay references, prioritize verifiable pages, videos, store pages, developer material, player feedback, or hands-on notes. Do not rely only on memory.
6. For each key finding, record source, date, and confidence. Separate facts, observations, and inferences.
7. Output a research report using the structure in `references/RESEARCH-REPORT.md`.
8. Convert findings into design implications, constraints, risks, and recommended next steps that downstream skills can use.

## Research Types

| Type | Key Evidence | Common Next Skill |
|---|---|---|
| Gameplay / Genre | Core loop, player expectations, common failure points, differentiation opportunities. | `game-brief`, `game-gdd` |
| Competitors / Market | Target users, store page messaging, review pain points, content volume, monetization signals. | `game-brief`, `game-release-liveops` |
| Feature / UX | Interaction flow, HUD, feedback, onboarding, failure recovery, readability. | `game-gdd`, `game-feature-slicer` |
| Technology / SDK | Official capabilities, version limits, platform compatibility, integration cost, licensing risk. | `game-task-triage`, `game-unity-system-design` |
| Content / Assets | Style references, asset specs, naming, import rules, generation limits. | `game-art-audio-pipeline` |
| Release / Platform | Store requirements, asset dimensions, privacy, SDKs, review risks, hotfix limits. | `game-performance-build`, `game-release-liveops` |

## Evidence Quality

| Level | Usefulness |
|---|---|
| L1 Official / Primary Source | Can support decisions about technology, platforms, rules, or SDKs. |
| L2 Hands-On / Project Evidence | Can support current project decisions, but the environment must be recorded. |
| L3 Consistent Multi-Source Observation | Can be used as design reference or risk signal. |
| L4 Single Secondary Source | Only a lead; cannot decide high-risk matters alone. |
| L5 Subjective Inference | Must be marked as an assumption and left for prototype, test, or user confirmation. |

## Output Format

```text
Research question:
Decision served:
Research scope:

Key findings:
| Finding | Evidence level | Source | Impact on design/implementation |

Reference cases:
| Case | Useful pattern | Do not copy |

Constraints and risks:
| Risk | Trigger condition | Recommended handling |

Recommended next step:
<skill> -> <skill>

Still needs verification:
```

## Completion Criteria

- The research question and served decision are clear.
- Key findings have sources and evidence levels.
- Facts, observations, and inferences are clearly separated.
- Findings can be used directly by downstream skills.
- Time-sensitive information records either the query date or the source date.

## Quality Checks

- Do not treat a search result list as a research report.
- Do not use outdated, secondary, or unsourced content to decide platform, SDK, legal, privacy, monetization, or release matters.
- Do not copy a competitor design just because a competitor used it; state the conditions under which it fits the current game.
- Do not write external material as if it were an approved project fact; mark anything that needs user confirmation or prototype validation.
- Do not output long material unrelated to the current decision.

## Prohibited

- Do not directly modify code, scenes, Prefabs, or assets.
- Do not create tasks unless the user explicitly asks to move research findings into the task workflow.
- Do not replace the deliverables of `game-brief`, `game-gdd`, `game-feature-slicer`, or `game-unity-system-design`.
