using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;


namespace GanyuEditor.Physics
{
    [CustomEditor(typeof(CapsuleShape)), CanEditMultipleObjects]
    public class CapsuleShapeEditor : UnityEditor.Editor
    {
        private CapsuleBoundsHandle _handle = new CapsuleBoundsHandle() { axes = PrimitiveBoundsHandle.Axes.All, heightAxis = CapsuleBoundsHandle.HeightAxis.X };

        private void OnSceneGUI()
        {
            var component = target as CapsuleShape;

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
                    _handle.radius = component.Radius;
                    _handle.height = component.Height;
                    _handle.center = component.LocalCenter;
                    EditorGUI.BeginChangeCheck();
                    _handle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(component, $"Edit BoxShape [{component.name}] bounds");
                        component.Radius = _handle.radius;
                        component.Height = _handle.height;
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
                    Undo.RecordObject(component, $"Rotate BoxShape [{component.name}]");
                    component.Rotation = rot;
                }
            }
        }

        private void OnDisable()
        {
            var component = target as CollisionShape;
            component.ShowBoundsHandle = false;
            component.ShowRotationHandle = false;
        }
    }
}