# Unity Architecture Audit Report Standard

## Severity

| Level | Meaning |
|---|---|
| P0 | Currently blocks build, runtime, or core gameplay validation. |
| P1 | Has a high probability of causing regressions, state residue, Prefab/scene damage, or platform unavailability. |
| P2 | Increases maintenance cost, test cost, or future feature risk. |
| P3 | Style, naming, directory, or consistency issue. |

Express each finding as "evidence -> risk -> recommendation -> verification method".

## Health Check Priority

In periodic health-check mode, do not output a full technical-debt list. First select the top 1-3 improvement opportunities using these dimensions:

| Dimension | Judgment question |
|---|---|
| Delivery benefit | After the change, will later features ship faster, regress less, or connect to assets/platforms more easily? |
| Risk reduction | Will it reduce state residue, scene/Prefab damage, asset loading, build, or device risk? |
| Verification benefit | Will it make key rules, input, UI, or asset paths easier to test and reproduce? |
| Change cost | Can it be split into a 0.5-2 day task instead of a rewrite? |
| Timing | Is it related to the current version goal, recent tasks, or exposed pain points? |

Express improvement opportunities as "evidence -> current cost -> improvement benefit -> recommended small task -> verification method".
