---
name: game-brief
description: Clarify a vague game idea or existing-project feature change into a scoped project brief or feature delta brief that can continue into design or implementation.
---

# Game Project Brief

## When to Use

Use this skill when a game idea, feature direction, or project goal is still vague and the core experience and scope need to be clarified first. In an existing project, this skill can also produce a feature delta brief when the user wants to add or change functionality but the impact area, existing behavior protection, and regression scope are not yet clear.

The goal of this skill is to produce and save a brief. Do not turn it into an open-ended multi-round interview. Ask follow-up questions under the "one clarification question at a time" rule only when the save path, game ownership, core loop, or MVP boundary is too unclear to save reliably.

## When Not to Use

- A confirmed GDD already exists and the next step is feature slicing.
- An existing-project delta already has a clear feature delta brief and tasks; proceed to `game-task-triage` or `game-unity-implementation`.
- The user is reporting a bug or build failure.
- The user asks for direct implementation from a clear task.

## Inputs

- The user's game idea.
- Existing project documents or context.
- For an existing project: change requests, issues, current features, code/resource/config impact, and regression requirements.
- Constraints such as engine, platform, team size, schedule, and content capacity.

## Workflow

1. If working inside a project repository, read the existing context first.
2. Confirm where the brief should be saved and follow the "Save Location Rules".
3. Ask only one clarification question at a time.
4. Prioritize questions that decide scope:
   - What game objective must the player ultimately achieve?
   - What is the player doing every 10 seconds?
   - What is the core loop?
   - What is the launch platform?
   - What must the MVP include?
   - How much content can the team produce?
   - What is explicitly out of scope?
5. For every question, provide a recommended answer and explain the tradeoff.
6. If this is an existing-project delta, prioritize questions that decide impact:
   - What player-visible result is being added or changed?
   - Which existing behaviors must remain unchanged?
   - Which scenes, Prefabs, UI, input, resources, configs, or saves may be affected?
   - Which old flows need regression coverage?
   - What is explicitly out of scope for this change?
7. Stop asking once the game objective, core loop, target users, platform, MVP, constraints, and acceptance direction are clear, or once the existing-project change goal, impact area, and regression scope are clear.
8. Output and save a concise project brief or feature delta brief, preferably using Markdown tables.

## Save Location Rules

The `game-brief` output must be saved as Markdown unless the user explicitly asks to "only output it in chat".

Save priority:

1. The path explicitly specified by the user.
2. The design-document directory declared in the project setup document.
3. Default path: `game-design/<game-slug>/briefs/`.

`<game-slug>` is the stable short name of the current mini-game. When one project contains multiple mini-games, first identify or create the corresponding `game-design/<game-slug>/`, then write the brief there. Do not mix briefs for different mini-games under `game-design/briefs/`.

Naming rules:

```text
YYYY-MM-DD-<topic>-brief.md
YYYY-MM-DD-<feature>-delta.md
```

Example:

```text
game-design/minimal-shooter/briefs/2026-07-03-2d-shooter-brief.md
```

Path constraints:

- A brief belongs to a specific mini-game and should be saved under `game-design/<game-slug>/briefs/` at the project root.
- It should not be saved under the skill-library root `docs/`; that directory is only for skill-library documentation.
- If the current repository is the skill library itself, first determine whether there is an accompanying game project, such as `unity-skill-lab/`.
- If the current game project directory or `<game-slug>` cannot be determined, ask the user before writing the brief to the wrong location.

## Completion Criteria

- The game idea has a clear player action loop.
- The game objective is stated explicitly, such as survival, scoring, clearing levels, defeating opponents, collecting, creating, or setting a record.
- MVP scope is constrained.
- Major production risks are identified.
- A new-game brief can proceed to `game-gdd`; an existing-project feature delta brief can proceed to `game-task-triage`.
- The brief Markdown has been saved, or the user explicitly asked for chat-only output.

## Output Format

Use tables for the brief wherever possible. Use step tables only for strongly ordered content such as the core loop.

```markdown
# <Project Name> Project Brief

| Item | Content |
|---|---|
| One-line Positioning |  |
| Game Objective | State what the player ultimately needs to achieve and how performance is judged within one run/session. |
| Target Players |  |
| Launch Platform |  |
| Input Method |  |
| Art Style |  |
| MVP Goal |  |

## Game Objective

| Mode/Scenario | Player Objective | In-Run Decisions | Result or Success Criteria |
|---|---|---|---|
| Default Mode |  |  |  |

| Step | Player Action | System Feedback |
|---|---|---|
| 1 |  |  |

| Category | Content | Notes |
|---|---|---|
| Key Systems |  |  |
| Art Style |  |  |
| Resource Needs |  |  |
| Technical Constraints |  |  |
| Risks |  |  |
| Out of Scope |  |  |
| Acceptance Direction |  |  |

| Next Step | Recommended Skill |
|---|---|
| Continue Design | `game-gdd` |
```

Existing-project feature delta brief format:

```markdown
# <Feature or Change Name> Feature Delta Brief

| Item | Content |
|---|---|
| Current Game/Version State |  |
| Player Result to Add or Change |  |
| Existing Behaviors That Must Not Change |  |
| Affected Modules |  |
| Scene/Prefab/Resource/Config Impact |  |
| Save/Data/Compatibility Impact |  |
| Explicitly Out of Scope |  |
| Regression Scope |  |
| Acceptance Direction |  |
| Next Recommended Skill | `game-task-triage` |
```

The final reply must include:

```text
Brief path:
Next recommended skill:
```

## Quality Checks

- Do not expand worldbuilding details before gameplay is clear; in an existing project, do not rewrite the entire game setting.
- Recommendations must fit production reality.
- State assumptions explicitly.
- A new-game brief must explicitly state the game objective; do not substitute only a one-line positioning statement or MVP goal.
- An existing-project feature delta brief must state unchanged existing behaviors and regression scope.
- Check that the brief path is inside the specific game project.
- Avoid long stacked paragraphs; prefer tables for information that can be tabulated.
- When the user has named `game-brief` or asked to "write a brief", write the brief directly if enough information is available; do not switch to `game-design-grilling` first.

## Prohibited

- Do not write code.
- Do not create a full GDD before the brief is accepted.
- Do not force a full GDD when an existing project only needs a feature delta brief.
- Do not ask multiple unrelated questions in one message.
- Follow the save location rules.
