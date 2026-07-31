# Unity Architecture Audit Engineering Standard

## Audit Axes

- System boundaries: whether gameplay, input, UI, assets, data, and state machines are mixed together.
- MonoBehaviour responsibility: whether lifecycle, input reading, state mutation, and presentation updates are over-concentrated.
- Global state: whether Manager, Singleton, static variables, or DontDestroyOnLoad create implicit dependencies.
- Scene and Prefab: whether references are fragile and whether Prefabs carry business state.
- Data and configuration: whether ScriptableObject is configuration or implicit globally mutable state.
- Asset loading: whether Resources, Addressables, or AssetBundle create platform or memory risk.
- Test seam: whether rule logic can be tested outside the Unity lifecycle.
- Delivery health: whether recent features are repeatedly slowed, regressed, or made hard to verify by the same structural issues.
- Improvement opportunity: whether a small number of small changes can significantly reduce later delivery and verification cost.

Recommendations must be small, verifiable changes. Do not output one-time large rewrite plans.
