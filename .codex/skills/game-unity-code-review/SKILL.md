---
name: game-unity-code-review
description: Review Unity changes for spec fit, increment scope, engineering quality, regression risk, resources, scenes, Prefabs, or build impact.
---

# Unity Code and Engineering Review

## Use Cases

Use this skill when reviewing Unity changes, a WIP branch, a PR, or a local diff.

## Non-Use Cases

- There is no diff or change content.
- The user is asking for implementation, not review.

## Inputs

- Fixed comparison point or diff.
- GDD, feature increment brief, task, or acceptance criteria.
- Project standards and setup documentation.

## Workflow

1. If reviewing a diff, first fix the comparison point.
2. Find the specification source.
3. Review along the Spec / Engineering / Runtime / Regression axes in `references/REVIEW-AXES.md`.
4. Report findings by severity and cite file locations.
5. Keep review axes separate when needed; do not mix them together.

Unified standards:

- Use `references/REVIEW-AXES.md` for three-axis review, finding format, and verification gaps.

## Completion Criteria

- Issues are actionable and based on actual changes.
- Missing specifications or missing verification are called out.
- Unrelated refactoring suggestions are not mixed in.

## Output Format

```text
Findings:
Open questions:
Verification gaps:
Regression gaps:
Summary:
```

## Quality Checks

- Prioritize bugs and regression risks over style preferences.
- State clearly when there are no issues.
- For existing-project reviews, state whether old behavior, asset references, scenes/Prefabs, config/save data, and release risks still have verification gaps.

## Prohibited

- Do not change code during review unless the user explicitly asks.
- Do not repeat issues already checked automatically by existing tools as major findings, unless the tool is missing or misconfigured.
