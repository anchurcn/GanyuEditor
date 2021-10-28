using GanyuEditor.Extensions;
using UnityEditor;
using UnityEngine;


namespace GanyuEditor
{
    public class StudioBone : MonoBehaviour
    {

        public int Index { get; set; } = -1;
        public Matrix4x4 WorldTransform => gameObject.transform.localToWorldMatrix;

        public Matrix4x4 LocalTransformGoldsrc =>
            (transform.parent.localToWorldMatrix.inverse * WorldTransform).ToGoldsrc();

        void OnDrawGizmosSelected()
        {
            if (Selection.activeGameObject?.GetInstanceID() == gameObject.GetInstanceID())
            {
                if (transform.parent != null)
                {
                    Gizmos.color = Color.yellow;
                    DrawBone(transform.parent.position, transform.position);
                }
                foreach (var i in RootObject(gameObject).GetComponentsInChildren<StudioBone>())
                {
                    if (i.transform.parent.gameObject.GetInstanceID() == gameObject.GetInstanceID())
                    {
                        Gizmos.color = Color.blue;
                        DrawBone(transform.position, i.transform.position);
                    }
                }
            }
        }
        static void DrawBone(in Vector3 parent, in Vector3 child)
        {
            Gizmos.DrawWireSphere(parent, 0.5f);
            var save = Gizmos.matrix;

            float h = (parent - child).magnitude;
            var a = new Vector3(-0.5f, 0, -0.5f);
            var b = new Vector3(0.5f, 0, -0.5f);
            var c = new Vector3(0.5f, 0, 0.5f);
            var d = new Vector3(-0.5f, 0, 0.5f);
            var tip = new Vector3(0, h, 0);

            Gizmos.matrix = Matrix4x4.Translate(parent).Lookat(child, Vector3.up);

            Gizmos.DrawLine(a, tip);
            Gizmos.DrawLine(b, tip);
            Gizmos.DrawLine(c, tip);
            Gizmos.DrawLine(d, tip);

            Gizmos.matrix = save;
        }
        public static GameObject RootObject(GameObject self)
        {
            while (self.transform.parent)
            {
                self = self.transform.parent.gameObject;
            }
            return self;
        }
        void OnDrawGizmos()
        {
            if (transform.parent != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, transform.parent.position);

            }
        }
    }

}
