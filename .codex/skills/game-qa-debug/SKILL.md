---
name: game-qa-debug
description: Use for game bugs, regressions, build failures, performance issues, intermittent issues, or hard-to-reproduce runtime exceptions.
---

# Game QA and Debugging

## Use Cases

Use this skill when the user reports a game bug, build failure, runtime regression, performance drop, intermittent behavior, or device-specific issue.

## Non-Use Cases

- The user only wants to build a new feature.
- There is no concrete symptom yet, only a design concern.

## Inputs

- The symptom described by the user.
- Platform, device, build version, scene, reproduction steps, logs, screenshots, screen recordings, and Profiler capture.
- Project setup documentation.

## Workflow

1. Establish a feedback loop before guessing causes:
   - Automated tests
   - Reproducible scene
   - Input replay
   - Build command
   - Profiler capture
   - Structured manual checklist
2. Reproduce the exact symptom described by the user.
3. Reduce it to the smallest scene.
4. Propose 3 to 5 ranked, falsifiable hypotheses.
5. Validate only one hypothesis at a time. Temporary logs must use a unique prefix.
6. Fix the root cause.
7. Rerun the original feedback loop.
8. Add regression coverage, or explain why no correct test seam exists.
9. Remove temporary instrumentation.

Unified standards:

- Use `references/VERIFICATION-STANDARD.md` for feedback loops, regression evidence, and fix acceptance.

## Completion Criteria

- The original symptom no longer reproduces.
- The verification path is recorded.
- Temporary logs and prototypes are removed or isolated.
- The root cause is clearly explained.

## Output Format

```text
Symptom:
Feedback loop:
Minimal reproduction:
Root cause:
Fix:
Regression coverage:
Verification:
Remaining risks:
```

## Quality Checks

- Do not guess causes before a feedback loop exists.
- Distinguish "cannot reproduce" from "fixed."
- Measure performance issues before fixing them.

## Prohibited

- Do not leave debug logs behind.
- Do not claim a fix without rerunning the original scenario.
- Do not make broad architecture changes before understanding the bug.
