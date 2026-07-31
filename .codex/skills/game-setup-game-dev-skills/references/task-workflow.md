# Task Workflow Recording Rules

For task fields, parent-child relationships, acceptance evidence, and checkbox sync rules, `TASK-CONTRACT.md` is authoritative. This file only records how setup identifies and writes the task system currently used by the project.

During setup, only record the task system the project actually uses; do not force a migration.

## Recognizable Task Sources

- Local Markdown: for example `game-design/<game-slug>/tasks/`, `.scratch/`, `tasks/`
- GitHub Issues
- GitLab Issues
- Jira
- Tapd
- Feishu docs or multidimensional tables
- Notion
- Custom spreadsheet or configuration file

## Local Markdown Tasks

If the project uses local Markdown, record:

- Task directory
- File naming convention
- Single-task template
- Whether a parent task is required
- Whether tasks are split and refined by feature

## Acceptance Sync

Local Markdown tasks use checkboxes as the source of implementation and acceptance state; all fields, parent-child tasks, evidence types, and check conditions are governed by `TASK-CONTRACT.md`.

## External Task Systems

If an external task system is used, record:

- System name
- Access method
- issue/ticket link format
- Statuses or tags
- Whether the Agent may create tasks
- Whether the Agent may modify status

If permissions cannot be confirmed, default to read-only.
