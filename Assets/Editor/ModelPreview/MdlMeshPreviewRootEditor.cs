using GanyuEditor.ModelPreview;
using UnityEditor;
using UnityEngine;

namespace GanyuEditor.ModelPreview.Editor
{
    [CustomEditor(typeof(MdlMeshPreviewRoot))]
    public class MdlMeshPreviewRootEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var previewRoot = (MdlMeshPreviewRoot)target;
            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(previewRoot.ModelPath)))
            {
                if (GUILayout.Button("Rebuild Assimp Mesh Preview"))
                {
                    MdlMeshPreviewController.Rebuild(previewRoot.ModelPath, previewRoot.transform.parent);
                }
            }

            if (GUILayout.Button("Clear Assimp Mesh Preview"))
            {
                MdlMeshPreviewController.Clear(previewRoot.transform.parent);
            }
        }
    }
}
