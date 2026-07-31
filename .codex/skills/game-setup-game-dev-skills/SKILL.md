---
name: game-setup-game-dev-skills
description: Initialize agent working conventions for a game project, including engine facts, documentation layout, task workflow, asset pipeline, and build targets.
---

# Game Development Skill Library Setup

## When To Use

Use this skill before using this skill library in a game project for the first time, so the project has agent-readable working conventions.

This skill is not responsible for refactoring the project. Its job is to organize existing project facts into stable documents so later `game-brief`, `game-gdd`, `game-feature-slicer`, `game-task-triage`, `game-unity-system-design`, `gameplay-balance-tuning`, `game-unity-implementation`, and `game-qa-debug` know what to read first, how to verify, and what not to touch.

## When Not To Use

- The current directory is not a game project.
- The user is only inspecting this skill library itself.
- `game-design/<game-slug>/agents/*.md` already exists and is trustworthy, and the user did not ask to inspect or rebuild it.
- The user asks to implement a feature directly; in that case, first check whether setup already exists, and return to this skill only if it does not.

## Inputs

- Current repository path.
- Existing docs, project settings, task files, build scripts, and asset directories.
- Team conventions confirmed by the user when they cannot be inferred automatically.

## Workflow

### 1. Explore Project Facts

Read directories and key files first. Do not ask the user immediately.

Check engine signals:

- Unity: `Assets/`, `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`
- Unreal: `.uproject`, `Content/`, `Source/`
- Godot: `project.godot`

Check game documents:

- `README.md`
- `game-design/<game-slug>/agents/`
- `game-design/<game-slug>/GAME_CONTEXT.md`
- `game-design/<game-slug>/briefs/`
- `game-design/<game-slug>/gdd/`
- `game-design/<game-slug>/tasks/`
- `game-design/<game-slug>/adr/`
- `game-design/<game-slug>/balance/`
- `game-design/<game-slug>/system-design/`
- Legacy flat directories: `game-design/agents/`, `game-design/GAME_CONTEXT.md`, `game-design/briefs/`, `game-design/gdd/`, `game-design/tasks/`, `game-design/adr/`
- Existing project GDDs, tasks, config tables, or design documents

Legacy flat directories are only for recognizing existing projects or migration references. Do not use them as write targets for new deliverables.

When checking a Unity project, you must use the checklist in `references/unity-project.md`.

When checking build and verification targets, you must use the checklist in `references/build-targets.md`.

Standard references:

- Use `references/TASK-CONTRACT.md` for task fields, parent/child task rules, and acceptance evidence rules.
- Use `references/unity-project.md` for Unity project recognition and engineering conventions.
- Use `references/content-pipeline.md` for content asset conventions.
- Use `references/build-targets.md` for build and verification targets.
- After setup, use `references/setup-checklist.md` for self-check.

### 2. Judge Setup Status

Classify the project into three states:

```text
Not initialized: no game-design/<game-slug>/agents/, and no clear Agent working conventions.
Partially initialized: some game-design/<game-slug>/agents/*.md files exist, but task, asset, build, or domain conventions are missing.
Initialized: game-design/<game-slug>/agents/ is complete and explains engine, docs, tasks, assets, build, and domain language.
```

If the project is already initialized, summarize the current state and ask whether the user wants updates. Do not overwrite directly.

### 3. Summarize Findings and Ask Only Necessary Questions

First output what was found and what is missing. Ask the user item by item only when the project cannot answer it.

Common questions that may need to be asked:

- Where tasks actually flow: local Markdown, GitHub, Jira, Tapd, Feishu, Notion, or something else.
- What the primary target platform is: Editor, PC, Android, iOS, WebGL, or console.
- Whether asset production directories already have fixed conventions.
- Whether one project contains multiple minigames; if so, confirm or infer the current minigame's `<game-slug>`.
- If UnityCaptain / UnityMCP packages are detected but the current MCP is unavailable, confirm whether they should be the main Unity Editor verification entry point.
- Whether `AGENTS.md` or `CLAUDE.md` should be created or updated.

Ask only one question at a time.

### 4. Write or Update game-design/<game-slug>/agents

Write the following files according to `references/docs-layout.md`:

- `game-design/<game-slug>/agents/game-project.md`
- `game-design/<game-slug>/agents/references/task-workflow.md`
- `game-design/<game-slug>/agents/references/content-pipeline.md`
- `game-design/<game-slug>/agents/references/build-targets.md`
- `game-design/<game-slug>/agents/domain.md`

Write rules:

- Record only project facts and team conventions.
- Record multi-minigame layouts: each minigame has its own `game-design/<game-slug>/agents/` and `game-design/<game-slug>/GAME_CONTEXT.md`.
- Do not write temporary guesses as firm rules.
- Do not force the project to migrate to a new directory structure.
- When files already exist, prefer minimal updates. Do not rewrite the user's documents wholesale.

### 5. Optionally Update Agent Entry Files

If the project already has `AGENTS.md` or `CLAUDE.md`, add or update the `## Agent skills` section.

If neither exists, ask the user whether to create one. Do not create it by default.

The entry section should contain only an index. Do not copy the full text of `game-design/<game-slug>/agents/*.md`.

### 6. Self-Check

After finishing, self-check against `references/setup-checklist.md`.

If the current project is similar to this repository's `unity-skill-lab/`, it should recognize:

- Unity version from `ProjectSettings/ProjectVersion.txt`
- Runtime scripts in `Assets/Scripts/Runtime/`
- EditMode tests in `Assets/Scripts/Tests/EditMode/`
- Brief directory in `game-design/<game-slug>/briefs/`
- GDD directory in `game-design/<game-slug>/gdd/`
- Task documents in `game-design/<game-slug>/tasks/`
- Domain language in `game-design/<game-slug>/GAME_CONTEXT.md`
- The current task system is local Markdown
- UnityCaptain / UnityMCP packages and settings from `Packages/manifest.json` and `Assets/UnityCaptain/Resources/UnityCaptainSettings.asset`

## Completion Criteria

- The project has clear `game-design/<game-slug>/agents/` working convention documents.
- Later skills know where design documents, tasks, assets, build targets, and domain language live.
- Setup documents reflect the current project instead of imposing a new process.
- Uncertain items are explicitly recorded or confirmed with the user.

## Output Format

```text
Setup result:

Project type:
Detected engine:
Document layout:
Task workflow:
Asset pipeline:
Build/verification targets:
UnityCaptain / UnityMCP:
Domain language source:
Created/updated files:
Still needs user confirmation:
Recommended next skill:
```

## Quality Checks

- Preserve existing project conventions.
- Prefer reading files before asking the user.
- Mark uncertain information as unknown or pending confirmation.
- Do not directly fix test failures, build failures, or similar problems during setup. Only record the verification method or hand off to `game-qa-debug`.

## Prohibited

- Do not migrate the project structure.
- Do not create a new task system.
- Do not overwrite `AGENTS.md` or `CLAUDE.md`.
- Do not edit Unity scenes, Prefabs, assets, or C# code.
- Do not commit to git by default.
