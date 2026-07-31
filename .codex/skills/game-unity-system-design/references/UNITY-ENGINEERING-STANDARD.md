# Unity System Design Engineering Standard

## Layered Responsibilities

| Layer | Responsibility |
|---|---|
| Rule layer | Gameplay rules, state machines, numeric calculations, win/loss, scoring, cooldowns. |
| Unity Adapter | MonoBehaviour lifecycle, input bridging, Scene/Prefab wiring, presentation synchronization. |
| Data configuration | ScriptableObject, configuration tables, Prefab parameters, asset references. |
| Presentation layer | UI, animation, VFX, audio, feedback pacing, visual readability. |
| Platform layer | Build, permissions, device input, performance, package size, SDK. |

## Design Rules

- MonoBehaviour owns wiring and lifecycle. It should not swallow all gameplay rules.
- ScriptableObject is primarily for configuration. It should not carry mutable global runtime state by default.
- Create a seam only when it provides real replacement, testing, or verification value.
- Manager, Singleton, event bus, DI, or a new framework is not the default answer.
- Directory layering is not an architecture improvement by itself; explain call relationships, state ownership, and verification boundaries.
