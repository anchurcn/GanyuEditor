using UnityEngine;


namespace GanyuEditor.Extensions
{
    public static class Matrix4x4Extensions
    {
        public static Vector3 GetTranslation(ref this Matrix4x4 self)
        {
            return new Vector3(self.m03, self.m13, self.m23);
        }

        public static void SetTranslation(ref this Matrix4x4 self, Vector3 translation)
        {
            self.m03 = translation.x;
            self.m13 = translation.y;
            self.m23 = translation.z;
        }

        public static void Decomposition(this ref Matrix4x4 self, out Vector3 pos, out Quaternion rot, out Vector3 scale)
        {
            var pos4 = self.GetColumn(3);
            pos = new Vector3(pos4.x, pos4.y, pos4.z);
            rot = self.rotation;
            scale = self.lossyScale;
        }

        public static Matrix4x4 ToUnity(this Matrix4x4 self)
        {
            var result = new Matrix4x4();
            result.SetRow(0, new Vector4(self.m00, self.m02, self.m01, self.m03));
            result.SetRow(1, new Vector4(self.m20, self.m22, self.m21, self.m23));
            result.SetRow(2, new Vector4(self.m10, self.m12, self.m11, self.m13));
            result.SetRow(3, new Vector4(self.m30, self.m32, self.m31, self.m33));
            return result;
        }

        public static Matrix4x4 ToGoldsrc(this Matrix4x4 self)
        {
            var result = new Matrix4x4();
            result.SetRow(0, new Vector4(self.m00, self.m02, self.m01, self.m03));
            result.SetRow(1, new Vector4(self.m20, self.m22, self.m21, self.m23));
            result.SetRow(2, new Vector4(self.m10, self.m12, self.m11, self.m13));
            result.SetRow(3, new Vector4(self.m30, self.m32, self.m31, self.m33));
            return result;
        }

        public static void WriteToTransform(this ref Matrix4x4 self, Transform outTransform)
        {
            self.Decomposition(out Vector3 pos, out Quaternion rot, out Vector3 scale);

            outTransform.position = pos;
            outTransform.rotation = rot;
            outTransform.localScale = scale;
        }
        public static void Matrix4x4Lookat(ref Matrix4x4 transform, in Vector3 worldPoint, in Vector3 forward)
        {
            var originVector = forward;
            var worldToLocalTransform = transform.inverse;

            //transform the target in world position to object's local position
            var targetVector = worldToLocalTransform.MultiplyPoint(worldPoint);

            var rot = Quaternion.FromToRotation(originVector, targetVector);
            var rotMatrix = Matrix4x4.Rotate(rot);
            transform = transform * rotMatrix;
        }
        //public static void Lookat(ref this Matrix4x4 self,in Vector3 worldPoint,in Vector3 forward)
        //{
        //    Matrix4x4Lookat(ref self, worldPoint, forward);
        //}
        public static Matrix4x4 Lookat(this Matrix4x4 self, in Vector3 worldPoint, in Vector3 forward)
        {
            Matrix4x4Lookat(ref self, worldPoint, forward);
            return self;
        }

        public static bool MatrixEqual1(in Matrix4x4 lhs, in Matrix4x4 rhs)
        {
            for (int i = 0; i < 16; i++)
            {
                if ((Mathf.Abs(lhs[i]) - Mathf.Abs(rhs[i])) > 1E-5)
                    return false;
            }
            return true;
        }
        public static bool MatrixEqual(in Matrix4x4 lhs, in Matrix4x4 rhs)
        {
            for (int i = 0; i < 16; i++)
            {
                var flt1 = Mathf.Abs(lhs[i]);
                var flt2 = Mathf.Abs(rhs[i]);
                if (flt1 - flt2 > 1E-4)
                    return false;
            }
            return true;
        }
        public static bool Equal(this Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return MatrixEqual(lhs, rhs);
        }
    }
}


