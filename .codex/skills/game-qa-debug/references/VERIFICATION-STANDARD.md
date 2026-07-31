# QA Debug Acceptance Evidence Standard

## Feedback Loop

Before fixing, choose one reviewable loop:

- EditMode, PlayMode, or unit tests
- Dedicated test scene
- Input replay
- Build command or CI job
- Profiler capture
- Device checklist with screen recording and timestamps

A usable loop must target the specific symptom, be repeatable or have a sufficiently high reproduction rate, support iteration, and either be runnable by the agent or provide a clear structure for manual execution.

## Fix Acceptance

- The original symptom must be revalidated.
- New validation cannot replace the original reproduction loop.
- Temporary logs, exploratory code, and instrumentation must be removed or promoted to permanent diagnostics.
- Unable to reproduce is not the same as fixed.
