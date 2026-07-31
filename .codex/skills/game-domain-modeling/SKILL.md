---
name: game-domain-modeling
description: Use when game terminology, economy concepts, content naming, system boundaries, or design decisions need clarification and durable recording.
---

# Game Domain Modeling

## When to Use

Use this skill when game vocabulary, system boundaries, content naming, or long-term design decisions need clarification.

## When Not to Use

- You only need to read existing vocabulary and no change is needed.
- A term is used only for a one-off prototype.

## Inputs

- Existing `game-design/<game-slug>/GAME_CONTEXT.md`.
- GDD, feature documents, config tables, code, or resource names.
- User-confirmed explanations.

## Workflow

1. If `game-design/<game-slug>/GAME_CONTEXT.md` exists, check the current terms first.
2. Point out terminology conflicts immediately when found.
3. Test concept boundaries with concrete gameplay scenarios.
4. Write confirmed terms into `game-design/<game-slug>/GAME_CONTEXT.md` according to `references/GAME-CONTEXT-FORMAT.md`.
5. Suggest creating an ADR only when the decision is hard to reverse, would be confusing without context, and involves a real tradeoff.

## Completion Criteria

- The term or decision has a single clear meaning.
- Future agents can find that meaning in the project documents.

## Output Format

```text
Added/modified terms:
Resolved conflicts:
Create ADR:
Downstream impact:
```

## Quality Checks

- `game-design/<game-slug>/GAME_CONTEXT.md` records game language, not implementation details.
- Do not create ADRs for temporary preferences.

## Prohibited

- Do not rewrite unrelated terms.
- Do not create broad, generic worldbuilding documents.
