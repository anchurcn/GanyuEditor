using UnityEditor;
using UnityEngine;
using GanyuEditor.Extensions;


namespace GanyuEditor.Physics
{
    [ExecuteInEditMode]
    public abstract class CollisionShape : MonoBehaviour
    {
        // shape is local to rigidbody
        public Vector3 LocalCenter = Vector3.zero;
        // rotation is in world space
        public Quaternion Rotation = Quaternion.identity;

        public bool ShowBoundsHandle;
        public bool ShowRotationHandle;

        public Matrix4x4 WorldTransform
        {
            get => Matrix4x4.TRS(transform.position, Rotation, Vector3.one) * Matrix4x4.Translate(LocalCenter);

            set
            {
                Rotation = value.rotation;
                LocalCenter = Matrix4x4.TRS(transform.position, Rotation, Vector3.one).inverse.MultiplyPoint(value.GetTranslation());
            }
        }
        public Matrix4x4 WorldTransformGoldsrc => Matrix4x4.TRS(
            transform.localToWorldMatrix.MultiplyPoint(LocalCenter),
            Rotation,
            Vector3.one).ToGoldsrc();

        public Matrix4x4 LocalTransform { get; set; }

        public Color GizmosColor
        {
            get
            {
                var rigid = GetComponent<Rigidbody>();
                if (rigid && rigid.IsAttachment)
                    return Color.blue;
                else
                    return Color.white;
            }
        }
        public abstract void GizmosDrawBounds();

        private void OnDrawGizmos()
        {
            Gizmos.color = GizmosColor;
            GizmosDrawBounds();
        }

        public bool IsBoundsEditing => (Selection.activeGameObject &&
                    Selection.activeGameObject.GetInstanceID() == GetInstanceID() &&
                    ShowBoundsHandle);
    }
}
