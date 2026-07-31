# UI / HUD Flow Spec

## Basic Information

| Field | Content |
|---|---|
| Game / Feature |  |
| Version / Date |  |
| Design Source | GDD / Task / Playtest feedback / Existing flow |
| Platform | PC / Mobile / Web / Console / Large screen / Exhibition |
| Input Method | Keyboard and mouse / Touchscreen / Controller / Body motion / Local multiplayer |
| View and Distance | First person / Third person / Top-down / Fixed camera / Distant viewing |

## Player Scenarios

| Scenario | Player Goal | Current Pressure | Most Needed Information | Next Action |
|---|---|---|---|---|
|  |  |  |  |  |

## Information Hierarchy

| Information | Type | Timing | Priority | Presentation Recommendation | Hide Condition |
|---|---|---|---|---|---|
|  | Persistent / Conditional / Feedback / Low priority |  | P0 / P1 / P2 / P3 |  |  |

## HUD Spec

| HUD Element | Player Question | Data Source | Position | State Change | Feedback | Risk |
|---|---|---|---|---|---|---|
|  |  |  |  |  |  |  |

## Menu Flow

| Scenario | Entry Point | Actions Available | Default Focus | Return Path | Confirm / Cancel | Misinput Prevention |
|---|---|---|---|---|---|---|
|  |  |  |  |  |  |  |

## Results Flow

| Result | Must Explain | Reward / Loss | Next Button | Restart Path | Failure Learning Cue |
|---|---|---|---|---|---|
| Victory |  |  |  |  |  |
| Failure |  |  |  |  |  |

## Settings and Accessibility

| Setting | Default Value | Applicable Platform | Required in First Version | Notes |
|---|---|---|---|---|
| Volume |  |  |  |  |
| Graphics / Performance |  |  |  |  |
| Input sensitivity |  |  |  |  |
| Subtitles / Text size |  |  |  |  |
| Color-vision support / Contrast assist |  |  |  |  |

## Onboarding

| Trigger Condition | Teaching Goal | Prompt Content Placeholder | Player Action | Success Feedback | Failure / Retry |
|---|---|---|---|---|---|
|  |  |  |  |  |  |

## Distance Readability

| Check Item | Goal | Current Judgment | Risk | Verification Method |
|---|---|---|---|---|
| Text size hierarchy | Key text is readable from a distance |  |  |  |
| Contrast | Important information is not swallowed by the background |  |  |  |
| Icon semantics | Meaning is understandable without reading text |  |  |  |
| Safe area | Not clipped by screen edges, notches, or projection bounds |  |  |  |
| Local multiplayer | Each player knows which information belongs to them |  |  |  |

## Asset Requirements

| Asset | Purpose | Specification | Status | Handoff |
|---|---|---|---|---|
|  |  |  | Missing / Placeholder / Existing | game-art-audio-pipeline / implementation |

## Unity Integration Boundary

| UI Element | Prefab / Canvas | State Source | Event Entry Point | Animation / SFX Trigger | Acceptance Method |
|---|---|---|---|---|---|
|  |  |  |  |  |  |

## Acceptance Criteria

- [ ] The player can understand the current goal or next action within 3 seconds.
- [ ] Key state changes have visual or audio feedback.
- [ ] Persistent HUD does not block the core gameplay area.
- [ ] Menu, results, and pause flows have clear return paths.
- [ ] Key text and icons are readable at distance, on small screens, and on the target platform.
- [ ] Existing UI behavior and regression scope for existing projects are identified.

## Out of Scope

-

## Follow-Up Handoff

| Item | Handoff Skill | Notes |
|---|---|---|
| UI visual assets, icons, motion, SFX | game-art-audio-pipeline |  |
| Unity UI implementation | game-unity-implementation |  |
| Post-playtest understanding and readability validation | game-playtest-ux-evaluation |  |
