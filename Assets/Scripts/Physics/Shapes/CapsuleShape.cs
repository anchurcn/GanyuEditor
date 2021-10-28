using System.Collections;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using static UnityEditor.IMGUI.Controls.CapsuleBoundsHandle;


namespace GanyuEditor.Physics
{
    public class CapsuleShape : CollisionShape
    {
        public float Radius = 1;
        public float Height = 2;

        public override void GizmosDrawBounds()
        {
            Gizmos.matrix = Matrix4x4.TRS(
                transform.position,
                Rotation,
                Vector3.one);

            float cylinderH = Height - 2 * Radius;
            Gizmos.DrawWireSphere(LocalCenter + new Vector3(cylinderH / 2, 0, 0), Radius);
            Gizmos.DrawWireSphere(LocalCenter + new Vector3(-(cylinderH / 2), 0, 0), Radius);
            float width = Mathf.Sin(Mathf.PI / 4) * Radius * 2;
            Gizmos.matrix *= (Matrix4x4.Translate(LocalCenter) * Matrix4x4.Rotate(Quaternion.Euler(45, 0, 0)));
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(cylinderH, width, width));
        }
        #region DrawCapsule

        //private static readonly Vector3[] s_HeightAxes = new Vector3[3]
        //{
        //    Vector3.right,
        //    Vector3.up,
        //    Vector3.forward
        //};
        //private static readonly int[] s_NextAxis = new int[3]
        //{
        //    1,
        //    2,
        //    0
        //};

        //private int m_HeightAxis = 1;
        //private void DrawCapsule()
        //{
        //    var heightAxis = HeightAxis.X;
        //    HeightAxis vector3Axis = HeightAxis.Y;
        //    HeightAxis vector3Axis2 = HeightAxis.Z;
        //    switch (heightAxis)
        //    {
        //        case HeightAxis.Y:
        //            vector3Axis = HeightAxis.Z;
        //            vector3Axis2 = HeightAxis.X;
        //            break;
        //        case HeightAxis.Z:
        //            vector3Axis = HeightAxis.X;
        //            vector3Axis2 = HeightAxis.Y;
        //            break;
        //    }

        //    bool flag = true;//sAxisEnabled((int)heightAxis);
        //    bool flag2 = true;//IsAxisEnabled((int)vector3Axis);
        //    bool flag3 = true;// IsAxisEnabled((int)vector3Axis2);
        //    Vector3 vector = s_HeightAxes[m_HeightAxis];
        //    Vector3 vector2 = s_HeightAxes[s_NextAxis[m_HeightAxis]];
        //    Vector3 vector3 = s_HeightAxes[s_NextAxis[s_NextAxis[m_HeightAxis]]];
        //    float radius = Radius;
        //    float height = Height;
        //    var center = LocalCenter;
        //    Vector3 vector4 = center + vector * (height * 0.5f - radius);
        //    Vector3 vector5 = center - vector * (height * 0.5f - radius);
        //    if (flag)
        //    {
        //        if (flag3)
        //        {
        //            //Handles.DrawWireArc(vector4, vector2, vector3, 180f, radius);
        //            //Handles.DrawWireArc(vector5, vector2, vector3, -180f, radius);
        //            Gizmos.DrawLine(vector4 + vector3 * radius, vector5 + vector3 * radius);
        //            Gizmos.DrawLine(vector4 - vector3 * radius, vector5 - vector3 * radius);
        //        }

        //        if (flag2)
        //        {
        //            //Handles.DrawWireArc(vector4, vector3, vector2, -180f, radius);
        //            //Handles.DrawWireArc(vector5, vector3, vector2, 180f, radius);
        //            Gizmos.DrawLine(vector4 + vector2 * radius, vector5 + vector2 * radius);
        //            Gizmos.DrawLine(vector4 - vector2 * radius, vector5 - vector2 * radius);
        //        }
        //    }

        //    if (flag2 && flag3)
        //    {
        //        //Handles.DrawWireArc(vector4, vector, vector2, 360f, radius);
        //        //Handles.DrawWireArc(vector5, vector, vector2, -360f, radius);
        //    }
        //}

        #endregion
        private void Awake()
        {
            Rotation = transform.rotation;
        }


        private void OnDrawGizmosSelected()
        {
            if (Selection.activeGameObject.GetInstanceID() == gameObject.GetInstanceID())
            {
                if (!ShowBoundsHandle)
                {
                    Gizmos.color = Color.red;
                    Gizmos.matrix = Matrix4x4.TRS(
                    transform.position,
                    Rotation,
                    Vector3.one);
                    float cylinderH = Height - 2 * Radius;
                    Gizmos.DrawWireSphere(LocalCenter + new Vector3(cylinderH / 2, 0, 0), Radius);
                    Gizmos.DrawWireSphere(LocalCenter + new Vector3(-(cylinderH / 2), 0, 0), Radius);
                    float width = Mathf.Sin(Mathf.PI / 4) * Radius * 2;
                    Gizmos.matrix *= (Matrix4x4.Translate(LocalCenter) * Matrix4x4.Rotate(Quaternion.Euler(45, 0, 0)));
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(cylinderH, width, width));
                }
            }
        }

    }
}