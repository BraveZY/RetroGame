# Release Readiness Standard

## Release Types

| Type | Goal |
|---|---|
| Internal test | Validate core gameplay, build, and device runnability. |
| External test | Validate retention, performance, crashes, feedback, and content completeness. |
| Production release | Submit to a channel or store. |
| Hotfix | Fix a live blocker or high-priority issue. |
| Liveops event | Configure events, rewards, remote parameters, or limited-time content. |

## Conclusion Format

```text
Release type:
Version/build:
Conclusion: ready to release / conditionally releasable / not releasable
Blockers:
Verified items:
Pending verification:
Rollback plan:
Liveops handoff:
```

Do not give a "ready to release" conclusion without build or device evidence.
