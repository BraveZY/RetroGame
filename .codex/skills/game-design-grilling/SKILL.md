---
name: game-design-grilling
description: Use one-question-at-a-time probing and options to clarify unclear game design, mechanics, scope, or resource decisions; when settled context needs persistence, pair it with domain modeling.
---

# Game Design Grilling

## When to Use

Use this skill when a game design needs deeper probing and clarification before it becomes documentation or implementation work.

Also prioritize this skill when the user says "discuss first", "ask me probing questions", "grill", "probe with documents", or "clarify while recording context".

## When Not to Use

- The answer can be found directly in existing project documents or code.
- The user has already confirmed the relevant design.

## Inputs

- The design decision that needs clarification.
- Existing GDD, `game-design/<game-slug>/GAME_CONTEXT.md`, task documents, or project files.

## Workflow

1. Look for the answer in the project context first.
2. If it is still unclear, ask only one question.
3. By default, provide 2-3 options when asking, so the user can move forward even if they do not know how to answer:
   - Generate the exact design decision as a complete question before presenting any recommendation or options. `Question:` is a system-generated heading, not a field for the user to fill; the heading alone is not sufficient.
   - Put the recommended option first and explain why it is recommended.
   - For each option, explain the tradeoff, production cost, or experience risk.
   - Allow the user to choose A/B/C directly or answer in their own words.
   - If the question must be open-ended, still provide a recommended answer or example answer first.
4. Resolve dependencies in this order:
   - Player behavior
   - Feedback and rewards
   - Failure state
   - Content needs
   - Technical needs
   - Acceptance criteria
5. If the user asks for documented consolidation, or if stable terms, system boundaries, or hard-to-reverse design decisions appear during grilling, follow the `game-domain-modeling` discipline:
   - Record only confirmed terms, boundaries, tradeoffs, and downstream effects.
   - Do not write every Q&A exchange into `GAME_CONTEXT.md`.
   - Do not treat `GAME_CONTEXT.md` as a brief, GDD, PRD, or task list.
   - Suggest an ADR only when there is a real tradeoff and the decision would be easy to misunderstand later.
6. If terminology is vague or conflicting, use the `game-domain-modeling` discipline to clarify it.

## Completion Criteria

- The design decision has a clear answer.
- The answer can be converted into GDD text, tasks, or acceptance criteria.
- In documented consolidation mode, confirmed terms and long-term decisions can be written by `game-domain-modeling` into `GAME_CONTEXT.md` or an ADR.

## Output Format

Keep the interview conversational. Summarize at the end:

Recommended format for a single follow-up question:

```text
Question: What should be the primary player behavior in the first 30 seconds of a match?

Recommended choice: A
Reason: It validates the core interaction with the smallest content and production scope.

Options:
A. Option name - Tradeoff explanation
B. Option name - Tradeoff explanation
C. Option name - Tradeoff explanation

You can choose A/B/C directly or answer in your own words.
```

End summary:

```text
Decision:
Chosen direction:
Rejected options:
Reason:
Downstream impact:
```

## Quality Checks

- Ask only one question per message.
- The system must generate the question body: it must be a complete, answerable sentence placed immediately after `Question:`. The user should only need to choose an option or answer naturally; never leave the question blank or replace it with a recommendation.
- Provide 2-3 options by default for every question; the user can answer by choosing an option or by free response.
- Probe gameplay consequences, not just preferences.
- Options must differ in substance, not just wording.
- The recommended option should serve the higher-probability path to a good game, not the most complex or flashy option.
- Stop asking low-value follow-ups once the design is executable.
- Documented consolidation is not real-time logging; confirm first, then record.

## Prohibited

- Do not implement features.
- Do not directly write a full specification.
- Do not automatically proceed to implementation after grilling ends.
- Do not write temporary preferences, unconfirmed guesses, or one-off prototype content into long-term documents.
- Do not invent answers that conflict with project files.
- Do not present a recommendation, options, or rationale before stating the question being asked.
