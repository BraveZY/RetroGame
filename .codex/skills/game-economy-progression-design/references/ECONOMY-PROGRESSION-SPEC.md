# Economy / Progression Spec

## Basic Information

| Field | Content |
|---|---|
| Game / System |  |
| Version / Date |  |
| Business Model | Free / Premium / Ads / In-app purchases / Subscription / No monetization |
| Player Stage | New player / Early / Mid / Late / Returning |
| Design Source | GDD / Content spec / Event / Existing economy |

## Economy Goals

| Goal | Notes | Success Metric |
|---|---|---|
| Retention / Collection / Expression / Mastery / Content consumption / Monetization |  |  |

## Monetization Boundaries

| Boundary | Rule | Risk | Research / Compliance Needed |
|---|---|---|---|
| Payment |  |  |  |
| Ads |  |  |  |
| Random rewards |  |  |  |

## Resource Flow

| Resource | Type | Source | Sink | Cap / Hoarding | Purchasable | Risk |
|---|---|---|---|---|---|---|
|  | Soft currency / Hard currency / XP / Material / Energy / Item |  |  |  |  |  |

## Progression and Unlock Curve

| Stage | Player Goal | Unlock | Reward Peak | Spending Pressure | Risk |
|---|---|---|---|---|---|
| First session |  |  |  |  |  |
| First day |  |  |  |  |  |
| First three days |  |  |  |  |  |
| First week |  |  |  |  |  |
| Mid to long term |  |  |  |  |  |

## Rewards and Costs

| Event | Reward | Cost | Target Player Feeling | Validation Metric |
|---|---|---|---|---|
|  |  |  |  |  |

## Protections and Risks

| Risk | Symptom | Protection | Verification Needed |
|---|---|---|---|
| Inflation |  |  |  |
| Resource deadlock |  |  |  |
| Paywall |  |  |  |
| Veteran player overflow |  |  |  |
| New player catch-up difficulty |  |  |  |

## Data and Implementation Handoff

| Item | Notes | Handoff |
|---|---|---|
| Config table fields |  | game-unity-system-design / game-unity-implementation |
| UI display |  | game-ui-hud-flow-design |
| Balance validation |  | gameplay-balance-tuning |
| Platform / policy research |  | game-research |
| Release configuration |  | game-release-liveops |

## Acceptance Criteria

- [ ] Each resource has source, sink, and risk notes.
- [ ] Each key unlock point has a player motivation and validation metric.
- [ ] Payment / ad boundaries do not break the core fairness promise.
- [ ] Existing-project player assets and regression scope are identified.
