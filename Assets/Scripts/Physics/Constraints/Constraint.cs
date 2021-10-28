using GanyuEditor.Extensions;
using UnityEngine;


namespace GanyuEditor.Physics
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Constraint : MonoBehaviour
    {
        // 约束共享骨骼的位置，但有自己的旋转。
        public Vector3 Position => transform.position;
        public Quaternion Rotation = Quaternion.identity;

        public Rigidbody ConnectedBody;

        public Matrix4x4 WorldTransform
        {
            get => Matrix4x4.TRS(Position, Rotation, Vector3.one);

            set
            {
                Rotation = value.rotation;
            }
        }
        public Matrix4x4 WorldTransformGoldsrc =>
            Matrix4x4.TRS(Position, Rotation, Vector3.one).ToGoldsrc();
    }
}