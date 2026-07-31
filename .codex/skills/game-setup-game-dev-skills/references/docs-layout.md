# game-design Document Layout

By default, `game-setup-game-dev-skills` writes Agent working conventions into the specific minigame directory at `game-design/<game-slug>/agents/`.

If the same Unity/game project will contain multiple minigames, each minigame's Agent working conventions, briefs, GDD, tasks, domain language, ADRs, balance data, and system design must live under its own `game-design/<game-slug>/`. Do not flatten and mix multiple minigames' Agent docs or design docs under `game-design/agents/`, `game-design/briefs/`, `game-design/gdd/`, `game-design/tasks/`, or `game-design/adr/`.

## game-design/<game-slug>/agents/game-project.md

Record:

- Engine and version
- Project type
- Current goal
- Recommended Agent read order
- Key project directories
- Design document directories, including default locations for briefs, GDD, and tasks

## game-design/<game-slug>/agents/task-workflow.md

Record:

- Task system
- Task directory or external system link
- Task template
- Statuses/tags
- Agent permissions

## game-design/<game-slug>/agents/content-pipeline.md

Record:

- Asset directories
- Naming rules
- Distinctions between placeholder, prototype, and production assets
- Unity import and Prefab/Scene modification rules
- Asset risks

## game-design/<game-slug>/agents/build-targets.md

Record:

- Target platforms
- Current verification priority
- Test commands or manual verification methods
- Build scripts
- Known build/test risks

## game-design/<game-slug>/agents/domain.md

Record:

- Domain language sources
- ADR directory
- `GAME_CONTEXT.md` usage rules
- Content that should not be written into domain docs

## Recommended Game Document Directories

If the project has no existing convention, setup can record these default directories:

```text
game-design/<game-slug>/agents/         # Agent working conventions for this minigame
game-design/<game-slug>/GAME_CONTEXT.md # Domain language for this minigame
game-design/<game-slug>/briefs/         # game-brief output
game-design/<game-slug>/gdd/            # GDD and feature design
game-design/<game-slug>/tasks/          # Verifiable feature tasks
game-design/<game-slug>/adr/            # Important design decisions
game-design/<game-slug>/balance/        # Balance and tuning records
game-design/<game-slug>/system-design/  # System design records
```

These directories belong to the specific game project, not to the skill library root documentation.

`<game-slug>` uses a stable short name for the minigame, such as `motion-dodgeball`, `blackboard-eraser`, or `minimal-shooter`. When creating a new minigame, create a new `game-design/<game-slug>/` first, then write that minigame's related docs inside it.

## Update Rules

- Create the file if it does not exist.
- If the file exists, prefer updating the relevant sections.
- If it cannot be merged safely, show the plan first and ask the user.
