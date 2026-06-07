# 加载已有 GPD Devlog

> 对应方案文档：`load-existing-gpd-plan.md`

---

## 初始实现

### 用户反馈

用户希望实现加载已有 `.gpd`：

1. 导入 `.mdl` 时同时尝试加载同目录下的 `.gpd`。
2. 可以在 ModelRoot 右键加载 `.gpd...`，弹框选择指定 `.gpd` 目录/文件。
3. 工作流参考 `AgentsWorkflow`。

### 已调整

- 按项目约定新增方案文档与 devlog。
- 确认现有导出流程在 `PhysicsDataExporter`，导入 MDL 流程在 `ImportStudioBoneWizard`，右键导出菜单在 `ToolMenu`。

### 实现计划

- 新增 `PhysicsDataImporter` 解析当前导出器生成的 XML。
- 自动加载路径使用 `ModelInfo.OutputPath`，即与 MDL 同目录同名 `.gpd`。
- 手动加载使用 `EditorUtility.OpenFilePanel`。

### 已完成

- 新增 `PhysicsDataImporter`：
  - 校验 GPD 根节点与 checksum。
  - 清理旧物理组件。
  - 解析 shape、rigidbody、constraint 并还原到骨骼对象。
- `ImportStudioBoneWizard` 在 MDL 导入成功后调用 `TryImportSameDirectory`，同目录同名 GPD 存在时自动加载。
- `ToolMenu` 新增 ModelRoot 右键 `LoadRagdoll (.gpd)...`，弹窗选择 GPD 文件并导入。

### 验证情况

- IDE diagnostics 未发现新增脚本编译问题。

---

## 日志与菜单修复

### 用户反馈

- 加载 GPD 需要添加适当日志。
- `LoadRagdoll` 菜单项没看见。

### 已调整

- `PhysicsDataImporter` 增加自动加载尝试、文件缺失、开始导入、骨骼数量、清理数量、rigidbody/constraint 导入数量、最终汇总日志。
- `LoadRagdoll (.gpd)...` 菜单移除验证方法，避免 Unity GameObject 菜单校验导致菜单不可见；执行时再检查是否是带 `ModelInfo` 的 ModelRoot。
- 菜单优先级调整为 `10`，与 Unity `GameObject` 右键菜单常见分组一致。

### 验证情况

- IDE diagnostics 未发现新增问题。

---

## Hierarchy 菜单分组

### 用户反馈

- 希望统一把 Hierarchy 里的 Goldsrc 相关菜单放进一个 `GoldsrcPhysics` 菜单下。

### 已调整

- `GameObject/ExportRagdoll (same path)` 改为 `GameObject/GoldsrcPhysics/ExportRagdoll (same path)`。
- `GameObject/LoadRagdoll (.gpd)...` 改为 `GameObject/GoldsrcPhysics/LoadRagdoll (.gpd)...`。
- `GameObject/SetupRagdoll...` 改为 `GameObject/GoldsrcPhysics/SetupRagdoll...`。
- `AddHinge(Auto Parent)` 与 `AddConeTwist(Auto Parent)` 改入 `GameObject/GoldsrcPhysics/` 子菜单。
- 顺手修正 `AddConeTwist` 的 validation 菜单路径，使其与执行菜单路径一致。

### 验证情况

- IDE diagnostics 未发现新增问题。

---

## 子菜单条目被隐藏修复

### 用户反馈

- `GoldsrcPhysics` 子菜单里只剩 `LoadRagdoll`、`SetupRagdoll`、`ExportRagdoll`，缺少骨骼相关菜单。

### 原因

- `AddHinge(Auto Parent)` 与 `AddConeTwist(Auto Parent)` 保留了 validation 方法。
- 当前右键的是 ModelRoot 而不是 `StudioBone` 时，validation 返回 false，Unity 会隐藏/禁用这些菜单项。

### 已调整

- 移除这两个菜单项的 validation 方法，使其在 `GoldsrcPhysics` 子菜单下始终可见。
- 执行时再检查是否右键了 `StudioBone`，并检查是否有 `PhysicsBody`，不满足时输出日志后返回。

### 验证情况

- IDE diagnostics 未发现新增问题。

---

## 尝试恢复 Validation

### 用户反馈

- 希望尝试加回 Hierarchy 菜单 validation。

### 已调整

- 为 `AddHinge(Auto Parent)` / `AddConeTwist(Auto Parent)` 加回 validation。
- validation 改为无参方法，使用 `Selection.activeGameObject` 判断，避免之前依赖 `MenuCommand` 的右键上下文校验。
- 约束菜单仅在选中对象同时具有 `StudioBone` 与 `PhysicsBody` 时通过。
- 为 `LoadRagdoll`、`ExportRagdoll`、`SetupRagdoll` 也增加 ModelRoot validation：选中对象需要包含 `ModelInfo`。

### 验证情况

- IDE diagnostics 未发现新增问题。




