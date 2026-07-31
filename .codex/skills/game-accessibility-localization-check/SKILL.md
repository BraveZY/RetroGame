---
name: game-accessibility-localization-check
description: Check game accessibility, localization, and inclusion quality gates, including readability, color blindness, subtitles, input adaptation, vibration/flashing, audio alternatives, multilingual text expansion, font character sets, platform safe areas, and pre-release language risk when UI, tutorial, content, release, or playtest feedback needs to confirm more players can understand and operate the game.
---

# Game Accessibility and Localization Check

## Core Principles

This skill is a quality gate, not an art style or translation-writing skill. It checks whether players can see, hear, read, control, and recover, then turns issues into UI, content, implementation, asset, or release tasks.

## When To Use

- UI/HUD, tutorials, results, settings, subtitles, prompts, stores, rewards, or pre-release materials need an accessibility check.
- You need to check color blindness, contrast, font size, long-distance readability, flashing, vibration, audio alternatives, and input remapping.
- You need to check multilingual text expansion, font character sets, line breaks, truncation, variable placeholders, localization keys, and right-to-left language risks.
- Target platforms include mobile, console, large screens, exhibitions, children, international release, or multiple input methods.
- Playtest feedback mentions hard to see, hard to hear, hard to read, accidental input, input difficulty, or language overflow.

## When Not To Use

- HUD or menu flow needs to be designed; use `game-ui-hud-flow-design` first.
- Onboarding and first-time experience need to be written; use `game-onboarding-tutorial-design`.
- Fonts, icons, subtitle audio, or asset imports need to be produced; use `game-art-audio-pipeline`.
- Current platform compliance, ratings, privacy, or store policies need confirmation; use `game-research` or `game-release-liveops` first.
- Runtime UI bugs, input bugs, or build issues need fixing; use `game-qa-debug`.

## Inputs

- UI/HUD specs, tutorial specs, content copy, settings, platform targets, language list, and input methods.
- Screenshots, recordings, playable build, Prefab/Canvas structure, font assets, subtitle/audio assets, and localization tables.
- Playtest feedback, device information, screen size, viewing distance, and platform safe areas.
- For an existing project, read existing settings, language assets, and UI behavior that must not be broken.

## Workflow

1. Confirm check scope: UI, tutorial, HUD, menus, subtitles, input, language, release page, or full package.
2. Check readability: font size, contrast, background interference, dynamic scenes, long distance, small screens, safe areas, and text truncation.
3. Check color and feedback: key states must not rely only on color; add icons, shapes, text, animation, or audio.
4. Check audio and subtitles: important voice/audio cues need subtitles, visual prompts, or logs; subtitles must be readable, controllable, and able to distinguish speakers.
5. Check input adaptation: mouse/keyboard, touch, controller, motion input, one-handed use, remapping, hold/tap, accidental input, and accessible alternatives.
6. Check motion and stimulation: screen shake, flashing, bright lights, camera shake, vibration feedback, and options to disable them.
7. Check localization: text expansion, variable order, plural/gender handling, font character sets, line breaks, button width, RTL, and hardcoded text.
8. Check settings entry points: volume, subtitles, text size, vibration, camera shake, color assistance, input sensitivity, and language switching.
9. Assign severity: P0 blocking, P1 high risk, P2 polish, P3 suggestion; mark evidence and handoff skill.
10. Output or update `references/ACCESSIBILITY-LOCALIZATION-CHECK.md`.

## Check Template

When a persistent document is needed, use `references/ACCESSIBILITY-LOCALIZATION-CHECK.md` from this skill directory.

Default project path:

```text
game-design/<game-slug>/quality/<check-slug>/references/ACCESSIBILITY-LOCALIZATION-CHECK.md
```

## Output Format

```text
Check scope:
Target platform/languages/input:
Evidence:

Issue list:
| Severity | Dimension | Issue | Evidence | Suggestion | Handoff |

Passed items:
Risk assumptions:
Regression validation:
```

## Completion Criteria

- Key UI, tutorial, feedback, subtitle, input, and language risks have all been checked.
- Every issue has severity, evidence, suggestion, and handoff skill.
- Localization risk checks cover not only translation, but also text expansion, fonts, variables, and layout.
- Accessibility suggestions do not sacrifice the core gameplay promise; provide optional settings when needed.
- Pre-release risks are clearly routed to `game-release-liveops` or `game-research`.

## Quality Checks

- Do not treat "looks fine" as evidence; use screenshots, recordings, tables, devices, or manual flow descriptions.
- Do not check only short English/Chinese text; consider long languages and variable expansion.
- Do not give only art suggestions; state whether UI, input, audio, implementation, or release changes are needed.
- Do not treat accessibility as a last-minute release patch; high-risk items should return to design and implementation.
- Do not claim translation, compliance, or platform certification is complete.
