using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;


namespace GanyuEditor.Physics
{
    [CustomEditor(typeof(ConeTwistConstraint)), CanEditMultipleObjects]
    public class ConeTwistConstraintEditor : UnityEditor.Editor
    {
        private readonly JointAngularLimitHandle _handle = new JointAngularLimitHandle()
        {
            zHandleColor = Color.red,
            xHandleColor = new Color(35 / 255f, 35 / 255f, 1),
            yHandleColor = new Color(35 / 255f, 1, 35 / 255f)
        };

        private void OnSceneGUI()
        {
            var component = target as ConeTwistConstraint;

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
                _handle.zMin = -component.TwistSpan;
                _handle.zMax = component.TwistSpan;

                _handle.xMin = -component.SwingSpan1;
                _handle.xMax = component.SwingSpan1;

                _handle.yMin = -component.SwingSpan2;
                _handle.yMax = component.SwingSpan2;

                Matrix4x4 trans = Matrix4x4.TRS(
                    component.Position,
                    component.Rotation*Quaternion.Euler(0,90,90),
                    Vector3.one);
                using (new Handles.DrawingScope(trans))
                {
                    _handle.radius = HandleUtility.GetHandleSize(Vector3.zero);
                    EditorGUI.BeginChangeCheck();
                    _handle.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(component, $"Edit ConeTwistConstraint [{component.name}] limits");

                        // copy the handle's updated data back to the target object
                        // 因为是对称的，如果最大值没变那就应该是改变了最小值，使用其中一个改变了的。
                        component.TwistSpan = _handle.zMax == component.TwistSpan ? -_handle.zMin : _handle.zMax;

                        component.SwingSpan1 = _handle.xMax == component.SwingSpan1 ? -_handle.xMin : _handle.xMax;

                        component.SwingSpan2 = _handle.yMax == component.SwingSpan2 ? -_handle.yMin : _handle.yMax;
                    }
                    Handles.color = Color.red;
                    Handles.DrawLine(Vector3.zero, Vector3.right * 10);
                    Handles.color = Color.blue;
                    Handles.DrawLine(Vector3.zero, Vector3.forward * 10);
                }
            }

            if (component.ShowRotationHandle)
            {
                EditorGUI.BeginChangeCheck();
                Quaternion rot = Handles.RotationHandle(component.Rotation, component.Position);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(component, "Rotate ConeTwistConstraint");
                    component.Rotation = rot;
                }
            }
            var size = HandleUtility.GetHandleSize(component.Position);
            if(Event.current.type == EventType.Repaint)
            {
                Handles.color = new Color(1,165f/255,0);
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