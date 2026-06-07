# Assimp for Unity MDL Mesh 渲染方案

**日期**: 2025-01-XX
**目标**: 在 GanyuEditor 导入 `.mdl` 时，除现有骨骼显示外，同步通过 Assimp 加载并渲染模型 Mesh，辅助骨骼与刚体编辑时观察模型外形。
**阶段策略**: 不再支持多 Body Part 选择与显隐；第一阶段只要求 Assimp 成功导入模型并以无贴图材质显示全部 Mesh。

---

## 1. 背景与参考

参考文档：
- 安装：https://intelligide.github.io/assimp-unity/installation/
- 使用：https://intelligide.github.io/assimp-unity/usage/

Assimp 采用用户本地构建的 AssimpNet UnityPlugin，而不是 UPM 版本。插件放置位置：

```text
Assets/Plugins/AssimpNet/
├── AssimpNet.dll
├── AssimpUnity.cs
└── Native/
```

AssimpNet 主要使用方式：

```csharp
using Assimp;

AssimpContext importer = new AssimpContext();
Scene scene = importer.ImportFile(path, PostProcessPreset.TargetRealTimeQuality);
```

可以从 `Scene.Meshes` 读取顶点、法线、UV、面片，从 `Scene.Materials` 读取材质与贴图信息。当前方案优先使用 Mesh 数据构建 Unity `GameObject + MeshFilter + MeshRenderer` 预览对象。

---

## 2. 功能目标

### 2.1 必须实现

1. **导入 MDL 并同步生成 Mesh 预览**
   - 在现有 `.mdl` 导入流程中接入 Assimp。
   - 目标效果是：导入后场景中同时看到骨骼和模型 Mesh，而不是只看到骨骼。
   - Mesh 预览跟随当前模型根对象，便于和骨骼、刚体、约束 Gizmo 对齐。

2. **渲染 Assimp 导入的全部 Mesh**
   - 不再做 Body Part 选择、Body Group 切换或局部显示/隐藏。
   - 遍历 `Scene.Meshes`，为每个 Assimp Mesh 创建 Unity `MeshFilter + MeshRenderer`。
   - 所有 Mesh 默认显示。

3. **无贴图模型绘制**
   - 第一阶段使用统一纯色材质即可，例如半透明浅灰或不透明浅灰。
   - 如果贴图处理麻烦，可以暂时完全忽略 Assimp Material 的贴图路径。
   - 正确处理坐标系、缩放、法线、三角面索引。

4. **骨骼编辑辅助**
   - 模型 Mesh 仅作为可视化参考，不参与物理导出。
   - 骨骼、碰撞体、约束等 Gizmo 始终叠加显示在 Mesh 上。

### 2.2 暂缓实现

1. Body Part 选择、Body Group 选项切换、局部 Mesh 显示/隐藏。
2. 带贴图材质还原。
3. 复杂动画、蒙皮权重实时预览。
4. 模型编辑或导出。

---

## 3. 总体架构

建议新增或接入一个轻量的 Mesh 预览模块，重点是服务 `.mdl` 导入后的可视化：

```text
Assets/
├── Scripts/
│   └── ModelPreview/
│       ├── AssimpModelImportService.cs
│       ├── AssimpUnityMeshBuilder.cs
│       ├── MdlMeshPreviewRoot.cs
│       └── MdlMeshPreviewController.cs
└── Editor/
    └── ModelPreview/
        └── MdlMeshPreviewInspector.cs
```

说明：
- `AssimpModelImportService`：封装 `AssimpContext`、导入参数和错误处理。
- `AssimpUnityMeshBuilder`：将 Assimp Mesh 转为 Unity `Mesh`。
- `MdlMeshPreviewRoot`：挂在预览根对象上，保存导入路径、缩放、生成的 MeshRenderer 列表。
- `MdlMeshPreviewController`：负责清理旧预览、重新导入、生成 Mesh 子对象。
- `MdlMeshPreviewInspector`：可选，用于在 Inspector 中提供重新生成、清理 Mesh 预览等按钮。

---

## 4. 导入与渲染流程

不再做 Body Part 映射。导入 `.mdl` 时的目标流程如下：

```text
用户导入 MDL
  ↓
现有流程生成/显示骨骼
  ↓
AssimpModelImportService 使用同一路径导入模型
  ↓
遍历 scene.Meshes
  ↓
AssimpUnityMeshBuilder 为每个 Mesh 创建 Unity Mesh
  ↓
在模型根对象下创建 MeshPreview 子节点
  ↓
MeshFilter + MeshRenderer 显示无贴图模型
```

实现要点：
1. Mesh 预览对象应挂在当前 `.mdl` 模型根对象下，命名建议为 `AssimpMeshPreview`。
2. 每次重新导入前先删除旧的 `AssimpMeshPreview`，避免重复生成。
3. Assimp 导入失败时不影响现有骨骼导入流程，只记录错误日志。
4. 如果 Assimp 无法直接读取目标 `.mdl`，需要确认是否先转换为 Assimp 支持的中间格式。

---

## 5. 显示设计

当前需求是“导入 mdl 时能同时显示 Mesh”，因此第一阶段不提供 Body Part 显隐 UI。

默认行为：
- 导入后全部 Mesh 都显示。
- MeshRenderer 默认启用。
- 如果需要临时隐藏整个模型 Mesh，可只对 `AssimpMeshPreview` 根节点使用 `SetActive(false)`，或关闭其子 Renderer。

建议默认材质：
- 使用无贴图纯色材质。
- 颜色建议浅灰或淡蓝灰，避免遮挡骨骼 Gizmo。
- 如需更清楚观察骨骼，可使用半透明材质，但需要注意 Unity 透明排序问题。

---

## 6. 无贴图绘制实现要点

1. 顶点：Assimp `Vector3D` 转 Unity `Vector3`。
2. 法线：如果有法线则导入；没有则 `mesh.RecalculateNormals()`。
3. UV：第一阶段可忽略；保留接口以便后续贴图。
4. 三角面：只接受三角面；导入时必须启用三角化后处理。
5. 材质：
   - 默认材质：统一浅灰、淡蓝灰或半透明灰。
   - 不按 Body Part 分色。
   - 后续如有需要再读取 `scene.Materials` 和贴图路径。
6. 坐标系：使用 `PostProcessPreset.ConvertToLeftHanded` 或项目已有矩阵转换规则，避免模型镜像、旋转错误。
7. 父子关系：第一阶段可以不完整还原 Assimp Node 层级，只要 Mesh 能在正确位置显示；如位置异常，再补充 Node Transform 递归转换。

建议导入参数：

```csharp
PostProcessSteps.Triangulate |
PostProcessSteps.GenerateNormals |
PostProcessSteps.JoinIdenticalVertices
```

如果模型法线本身可靠，可将 `GenerateNormals` 改为按需使用。

---

## 7. 与现有 MDL 导入流程的接入点

当前问题是导入 `.mdl` 后只显示骨骼。因此 Mesh 预览应接在“骨骼创建完成后”：

1. 现有导入逻辑读取 `.mdl` 并创建骨骼对象。
2. 获取模型文件路径与模型根对象。
3. 调用 `MdlMeshPreviewController.Rebuild(modelPath, modelRoot)`。
4. Controller 清理旧预览并调用 Assimp 导入。
5. 生成 `AssimpMeshPreview` 子对象，显示全部 Mesh。

伪代码：

```csharp
var modelRoot = BuildSkeletonFromMdl(mdlPath);
MdlMeshPreviewController.Rebuild(mdlPath, modelRoot.transform);
```

失败处理：
- Assimp 导入失败时，仅输出 `Debug.LogWarning` 或 `Debug.LogError`。
- 不阻断骨骼导入。
- 不改变现有骨骼和物理数据结构。

---

## 8. 实施步骤

1. 安装 Assimp for Unity 包。
2. 新增 `AssimpModelImportService`，验证 `.mdl` 是否能被 Assimp 直接导入。
3. 新增 `AssimpUnityMeshBuilder`，完成 Assimp Mesh 到 Unity Mesh 的转换。
4. 新增 `MdlMeshPreviewController`，负责在模型根节点下生成 `AssimpMeshPreview`。
5. 在现有 `.mdl` 导入流程中调用 Mesh 预览生成逻辑。
6. 与骨骼显示联调，确认 Mesh 与骨骼坐标、缩放、旋转一致。
7. 如果 Assimp 无法直接读取目标 `.mdl`，再评估中间格式或项目内 MDL Mesh 解析方案。
8. 记录问题并决定是否进入贴图材质阶段。

---

## 9. 风险与待确认问题

1. Assimp for Unity 是否能直接读取当前目标 `.mdl` 文件；GoldSrc MDL 与其它 MDL 格式可能存在差异。
2. Assimp 导入后的坐标系、单位缩放是否和当前骨骼一致。
3. Assimp 是否能保留 MDL Mesh 的正确局部变换；如果 Mesh 位置异常，需要递归应用 Node Transform。
4. 预览工具是否只需要 Editor 模式可用，还是运行时也要可用。
5. 如果 Assimp 不支持该 `.mdl`，是否允许使用现有 MDL 解析数据直接构建 Unity Mesh。

---

## 10. 建议优先级

P0：导入 `.mdl` 后同步显示无贴图 Mesh，并与骨骼对齐。
P1：处理坐标系、缩放、Node Transform 等显示正确性问题。
P2：贴图材质还原。
P3：动画/蒙皮预览。
