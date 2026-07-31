---
name: game-brainstorm
description: Use for game brainstorming and micro-innovation that turns a player, context, input capability, theme, or rough mechanic into distinct, grounded, prototype-verifiable game directions. Avoid reskins, decorative technology, and novelty without gameplay value.
---

# Game Brainstorming

## When To Use

Use this skill before a project has a confirmed core loop, when the user needs to decide whether a theme, body-input capability, life observation, mechanic fragment, or visual seed is worth making into a game direction.

The outcome is **not** a large idea list or a GDD. It is 1-3 evidence-aware gameplay hypotheses that a team can realistically prototype, park, or kill.

Typical inputs:

- A known player and play context, such as family living-room party, classroom, or mobile commute.
- An available input capability, such as camera body tracking, microphone, phone motion, controller, or touch.
- A proven game loop that needs a meaningful new rule, not new art.
- A meme, scene, emotion, or visual reference that may be a game, level content, an event, or only presentation.

## When Not To Use

- The direction and core loop are selected and a project brief is needed: use `game-brief`.
- A confirmed brief needs a GDD: use `game-gdd`.
- A GDD or version goal needs task slicing: use `game-feature-slicer`.
- The work is an existing-project feature, stabilization, hotfix, or content/value iteration: use `game-feature-iteration`.
- The answer depends on real competitor, platform, market, SDK, or technical facts: use `game-research` first or in parallel.
- The work has entered implementation, debugging, performance, or release stages.

## Core Principles

- **Reality before novelty.** Start from the player, play context, available input, session length, and team boundary. A strange premise is never evidence of value.
- **One better decision.** Every recommended direction must identify a recurring player decision that is clearer, richer, or more social than the baseline loop.
- **Micro-innovation changes a high-leverage relationship.** A useful small change may alter the goal, consequence, failure, information, space, tempo, role, input mapping, or relationship between mechanics. It does not need to invent a new genre.
- **Playable in one round.** Explain the first 90 seconds as observable player actions and screen-state changes. Do not hide a weak loop behind lore, progression, or a long pitch.
- **Innovation is rule-level, not adjective-level.** A new theme, art style, control gesture, or fiction is not a differentiator unless it changes choices, pressure, feedback, or rule interactions.
- **Diverge across design dimensions, then converge.** Search at least four different innovation routes before judging. Generate 8-12 distinct sparks internally, then output no more than three serious candidates by default. The constraint is on what gets recommended, not on the breadth of the search.
- **Separate content role from decision.** Give every spark both a role (`Core`, `Level`, `Event`, `Presentation`) and a decision (`Greenlight`, `Park`, `Kill`).
- **Technology earns its place.** Input or technology must create a visible game-state change, better feedback, or a decision that simpler input cannot provide.
- **Prototype before promise.** A candidate is only a hypothesis until it has a smallest test, measurable success signal, and kill condition.

## Workflow

1. Read only the project context needed to establish the current game, available assets/input, intended players, and constraints. For pure creative chat, state the assumptions instead of inventing hidden facts.
2. Capture the **reality anchor**: player, play context, platform/input, session length, available assets/capabilities, and non-negotiable constraints. Ask one question only if its answer would materially change the loop.
3. State the baseline: what players already do, what is currently weak or missing, and which outcome should improve. If there is no baseline, use a familiar genre loop as an explicitly labelled assumption.
4. Build a compact **play-element inventory** from the anchor: player verbs, input signals, world states, goals, failure, timing, resources, information, roles, and social relationships. Generate 8-12 sparks across at least four micro-innovation operators; do not produce all sparks from one formula.
5. Filter those sparks to up to three grounded candidates. Each candidate must begin with `Because <anchor>, change <baseline rule> so players choose between <tradeoff>.`
6. For each candidate, run a 90-second round walkthrough and the value gate. Do not expand a candidate that fails either.
7. Classify its content role and decision separately. Keep `Park` and `Kill` concise; do not rescue them with more fiction.
8. Compare only viable candidates. Recommend one next action: a smallest prototype, one blocking question, or `game-research`.
9. Route to `game-brief` only after a candidate passes the prototype gate or the user explicitly chooses it as a working direction.

## Reality Anchor

Before ideation, capture this minimum brief in plain language:

| Item | Must establish |
|---|---|
| Target player | Who should immediately understand the appeal? |
| Play context | Solo, family living room, party, classroom, arcade, commute, livestream, or training? |
| Session | Typical round and total-session length. |
| Platform and input | What can players reliably do and what can the game reliably observe? |
| Existing foundation | Current loop, reusable systems, content, art, hardware, or input signals. |
| Desired improvement | What should become more fun, readable, social, strategic, or replayable? |
| Constraints | Team/time, safety, fatigue, camera space, player count, and platform limits. |

If fewer than three anchors are known, do not manufacture precision. Offer one bounded question or return labelled exploratory hypotheses only.

## Grounded Ideation Operators

Use these in this order. Do not lead with arbitrary combinations.

1. **Decision upgrade**: Add a tradeoff to the repeated choice: timing, position, risk, cooperation, information, or resource use.
2. **Goal mutation**: Change what counts as success, what must be preserved, or which partial victory matters.
3. **Failure transformation**: Turn a miss into a new playable state, debt, opportunity, role change, or recovery problem instead of a flat penalty.
4. **Information shift**: Hide, delay, expose, distort, share, or let players manipulate information that changes decisions.
5. **Space and tempo reframe**: Change how territory, distance, lanes, timing windows, order, or escalating pressure shape the same action.
6. **Role and social mutation**: Change ownership, cooperation, asymmetry, interference, spectatorship, or turn relationships.
7. **Input-to-world mapping**: Make an available input visibly shape, move, protect, herd, balance, aim, or transform the world.
8. **Mechanic coupling**: Connect two mechanics only when one changes the state, target, risk, or next use of the other. This is one micro-innovation route, not a required structure.
9. **Constraint or convention mutation**: Remove an expected ability, impose one productive constraint, or invert one convention only when it creates better decisions.
10. **Theme fit**: Use a meme, emotion, setting, or visual to clarify feedback and stakes after the playable change exists.

Use at least four relevant operators during divergence. Read `references/creative-operators.md` for the fuller operator map and anti-reskin checks.

## Value Gate

Every serious candidate must answer all six checks in specific terms:

| Check | Must answer |
|---|---|
| Player value | What recurring decision or skill becomes better than the baseline? |
| Innovation delta | Which goal, consequence, failure, information, space, tempo, role, input mapping, or mechanic relationship changed? What new decision or mastery does it create? |
| Input fit | What can the player physically do, what screen state changes, and why is this input meaningful? |
| One-round proof | What happens at 0-15, 15-45, and 45-90 seconds? |
| Content runway | How can the same rule produce at least three encounters or variations? |
| Build fit | What existing capability is reused, and what single uncertainty must the prototype test? |

If any check is vague, classify the direction as `Park` or `Kill`; do not compensate with lore, features, or adjectives.

## Prototype Gate

Every `Greenlight` recommendation must define:

| Field | Requirement |
|---|---|
| Smallest test | One encounter, paper prototype, spreadsheet simulation, graybox, or fake-input test; never a full game. |
| Test group | Who and how many people will test it. |
| Observation | The player behavior to watch: comprehension, replay request, discussion of tradeoffs, cooperation, fatigue, or confusion. |
| Greenlight threshold | A concrete threshold, such as 4 of 6 testers replaying without a new explanation. |
| Kill condition | A specific observation that stops expansion or sends the idea back to `Park`. |

Read `references/mechanic-innovation.md` when a candidate needs a rule-level rewrite or prototype design.

## Body-Input Feasibility Check

Use for camera, microphone, motion, dance, fitness, or party play. A body action must visibly change the game state; gesture-button reskins fail.

| Question | Required answer |
|---|---|
| Observable action | What large, repeatable action can the player do in the next 10 seconds? |
| Reliable signal | Which coarse signal is actually required: position, silhouette, pose, hand zone, volume, tempo, or movement energy? |
| World effect | What immediately moves, blocks, shapes, protects, balances, aims, or changes because of the action? |
| Play space | Player count, camera framing, standing area, occlusion risk, and safe recovery space. |
| Recognition failure | How does a missed/noisy read remain fair and recoverable? |
| Fatigue and access | Round length, intensity variation, and a lower-intensity or accessible equivalent where needed. |
| Spectator readability | What can a bystander understand from success, error, and recovery within five seconds? |

If the game needs precision the input cannot reliably supply, redesign the rule or make that input auxiliary feedback.

## Meme and Theme Check

For a meme, emotion, scene, or visual seed, decide its role only after the value gate:

| Question | Result |
|---|---|
| Does it create a repeated decision and visible feedback? | Candidate for `Core`. |
| Does it make an existing core loop clearer, funnier, or more varied? | `Level` or `Event`. |
| Is it memorable but mostly watched rather than played? | `Presentation`. |
| Does it need a fictional system to become interactive? | `Park` or `Kill`. |

## Output Format

Default output is intentionally compact:

1. Reality anchor and labelled assumptions.
2. Baseline loop / opportunity statement.
3. Up to three candidate cards; include `Park` or `Kill` when that is the honest result.
4. A comparison with evidence gaps and one recommended next action.

Do not expose all raw sparks by default. When the user asks for breadth or more surprising directions, show a compact `Micro-Innovation Sparks` list of 8-12 one-line ideas and label the operator used. Avoid returning many cosmetic variations of the same rule.

Each serious candidate must include:

- One-sentence premise: anchor, changed rule, and tradeoff
- Micro-innovation delta: what relationship changed and what new decision it creates
- Mechanic coupling chain when two mechanics are actually combined
- Content role + decision
- Player value over the baseline
- 90-second round walkthrough
- Core player verbs and visible feedback
- Input fit / body-input feasibility when relevant
- Content runway: three variations from the same rule
- Build fit and biggest uncertainty
- Smallest prototype, test group, threshold, and kill condition

Use `references/output-templates.md` for copyable formats.

## Relationship To Other Game Skills

| Situation | Recommended skill |
|---|---|
| Need a small set of grounded gameplay hypotheses from a seed, input, or weak loop | `game-brainstorm` |
| One unknown materially changes the loop and needs a focused discussion | `game-design-grilling` |
| Need current competitor, market, platform, SDK, or technical evidence | `game-research` |
| A selected, plausibly validated direction needs scope | `game-brief` |
| A confirmed brief needs full design | `game-gdd` |
| A confirmed design needs smallest tasks | `game-feature-slicer` |

## Prohibited

- Do not use random grafting, role reversal, genre reversal, or absurd variants as a default answer format.
- Do not require every idea to combine two mechanics. A strong change to goal, failure, information, space, tempo, role, or input mapping can be sufficient.
- Do not present two mechanics side by side as innovation. Show the causal coupling and the new decision it creates, or keep only the stronger mechanic.
- Do not call a theme, art direction, meme, body gesture, or technology a gameplay innovation unless it changes a recurring decision and visible game state.
- Do not invent competitor facts, platform constraints, or technology capabilities. Label assumptions or route to `game-research`.
- Do not output a feature pile, lore pitch, or progression system to conceal an unproven first-round loop.
- Do not treat `Park` or `Kill` as failures to be rescued; they are useful decisions.
- Do not write code, create Unity scenes, or enter implementation.
