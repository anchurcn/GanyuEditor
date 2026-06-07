using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;


namespace GanyuEditor.Physics
{
    [CustomEditor(typeof(BoxCollisionShapeComponent)), CanEditMultipleObjects]
    public class BoxCollisionShapeEditor : UnityEditor.Editor
    {
        private BoxBoundsHandle _handle = new BoxBoundsHandle() { axes = PrimitiveBoundsHandle.Axes.All };

        private void OnSceneGUI()
        {
            var component = target as BoxCollisionShapeComponent;

            //if (component.ShowBoundsHandle || component.ShowRotationHandle)
            //{
            //    EditorManagerEditor.CustomEditingCount++;
            //}
            //else
            //{
            //    EditorManagerEditor.CustomEditingCount--;
            //}

            Matrix4x4 trans = Matrix4x4.TRS(
                component.transform.position,
                component.Rotation,
                Vector3.one);
            if (component.ShowBoundsHandle)
            {
                using (new Handles.DrawingScope(trans))
                {
                    _handle.size = component.HalfExtent * 2;
                    _handle.center = component.LocalCenter;
                    EditorGUI.BeginChangeCheck();
                    _handle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(component, $"Edit BoxCollisionShape [{component.name}] bounds");
                        component.HalfExtent = _handle.size / 2;
                        component.LocalCenter = _handle.center;
                    }
                }
            }

            if (component.ShowRotationHandle)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion rot = Handles.RotationHandle(component.Rotation, component.transform.position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(component, $"Rotate BoxCollisionShape [{component.name}]");
                    component.Rotation = rot;
                }
            }
        }

        private void OnDisable()
        {
            var component = target as CollisionShapeComponent;
            component.ShowBoundsHandle = false;
            component.ShowRotationHandle = false;
        }
    }
}