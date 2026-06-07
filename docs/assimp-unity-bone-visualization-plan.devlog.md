# Assimp for Unity MDL Mesh 渲染方案 Devlog

> 对应方案文档：`docs/assimp-unity-bone-visualization-plan.md`

---

## 2025-01-XX

### 已完成

- 阅读 Assimp for Unity 安装文档。
  - 支持通过 Unity Package Manager scoped registry 安装。
  - manifest 中需要加入 `com.frozenstorminteractive` registry 与 Assimp 相关依赖。
- 阅读 Assimp for Unity 使用文档。
  - 核心入口为 `AssimpContext`。
  - 可通过 `ImportFile` 或 `ImportFileFromStream` 导入模型。
  - 可访问 `Scene.Meshes`、`Scene.Materials`、Node 层级等数据。
- 初步查看项目结构。
  - 当前已有 `Assets/Scripts/Physics`、`Assets/Scripts/Extensions`、`Assets/Editor`、`docs` 目录。
- 新增方案文档：
  - `docs/assimp-unity-bone-visualization-plan.md`
- 初版曾明确第一阶段范围：
  - 先实现无贴图绘制。
  - 曾考虑 Body Part 显示/隐藏与 Node/Mesh 名称映射。
  - 该方向已在后续需求调整中废弃，当前不再支持多 Body Part。

### 初版待确认记录

1. 初版曾询问目标模型格式；现已明确核心场景为 `.mdl`。
2. 初版曾询问 Body Part 数据来源；现已明确不做多 Body Part 支持。
3. 仍需确认预览工具是否只需要 Editor 模式可用。
4. 仍需确认模型 Mesh 与骨骼坐标系是否已有统一转换规则。

### 下一步计划

1. 确认 Assimp 包安装方式并更新 `Packages/manifest.json` 方案。
2. 编写 `AssimpModelImportService` 与 `AssimpUnityMeshBuilder` 的实现草案。
3. 在现有 `.mdl` 导入流程中寻找骨骼生成完成后的接入点。
4. 实现导入 `.mdl` 后自动生成 `AssimpMeshPreview`。


---

## 2025-01-XX 需求调整

### 用户反馈

- 不需要支持多 Body Part 显示/隐藏。
- 核心需求改为：导入 `.mdl` 时，除了当前已有骨骼显示，还要同步显示 Mesh。
- 可以直接使用 Assimp 导入并渲染 Assimp 模型。
- 贴图仍可暂缓，先实现无贴图 Mesh 显示。

### 已调整

- 更新方案文档标题与目标：改为“Assimp for Unity MDL Mesh 渲染方案”。
- 删除 Body Part 映射、Body Group、多选显示/隐藏相关方案。
- 将第一阶段目标收敛为：
  - `.mdl` 导入后调用 Assimp。
  - 遍历 `scene.Meshes`。
  - 生成 Unity `MeshFilter + MeshRenderer`。
  - 在模型根对象下创建 `AssimpMeshPreview` 子节点。
  - 默认显示全部 Mesh。
- 明确 Mesh 预览接入点：现有骨骼创建完成后，再生成 Assimp Mesh 预览。
- 明确 Assimp 导入失败不应阻断现有骨骼导入流程。

### 新的待确认点

1. Assimp 是否能直接读取当前项目导入的 GoldSrc `.mdl`。
2. 如果 Assimp 不能读取，是否接受使用项目内已有 MDL 解析结果直接构建 Unity Mesh。
3. Mesh 与骨骼是否存在坐标系、缩放、旋转差异。
4. Mesh 预览是否只需要 Editor 模式可用。
