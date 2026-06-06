using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;


namespace GanyuEditor.Physics
{
    [AddComponentMenu("Goldsrc/AddHingeConstraint")]
    [CustomEditor(typeof(HingeConstraint)), CanEditMultipleObjects]
    public class HingeConstraintEditor : UnityEditor.Editor
    {
        // TODO: Y Limit 的 handle 起点应该为X轴，但Unity的实现为Z轴。可换成自己实现的 handle。
        private JointAngularLimitHandle _handle = new JointAngularLimitHandle()
        {
            xHandleColor = new Color(1, 1, 1, 0),
            zHandleColor = new Color(1, 1, 1, 0),
            xRange = Vector2.zero,
            zRange = Vector2.zero,
        };

        private void OnSceneGUI()
        {
            var component = target as HingeConstraint;

            //if (component.ShowLimitHandles || component.ShowRotationHandle)
            //{
            //    EditorManagerEditor.CustomEditingCount++;
            //}
            //else
            //{
            //    EditorManagerEditor.CustomEditingCount--;
            //}
            
            if (component.ShowLimitHandles)
            {
                // copy to handle
                _handle.yMin = -component.High;
                _handle.yMax = -component.Low;

                Matrix4x4 trans = Matrix4x4.TRS(
                    component.Position,
                    component.Rotation * Quaternion.Euler(0,90,0),
                    Vector3.one);
                using (new Handles.DrawingScope(trans))
                {
                    _handle.radius = HandleUtility.GetHandleSize(Vector3.zero);
                    EditorGUI.BeginChangeCheck();
                    _handle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(component, $"Edit {nameof(HingeConstraint)} [{component.name}] limits");

                        component.Low = -_handle.yMax;
                        component.High = -_handle.yMin;
                    }
                }
            }

            if (component.ShowRotationHandle)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion rot = Handles.RotationHandle(component.Rotation, component.Position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(component, $"Rotate {nameof(HingeConstraint)}");
                    component.Rotation = rot;
                }
            }

            var size = HandleUtility.GetHandleSize(component.Position);
            if (Event.current.type == EventType.Repaint)
            {
                Handles.color = new Color(1, 165f / 255, 0);
                Handles.ArrowHandleCap(
               0,
               component.Position,
               component.Rotation * Quaternion.LookRotation(Vector3.up),
               size * 1.35f,
               EventType.Repaint);

                Handles.color = Color.white;
                Handles.ArrowHandleCap(
               0,
               component.Position,
               component.Rotation * Quaternion.LookRotation(Vector3.right),
               size * 1.35f,
               EventType.Repaint);
            }
        }
    }
}
