# Unity System Design Verification Entry Points

The system design stage does not complete verification for the implementation stage. It only marks the follow-up verification entry point for each key module.

| Change type | Follow-up verification entry point |
|---|---|
| Rule layer | Automated tests or equivalent rule verification. |
| MonoBehaviour wiring | Unity Editor evidence, with PlayMode when needed. |
| Scene / Prefab / UI | Screenshots, component readback, or manual flow. |
| Device input / Performance / Build | Device records, Profiler, or build evidence. |
