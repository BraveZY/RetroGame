# Unity Review Axes

## 1. Scope

- Does the behavior match the task or GDD?
- For existing projects, does the change cover only this increment?
- Are existing behaviors that should not change protected?
- Are acceptance criteria covered?
- Is there scope creep?

## 2. Unity Engineering

- Are lifecycle usage, serialization, namespaces, folders, and dependencies consistent?
- Is logic placed behind testable boundaries where practical?
- Were scenes and Prefabs handled intentionally?
- Are there opportunistic refactors, global-state migrations, or asset-reference migrations unrelated to this task?

## 3. Risk

- Are there risks in `Update`, allocations, asset loading, memory, package size, platform compatibility, or missing assets?
- For near-release or hotfix work, are there platform, build, rollback, or release-window risks?

## 4. Regression

- Is there still verification evidence for old features, old UI, old input, old levels, or old configuration?
- Are save data, remote configuration, player data, asset references, and Scene/Prefab compatibility clear?
- Are unverifiable items in the regression scope listed with unblock conditions?

## Finding Format

```text
Finding:
Location:
Evidence:
Risk:
Recommendation:
Verification:
```

Prioritize bugs, regressions, and verification gaps. Do not treat style preferences as primary findings.
