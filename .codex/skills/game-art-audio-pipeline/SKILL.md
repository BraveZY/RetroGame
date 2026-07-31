---
name: game-art-audio-pipeline
description: Plan game art, UI, animation, sound effects, music, naming, import settings, and AI generation prompts.
---

# Art and Audio Asset Pipeline

## Use Cases

Use this skill when a task involves game visuals, UI, animation, sound effects, BGM, voice, naming, asset slicing, or import planning.

## Non-Use Cases

- The task is pure code implementation.
- The user wants to diagnose a runtime issue.

## Inputs

- GDD or feature specification.
- Existing style guide and asset directories.
- Target engine and platform constraints.

## Workflow

1. Read the style and content context.
2. Separate art, UI, animation, sound effect, BGM, and technical import requirements.
3. Output an asset list with names and intended uses.
4. Generate AI asset prompts or a production brief when needed.
5. Explain Unity import settings and Prefab usage.
6. Flag risks: resolution, atlas usage, compression, looping, loudness, memory, and platform limits.

Unified standards:

- Use `references/CONTENT-ASSET-PIPELINE-STANDARD.md` for content asset layering, naming, import, integration, and acceptance.

## Completion Criteria

- Art, audio, or AI tools can begin production from the output.
- Unity import and usage expectations are clear.
- Missing references or style decisions are listed.

## Output Format

```text
Style direction:
Asset list:
Naming/import rules:
AI prompts or production brief:
Unity integration notes:
Risks:
```

## Quality Checks

- Use concrete references and constraints, not only vague style words.
- Distinguish source assets from Unity-ready assets.

## Prohibited

- Do not claim final assets have been generated when no real files exist.
- Do not change the art direction for areas unrelated to the current feature.
