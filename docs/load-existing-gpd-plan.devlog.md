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

