# Content Pipeline Recording Rules

During setup, record asset production and import conventions; do not move assets.

## Asset Directories to Identify

- Art: characters, scenes, icons, VFX, materials
- UI: screens, buttons, panels, atlases
- Audio: BGM, SFX, voice, ambience
- Animation: Animator, AnimationClip, Spine, skeletal animation
- Prefab: players, enemies, props, UI, VFX
- Configuration: ScriptableObject, JSON, CSV, Excel, remote configuration

## Conventions to Record

- Placeholder asset naming
- Prototype asset naming
- Production asset naming
- Import settings
- Atlas or Addressables grouping
- Whether the Agent may create placeholder assets
- Whether the Agent may modify Prefabs or Scenes

## Risk Recording

Record the following as risks or open questions:

- Production asset directories are unclear
- Placeholder and production assets are mixed
- Prefab changes have no review requirement
- A Scene is a collaboration hotspot file
- Addressables/AssetBundle grouping is unclear
