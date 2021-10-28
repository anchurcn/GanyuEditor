using System.Collections;
using UnityEditor;
using UnityEngine;


namespace GanyuEditor.Physics
{
    public class BoxShape : CollisionShape
    {
        public Vector3 HalfExtent = Vector3.one;

        public override void GizmosDrawBounds()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, Rotation, Vector3.one);
            Gizmos.DrawWireCube(LocalCenter, HalfExtent * 2);
        }

        private void Awake()
        {
            LocalCenter = Vector3.zero;
            Rotation = transform.rotation;
            HalfExtent = Vector3.one;
        }

        private void OnDrawGizmosSelected()
        {
            if (Selection.activeGameObject.GetInstanceID() == gameObject.GetInstanceID())
            {
                if (!ShowBoundsHandle)
                {
                    Gizmos.color = Color.red;
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, Rotation, Vector3.one);
                    Gizmos.DrawWireCube(LocalCenter, HalfExtent * 2);
                }
            }
        }
    }
}