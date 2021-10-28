using System;
using UnityEngine;
using UnityEditor;

namespace Assets.Editor.GPhys
{
    [InitializeOnLoad]
    public class EditorManagerEditor
    {
        private static int _editingCount;
        public static int CustomEditingCount
        {
            get => _editingCount;
            set
            {
                _editingCount = value;

            }
        }
        public static void StartEdit()
        {

        }
        public static void EndEdit()
        {

        }
        public EditorManagerEditor()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            Debug.Log($"{nameof(EditorManagerEditor)} loaded.");
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (CustomEditingCount > 0)
            {
                Tools.hidden = true;
            }
            else
            {
                Tools.hidden = false;
            }
        }
    }
}
