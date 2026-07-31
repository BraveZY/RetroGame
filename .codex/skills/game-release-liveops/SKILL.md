---
name: game-release-liveops
description: Prepare release notes, store assets, platform checks, SDKs, remote config, and live operations handoff.
---

# Release and LiveOps

## Use Cases

Use this skill when preparing a game build release, update, event, store submission, or live-ops configuration.

## Non-Use Cases

- The build has not reached an inspectable state.
- The user is only asking for feature implementation.

## Inputs

- Version scope.
- Build target.
- Store or channel requirements.
- SDK, privacy, ads, payments, remote config, and event configuration.

## Workflow

1. Confirm the release type: internal test, external test, soft launch, full release, hotfix, or live-ops event.
2. Read the version scope and build target documents.
3. Check the release checklist:
   - Release notes
   - Store assets
   - Privacy and permissions
   - SDK status
   - Monetization
   - Remote config
   - Rollback plan
   - Customer service/support notes
4. Output a release readiness summary.

Unified standards:

- Use `references/RELEASE-READINESS-STANDARD.md` for release types, checklist items, conclusion format, severity, and evidence requirements.

## Completion Criteria

- Blockers are clear.
- Release notes and live-ops handoff are usable.
- Every risk maps to a verification step.

## Output Format

```text
Release type:
Build target:
Readiness:
Blockers:
Checklist:
Release notes:
LiveOps notes:
Rollback/support:
```

## Quality Checks

- Version numbers or build numbers must be accurate when present.
- Platform rules may change; verify current rules when needed.

## Prohibited

- Do not submit builds or change live configuration without explicit instructions.
- Do not invent compliance status.
