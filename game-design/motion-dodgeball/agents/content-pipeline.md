# 资源管线

当前样例优先使用原型资源验证体感丢沙包玩法闭环。

资源目录：

```text
Assets/Art/
Assets/Audio/
Assets/Prefabs/
```

规则：

- 占位资源命名使用 `Placeholder_` 前缀。
- 原型资源命名使用 `Prototype_` 前缀。
- 正式资源接入前，需要更新 GDD 或任务文档。
- 不在功能实现任务中随意移动资源目录。
- MVP 必需资源优先覆盖场地、躲避者 C、投手 A/B、沙包、HUD、命中反馈和结算界面。
