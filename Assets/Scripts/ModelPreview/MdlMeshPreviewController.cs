using System.IO;
using UnityEngine;

namespace GanyuEditor.ModelPreview
{
    /// <summary>
    /// 在现有 MDL 模型根对象下生成 Assimp Mesh 预览。
    /// </summary>
    public static class MdlMeshPreviewController
    {
        public static MdlMeshPreviewRoot Rebuild(string modelPath, Transform modelRoot, bool convertGoldsrcToUnity = true)
        {
            if (modelRoot == null)
            {
                Debug.LogWarning("Cannot build MDL mesh preview: model root is null.");
                return null;
            }

            Clear(modelRoot);

            if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
            {
                Debug.LogWarning($"Cannot build MDL mesh preview: model file does not exist. Path={modelPath}");
                return null;
            }

            if (!AssimpModelImportService.TryImport(modelPath, out var scene, out var error))
            {
                Debug.LogWarning($"Assimp failed to import mesh preview for '{modelPath}': {error}");
                return null;
            }

            var previewObject = new GameObject(MdlMeshPreviewRoot.PreviewRootName);
            previewObject.transform.SetParent(modelRoot, false);

            var previewRoot = previewObject.AddComponent<MdlMeshPreviewRoot>();
            previewRoot.ModelPath = modelPath;
            previewRoot.PreviewMaterial = CreatePreviewMaterial();

            for (int i = 0; i < scene.MeshCount; i++)
            {
                var assimpMesh = scene.Meshes[i];
                var unityMesh = AssimpUnityMeshBuilder.Build(assimpMesh, convertGoldsrcToUnity);

                var meshObject = new GameObject(GetMeshObjectName(assimpMesh.Name, i));
                meshObject.transform.SetParent(previewObject.transform, false);

                var meshFilter = meshObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = unityMesh;

                var meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = previewRoot.PreviewMaterial;

                previewRoot.Renderers.Add(meshRenderer);
            }

            previewRoot.MeshCount = previewRoot.Renderers.Count;
            Debug.Log($"Created Assimp mesh preview for '{Path.GetFileName(modelPath)}'. MeshCount={previewRoot.MeshCount}");
            return previewRoot;
        }

        public static void Clear(Transform modelRoot)
        {
            if (modelRoot == null)
            {
                return;
            }

            for (int i = modelRoot.childCount - 1; i >= 0; i--)
            {
                var child = modelRoot.GetChild(i);
                if (child.name == MdlMeshPreviewRoot.PreviewRootName || child.GetComponent<MdlMeshPreviewRoot>() != null)
                {
                    DestroyPreviewResources(child.gameObject);
                    DestroyObject(child.gameObject);
                }
            }
        }

        private static void DestroyPreviewResources(GameObject previewObject)
        {
            var filters = previewObject.GetComponentsInChildren<MeshFilter>(true);
            foreach (var filter in filters)
            {
                if (filter.sharedMesh != null)
                {
                    DestroyObject(filter.sharedMesh);
                }
            }

            var root = previewObject.GetComponent<MdlMeshPreviewRoot>();
            if (root != null && root.PreviewMaterial != null)
            {
                DestroyObject(root.PreviewMaterial);
            }
        }


        private static Material CreatePreviewMaterial()
        {
            var shader = Shader.Find("Standard") ?? Shader.Find("Diffuse");
            var material = new Material(shader)
            {
                name = "Assimp MDL Mesh Preview Material",
                color = new Color(0.72f, 0.82f, 0.95f, 1f)
            };
            return material;
        }

        private static string GetMeshObjectName(string meshName, int index)
        {
            return string.IsNullOrEmpty(meshName)
                ? $"AssimpMesh_{index}"
                : $"AssimpMesh_{index}_{meshName}";
        }

        private static void DestroyObject(Object obj)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.DestroyImmediate(obj);
            }
        }
    }
}
