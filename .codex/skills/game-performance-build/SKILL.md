---
name: game-performance-build
description: Check game performance, memory, package size, loading, build failures, and platform readiness.
---

# Performance and Build

## Use Cases

Use this skill when the user is concerned with FPS, GC, memory, Draw Call, package size, load time, build failures, or platform readiness.

## Non-Use Cases

- The current issue is only gameplay design.
- The user is reporting a functional bug with no performance or build symptoms; use `game-qa-debug` for that case.

## Inputs

- Target platform.
- Build logs, Profiler capture, device information, Project Settings, and asset settings.

## Workflow

1. Clarify the target platform and success thresholds.
2. Establish baseline measurements or build failure logs.
3. Check the performance, asset, build, and device axes according to `references/PERFORMANCE-BUILD-STANDARD.md`.
4. Propose fixes by priority.
5. Measure again after changes.

Unified standards:

- Use `references/PERFORMANCE-BUILD-STANDARD.md` for performance, build, device evidence, check axes, and release-blocker judgments.

## Completion Criteria

- Baseline and results are recorded.
- Recommendations are actionable.
- Platform-specific risks are clear.

## Output Format

```text
Target:
Baseline:
Findings:
Action items:
Verification:
Remaining risks:
```

## Quality Checks

- Measure before optimizing.
- Distinguish build failure root causes from performance tuning.

## Prohibited

- Do not claim performance improvement without measurement.
- Do not blindly change compression or stripping settings.
