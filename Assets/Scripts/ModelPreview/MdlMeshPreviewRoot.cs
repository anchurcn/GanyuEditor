using System.Collections.Generic;
using UnityEngine;

namespace GanyuEditor.ModelPreview
{
    /// <summary>
    /// 标记 Assimp 生成的 MDL Mesh 预览根节点，便于重新导入时清理旧预览。
    /// </summary>
    public class MdlMeshPreviewRoot : MonoBehaviour
    {
        public const string PreviewRootName = "AssimpMeshPreview";

        public string ModelPath;
        public int MeshCount;
        public Material PreviewMaterial;
        public List<MeshRenderer> Renderers = new List<MeshRenderer>();
    }
}
