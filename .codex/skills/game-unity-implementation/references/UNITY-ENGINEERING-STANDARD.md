# Unity Implementation Engineering Standard

## Layered Responsibilities

- Plain C#: testable logic such as rules, state, numeric values, win/loss, scoring, and timers.
- MonoBehaviour: Unity lifecycle, Inspector wiring, input bridging, and presentation synchronization.
- ScriptableObject: configuration, curves, and asset references; do not use it by default for mutable global runtime state.
- Scene/Prefab: stores Unity wiring facts; component or screenshot evidence is required after changes.

## Anti-Overengineering

- Do not add interfaces, managers, event buses, DI, compatibility layers, or configuration knobs for possible future expansion.
- An interface with a single implementation must prove current replacement or testing value.
- Changes must map to current task acceptance items.
