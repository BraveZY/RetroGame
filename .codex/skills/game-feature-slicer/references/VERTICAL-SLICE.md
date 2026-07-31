# Vertical Slice Task Template

```markdown
# <task title>

## Parent Epic

| Item | Content |
|---|---|
| Epic |  |
| Child task ID |  |
| Coverage |  |

## Player-Visible Outcome

## Features to Implement

- [ ]

## Implicit Gameplay Semantics

Break the core gameplay verb into an action chain; if there are no implicit semantics, write "No additional implicit semantics."

```text
Actor -> intent/target -> prep/aim/warning -> spawn/execute -> trajectory/feedback -> target response -> result tracking
```

## Systems Involved

## Existing Behavior Affected

For a new project, write "No existing behavior impact." For an existing project, explicitly state which old systems, old UI, old input, old levels, old configuration, or old asset references are affected.

## Existing Behavior That Must Not Change

Existing projects must state the old behaviors this task must not change; if there are none, write "No explicit old-behavior constraints."

## Assets Involved

If there are no new assets, write "No new assets"; do not leave this blank.

## Acceptance Criteria and Evidence

- [ ] <verifiable result> (Evidence: screenshot/recording/PlayMode/device/automated test/manual flow)

## Regression Checks

- [ ] <old behavior still holds> (Evidence: screenshot/recording/PlayMode/device/automated test/manual flow)

## Dependencies

## Risks

## Slice Completeness Check

| Check | Result |
|---|---|
| Player-visible loop | Can it be seen, operated, or felt by the player, and does it cover the necessary gameplay/UI/data/assets/verification? |
| Action chain and granularity | Does it cover the core gameplay verb action chain and fit 0.5-2 days and 1-2 core gameplay verbs? |
| Tracking and verification | Does it reference the parent epic, support independent verification, and avoid horizontal splitting? |
```
