---
name: game-handoff-game-context
description: Compress the current game-development context into a handoff document that a new agent session can continue from.
---

# Game Context Handoff

## Use Cases

Use this skill when a game development session is ending, branch exploration is needed, or the work needs to continue in a new Agent context.

## Non-Use Cases

- The current task is small and already complete.
- Existing documents already fully record the same context.

## Inputs

- Current conversation.
- Paths to GDD, tasks, code changes, ADRs, logs, builds, and verification results.

## Workflow

1. Clarify what the next session should do.
2. Reference existing artifacts instead of copying them again.
3. Summarize only the context needed to continue.
4. Note the recommended follow-up skills.
5. Remove secrets, credentials, private tokens, and personal sensitive information.
6. Save to the system temporary directory by default unless the user asks to place it in the project.

## Completion Criteria

- A new Agent can continue without reading the full conversation.
- Important artifacts are referenced by path or URL.
- Sensitive information has been removed.

## Output Format

```text
Handoff document path:
Next objective:
Recommended skills:
Key artifacts:
Open risks:
```

## Quality Checks

- Keep it concise.
- Do not repeat the body text of GDDs, tasks, diffs, or logs.

## Prohibited

- Do not commit the handoff document by default.
- Do not include sensitive information.
