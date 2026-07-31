---
name: game-gdd
description: Turn an already clarified game idea into a lightweight, actionable GDD.
---

# Game Design Document

## When to Use

Use this skill when a game brief or design discussion is clear enough to become a lightweight GDD.

## When Not to Use

- The idea is still vague; use `game-brief` first.
- The user only wants feature slicing; use `game-feature-slicer`.
- The user only wants worldbuilding or narrative text.

## Inputs

- An accepted project brief or design notes.
- Existing project documents.
- Existing `game-design/<game-slug>/GAME_CONTEXT.md` and ADRs.

## Workflow

1. Read the relevant project documents before writing.
2. Use the existing game terminology from `game-design/<game-slug>/GAME_CONTEXT.md`.
3. If critical content is missing, ask only necessary questions.
4. Write the GDD according to `references/GDD-FORMAT.md`; prefer Markdown tables for rules, systems, resources, risks, and acceptance criteria.
5. Keep the document executable: rules, flows, resources, constraints, and acceptance criteria must be explicit.
6. Save it according to the project's existing directory convention. If unknown, default to `game-design/<game-slug>/gdd/`; if `<game-slug>` cannot be determined, confirm the mini-game name first.

## Completion Criteria

- The GDD describes the target player experience.
- The core loop, systems, content, UI, resources, constraints, and acceptance criteria are clear.
- The document can proceed directly to `game-feature-slicer`.

## Output Format

```text
GDD path:
Scope summary:
Main risks:
Next recommended skill:
```

## Quality Checks

- Avoid vague adjectives that carry no gameplay meaning.
- Do not write code-level implementation details that will age quickly unless they are necessary constraints.
- Mark out-of-scope items explicitly.
- Avoid long prose blocks; prefer tables for information that needs comparison, review, or feature slicing.

## Prohibited

- Do not create tasks before the GDD is accepted.
- Do not overwrite existing design documents without confirmation.
