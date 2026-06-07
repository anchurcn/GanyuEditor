# 加载已有 GPD

## 1. 目标

支持把已有 `.gpd` 物理数据加载回当前 MDL 模型骨架：

- 导入 `.mdl` 时，自动尝试加载同目录同名 `.gpd`。
- 在带 `ModelInfo` 的 ModelRoot 上右键，可手动选择 `.gpd` 文件加载。

## 2. 当前范围

- 读取 `goldsrc-physics-data` XML。
- 校验 GPD 根节点与模型 checksum。
- 还原 collision shape、rigidbody、constraint 组件到现有骨骼对象。
- 加载前清理已存在的物理组件，避免重复叠加。

## 3. 不做的内容

- 不做 GPD 格式迁移，只支持当前导出的 `version="2.0"` 结构。
- 不支持无匹配骨骼的导入。
- 不实现批量导入多个模型。

## 4. 设计方案

新增 `PhysicsDataImporter`，与现有 `PhysicsDataExporter` 对称：

1. 读取 `collision-shape-block`，解析 shape/sub-shape 数据。
2. 读取 `rigidbody-block`，按 `bone` 找到 `StudioBone`，创建 `PhysicsBody` 与对应 `CollisionShape`。
3. 读取 `constraint-block`，在 rba 对应骨骼上创建约束组件，并连接 rbb 对应的 `PhysicsBody`。
4. 所有 GPD 中的矩阵按 GoldSrc 空间读取，乘以骨骼/刚体矩阵后转换回 Unity 空间。

## 5. 接入点/流程

```text
导入 .mdl
  ↓
创建 ModelRoot + 骨骼 + Mesh 预览
  ↓
PhysicsDataImporter.TryImportSameDirectory(ModelRoot)
  ↓
若同目录同名 .gpd 存在则加载
```

```text
ModelRoot 右键 LoadRagdoll...
  ↓
弹出 .gpd 文件选择框
  ↓
PhysicsDataImporter.Import(modelRoot, gpdPath)
```

## 6. 实施步骤

- 新增 GPD importer。
- 在 `ImportStudioBoneWizard` 的成功导入流程中调用同目录自动加载。
- 在 `ToolMenu` 增加 GameObject 右键菜单与校验方法。

## 7. 风险与待确认问题

- 旧 GPD 若不是当前导出器格式，可能无法加载。
- 导出器当前只记录 shape/rigidbody/constraint 的必要字段，未记录编辑器显示开关等辅助状态。
