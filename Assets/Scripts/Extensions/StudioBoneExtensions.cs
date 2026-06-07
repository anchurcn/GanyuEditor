using UnityEngine;
using GanyuEditor.Physics;

namespace GanyuEditor.Extensions
{
    public static class StudioBoneExtensions
    {
        public static Vector3 Pos(this StudioBone self) => self.transform.position;

        public static Matrix4x4 Trans(this StudioBone self) => self.transform.localToWorldMatrix;

        public static GameObject RootObject(this GameObject self)
        {
            while (self.transform.parent)
            {
                self = self.transform.parent.gameObject;
            }
            return self;
        }

        public static void AddShape(this StudioBone self, Matrix4x4 shapeTrans, float height, float radius)
        {
            var shape = self.gameObject.AddComponent<CapsuleCollisionShapeComponent>();
            self.gameObject.AddComponent<PhysicsBody>();
            shape.WorldTransform = shapeTrans;
            shape.Height = height;
            shape.Radius = radius;
        }
    }
}
