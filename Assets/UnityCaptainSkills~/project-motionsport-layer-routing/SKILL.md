---
name: project-motionsport-layer-routing
description: MotionSport project layer router. Maps Entry/Input/Decision/State/Execution/Presentation/Data/Resource/Build symptoms to project owners, first evidence, forbidden misfixes, and next Unity Captain Skill. Read-only.
---

# MotionSport Layer Routing

## Use

- Route MotionSport symptoms by owner layer before touching code.
- Prevent Decision issues being patched in Presentation, or Data/Resource issues being hidden by runtime fallback.
- Project Skill overlay only; built-in Unity Captain Skills still own generic Unity tools and writes.

## Route Out

| Need | Skill |
|---|---|
| vague scene/object/component target | `unity-task-intake` |
| PlayMode behavior chain / override / interruption | `unity-runtime-evidence` |
| static UI structure | `unity-ui` |
| Prefab/material/texture/resource reference | `unity-asset` / `unity-asset-lifecycle` |
| C# fix/refactor | `unity-refactor` |
| compile/Console/exception | `unity-debug` |
| Android/HybridCLR/build | `unity-android-build` |
| MotionSport acceptance template | `project-motionsport-validation-contract` |

## Project Map

| Domain | Owner path / entry | First check |
|---|---|---|
| Tennis | `Assets/CoreGameScript/Tennis_Script`, `Assets/CoreGameAssets/Tennis_Assets` | AI/rule → state → Role/Task → Animator/UI |
| Basketball | `Assets/CoreGameScript/Basketball_Script`, `Assets/CoreGameAssets/Basketball_Assets`, `Assets/ADD/Resources/Basketball` | module entry, role control, shot/move/score UI |
| FootBall | `Assets/CoreGameScript/FootBall_Script`, `Assets/CoreGameAssets/FootBall_Assets` | player/ball/rule/state/presentation chain |
| Bowing / Bowling | `Assets/CoreGameScript/Bowing_Script`, `Assets/CoreGameAssets/Bowing_Assets`, `Assets/Resources/BowlingRole` | note project spelling `Bowing_Script` |
| Entry | `Assets/ADD/Scripts/Project/AddInit.cs`, current scene entry Managers | init order, singleton, scene startup |
| Android Build | `Assets/Editor/BuildAndroidCommand.cs` | build log, HybridCLR, HOT/AOT, PlayerSettings |
| Pose/Input | `Assets/Scripts/PoseAPI` | sensor/input bridge before AI/Animator |

## Project Index Use

- Index is candidate evidence only; Editor scene, Inspector, PlayMode, Roslyn, and Review diff remain authority.
- Flow: `context_export/scene_query/scene_diagnostics` first when scene truth is needed → `asset_database smart_context` with the best available clue → `reference_trace/impact_analysis` for impact → `script_editing/component_inspector` for authority.
- Do not make users choose index sub-actions. Use `smart_context` by default, then follow its recommended authority or manual fallback.
- Manual fallback only when smart_context is insufficient: use `find` for folders/scripts, `query_index/read_asset_node` for exact scene assets or GUIDs, and `query_semantic_index` for live component fields.
- Basketball examples: `smart_context target_object=Main component_type=BasketballShotClockController`; `smart_context query=BasketballShotClockController`; `smart_context asset_path=Assets/CoreGameAssets/Basketball_Assets/Scenes/Basketball_Main.unity`.
- Good clues: `Assets/CoreGameScript/Basketball_Script`, `BasketballShotClockController`, `PoseDataSourceManager`. Bad: `Basketball_Main Main Recorder Ball UI PoseAPIManager`.
- If no candidates, irrelevant candidates, or provider diff appears, mark index weak and return to Editor/Roslyn tools.

## Layer Routing

| Layer | Symptoms | First owner | First evidence | Forbidden misfix |
|---|---|---|---|---|
| Entry | scene cannot enter, init null, singleton missing | `AddInit.cs`, scene entry Manager, module init | Console, active scene, entry object, init order | no startup fallback in sport state machine |
| Input | pose/key/sensor invalid or inconsistent | `Assets/Scripts/PoseAPI`, module input bridge | input event/value/device/log | do not edit AI/Task/Animator first |
| Decision | AI idle, wrong target, delayed action, rule/probability wrong | module AI/rule/planner/target selector | decision input, target, branch, random/probability | no animation/speed/runtime fallback masking |
| State | state not switching, stolen back, round stuck | module main match/role state machine | current state, transition history, owner, condition | no forced Task/Animator/UI patch |
| Execution | Task issued but no move/hit/shot, invalid target | Role, Task, Move, executor, physics owner | task queue, target, direction/speed, Transform/Rigidbody window | do not rewrite Decision/State to hide execution failure |
| Presentation | animation/UI/VFX/audio wrong | Animator, UI View/Presenter, VFX, Audio binding | Animator params/state, Canvas nodes, fields, screenshot/runtime display | do not change AI/state/rules/data first |
| Data | value/probability/config/default wrong | SO/config table/module numeric class/static config | config source, runtime read value, override chain | no hardcoded logic fallback |
| Resource | Prefab/Clip/material/image/AB missing or wrong | Prefab, material, Clip, AssetBundle/load tools | path, reference, load log, Scene/Prefab fields | no runtime-created resource/UI fallback |
| Build | Android/HybridCLR/HOT/AOT/asmdef/package/platform fail | `BuildAndroidCommand.cs`, HybridCLR, asmdef, Package/PlayerSettings | compile/build log, assembly boundary, hot-update config | do not bypass build by gameplay logic change |

## Symptom Router

| User words | Main layer | Candidate layers | First actions |
|---|---|---|---|
| AI 不动/不接球/不出手 | Decision | State, Execution, Presentation | decision input/target → state switch → Task/Role → animation |
| 角色不动/跑不到位/朝向错 | Execution | State, Decision, Presentation | task/move owner → state permission → animation/root-motion override |
| 动画不播/动作硬切 | Presentation | State, Execution | logic state and Animator params first; then presentation |
| UI 分数/倒计时/结果不刷新 | Presentation | State, Data, Override | static binding + runtime data refresh → state/data source |
| 进不了场景/卡启动/黑屏 | Entry | Resource, Build, Presentation | compile/Console/active scene/entry object → resource → camera/render |
| 配置改了不生效/概率不对 | Data | Decision, Resource | config source + runtime read value → decision use point |
| Prefab/材质/Clip 丢失 | Resource | Presentation, Data | reference/load path → presentation |
| Android/热更失败 | Build | Resource, Platform | compile/build log/HybridCLR/HOT/AOT/asmdef |

## Rules

- One main layer, at most two candidate layers; more means evidence insufficient.
- User gives only sport/object name → route `unity-task-intake`.
- Runtime mid-frame override/state rollback/animation interruption/UI value rollback → `unity-runtime-evidence`.
- Static missing object/Prefab/resource → static Skill first, not PlayMode fallback.
- No evidence for Decision means no AI/rule edit; no evidence for Presentation means no Animator/UI tuning.
- Owner judgment must cite path, object, script, or tool evidence.
- Read-only by default; any write routes to owning Skill and Unity Captain Review/Diff contract.

## Stop

- No module/object/scene/log/repro clue to produce first evidence.
- Not a MotionSport project/sport-module issue.
- PlayMode needed but trigger is missing.
- Write requested before owner layer is determined.

## Output

```text
现象：
-

MotionSport 层级判断：
- 主层：
- 候选层：
- 暂不判断/已排除层：

Owner 首查：
- 模块：
- 路径/对象：
- 第一证据：

最短链路：
1.
2.
3.

禁止误修：
-

下一步主 Skill：
-

验证点：
-

验收口径：
- 使用 project-motionsport-validation-contract 的对应模板
- 未覆盖项：
```
