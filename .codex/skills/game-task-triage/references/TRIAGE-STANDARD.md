# Task Triage Standard

## Implementability Judgment

Mark a task as "implementable" only when these facts are clear:

- The player-visible result is clear.
- Feature items can track implementation status.
- Acceptance items have evidence types.
- Dependencies, assets, Unity entry points, and verification entry points are clear.
- System boundaries, state ownership, and test seams do not block implementation.
- For existing projects, impact scope, protection of existing behavior, compatibility risk, and regression scope are clear.

## Status Priority

When multiple statuses apply, choose the primary status that blocks implementation the most:

`blocked > needs release-risk confirmation > needs compatibility/migration confirmation > needs device verification > needs technical research > needs system design > needs assets > needs regression scope > needs balance tuning > needs design clarification > implementable`

For tasks that are not "implementable", output only the minimum completion action. Do not expand the task into a full GDD redo.
