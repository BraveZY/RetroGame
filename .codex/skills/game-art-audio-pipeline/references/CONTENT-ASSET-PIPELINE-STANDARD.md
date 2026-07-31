# Content Asset Pipeline Standard

## Asset Layers

| Type | Definition |
|---|---|
| Source | Editable source files such as PSD, AI, Blend, or WAV project files. |
| Export | Delivery files such as PNG, JPG, FBX, WAV, OGG, or Sprite Sheet. |
| Engine Asset | Texture, AudioClip, AnimationClip, or Material after Unity import. |
| Runtime Binding | Prefab, Scene, ScriptableObject, or Addressables/AssetBundle reference. |

## Required Fields

- Asset name
- Purpose
- Specification
- Delivery format
- Import settings
- Integration point
- Acceptance evidence

## Type-Specific Additions

| Type | Additional Records Required |
|---|---|
| Art / Image | Purpose, dimensions/resolution, camera or perspective, style reference, color constraints, export format, Unity import settings. |
| UI Asset | Owning screen or component, states, safe area, 9-slice, localization risk, atlas grouping. |
| Audio | Trigger timing, mood or function, duration, loop behavior, loudness target, format, Unity import settings. |
| Animation / VFX | Trigger condition, duration, loop behavior, Prefab/Animator integration point, performance risk. |

## Naming Recommendations

```text
ui_<screen>_<element>_<state>
sfx_<system>_<action>
bgm_<scene>_<mood>_loop
vfx_<system>_<event>
prefab_<feature>_<object>
```

## Acceptance Evidence

| Asset Type | Recommended Evidence |
|---|---|
| UI / Image | Screenshot, import settings, Prefab/UI component readback. |
| Sound Effect / Music | Listening check, loop point record, AudioClip import settings. |
| Animation / VFX | Screen recording, AnimationClip/Prefab reference check. |
| Platform Asset | Target device screenshot, package size or memory record. |

Do not claim final assets have been generated when no real files exist. Placeholder assets must identify replacement risk.
