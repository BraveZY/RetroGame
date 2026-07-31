---
name: game-playtest-ux-evaluation
description: Evaluate a game prototype, implemented feature, or version playtest experience when you need to judge whether players understand goals, controls feel good, feedback is clear, HUD/onboarding/results are readable, pacing is engaging, and experience issues can become improvement tasks.
---

# Playtest UX Evaluation

## Core Principles

This skill evaluates player experience. It does not replace QA, balance tuning, or code review. It turns experience issues like "unclear, awkward, unsatisfying, unfair, no feedback" into evidence-backed, prioritized improvement items that can route into downstream skills.

## When To Use

- A gameplay prototype or vertical slice is playable and you need to judge whether the experience works.
- After feature implementation, you need to observe whether players understand goals, controls, feedback, failure reasons, and result meaning.
- HUD, onboarding, menus, results, large-screen readability, motion input, or multiplayer cooperation experience needs evaluation.
- Feedback from internal tests, exhibitions, family/child/casual playtests is scattered and needs to become improvement tasks.
- The user says "it feels weird to play", "not satisfying enough", "hard to see", "I do not know what to do", or "players misunderstand the rules".

## When Not To Use

- The current issue is a specific bug, exception, crash, build failure, or performance problem; use `game-qa-debug` or `game-performance-build`.
- The main issue is numbers, difficulty curves, hit fairness, or economy rewards; use `gameplay-balance-tuning`.
- There is no playable version or observable flow yet; use `game-brief`, `game-gdd`, `game-feature-slicer`, or `game-unity-implementation` first.
- You only need to review a code diff; use `game-unity-code-review`.

## Inputs

- GDD, feature-increment brief, tasks, acceptance criteria, or current version goals.
- Playable version, Unity scene, prototype, recording, screenshots, input logs, player feedback, or observation notes.
- Target player, playtest environment, device, viewing distance, input method, and questions to validate in this test.

## Workflow

1. Read the design source and task acceptance criteria to confirm what this experience intended players to understand and complete.
2. Clarify playtest goals, at most 3-5 questions, such as goal understanding, first success, feedback clarity, failure recovery, and desire to play again.
3. Design or organize playtest tasks: start a match, perform the core action, encounter failure, view results, restart, or enter the next round.
4. Collect evidence: recordings, screenshots, direct player quotes, observation notes, action counts, time spent, stuck points, misinputs, and test environment.
5. Classify issues by experience dimension:
   - Understanding: whether players know the objective, rules, and next step.
   - Controls: whether input, feel, forgiveness, misinput prevention, and motion recognition are smooth.
   - Feedback: whether success, failure, damage, rewards, and danger warnings are clear.
   - Pacing: whether waiting, pressure, repetition, climax, results, and restart flow are smooth.
   - UI / readability: whether HUD, hints, fonts, colors, distance, and multiplayer distinction are clear.
   - Emotion: satisfaction, tension, fairness, frustration, and desire to replay.
6. Mark severity and evidence; do not treat personal preference as a universal issue.
7. Output an experience evaluation report using the `references/PLAYTEST-REPORT.md` structure.
8. Route improvements to downstream skills: unclear design to `game-design-grilling`, task reslicing to `game-feature-slicer`, implementation/UI/feedback to `game-unity-system-design` or `game-unity-implementation`, tuning fairness to `gameplay-balance-tuning`, and suspected bugs to `game-qa-debug`.

## Severity

| Level | Meaning |
|---|---|
| P0 | Players cannot continue, fully misunderstand the core goal, or the flow is interrupted. |
| P1 | Players can continue, but misunderstand core rules, failure reasons, or key feedback. |
| P2 | The experience is awkward, feedback is weak, pacing drags, or readability is poor, but completion is not blocked. |
| P3 | Preference, copy, visual details, or polish that can be deferred. |

## Output Format

```text
Playtest goals:
Test subjects / environment:
Version / scene / materials:

Playtest tasks:
| Task | Expected Behavior | Observation Metric |

Key observations:
| Observation | Evidence | Impact |

Experience issues:
| Issue | Type | Severity | Evidence | Suggestion |

Player understanding:
Control smoothness:
Feedback clarity:
Pacing and difficulty:
UI / HUD readability:
Emotion and satisfaction:

Recommended improvements:
| Priority | Improvement | Recommended Skill |

Not recommended to change now:
Still needs revalidation:
```

## Completion Criteria

- The report states playtest goals, test subjects, environment, and materials.
- Every key issue has observation evidence, not just "feels bad."
- Issues are classified and assigned severity.
- Improvements can enter downstream skills instead of remaining verbal suggestions.
- It is clear which feedback is not recommended to change now and which needs a second playtest validation.

## Quality Checks

- Do not disguise bugs, performance issues, or build issues as UX problems; suspected technical issues must route to the appropriate skill.
- Do not treat one player's preference as a design conclusion; state the evidence source and applicable scope.
- Do not output empty advice like "optimize UI / enhance feedback"; identify the exact step where players misunderstood or got stuck.
- Do not require all experience issues to be fixed at once; prioritize blockers to understanding, core feedback, and frequent misinputs.
- Motion, large-screen, multiplayer, child/family scenarios must consider viewing distance, standing position, spectator readability, and input-loss prompts.

## Prohibited

- Do not directly modify code, scenes, Prefabs, or assets.
- Do not use the playtest report as a replacement for the GDD, task slicing, or QA reproduction report.
- Do not use "I think" as a substitute for observation evidence.
