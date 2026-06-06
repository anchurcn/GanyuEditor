using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using GanyuEditor.Extensions;
using static GoldsrcPhysics.Goldsrc.Studio_h;


namespace GanyuEditor
{
    public unsafe class ImportStudioBoneWizard : ScriptableWizard
    {

        public string ModelPath;
        //    public string GanyuPath = @"F:\_Goldsrc\Models\mdl\ganyu\GI_Ganyu.mdl";

        private GameObject ModelRoot;
        private studiohdr_t* m_pstudiohdr;
        void OnWizardCreate()
        {
            Start();
        }

        private void Start()
        {
            var bytes = File.ReadAllBytes(ModelPath);
            fixed (byte* p = bytes)
            {
                studiohdr_t* pStudioModel = (studiohdr_t*)p;
                m_pstudiohdr = pStudioModel;
                if (Validation(pStudioModel))
                {
                    Debug.Log("Loading...");
                    ModelRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    var info = ModelRoot.AddComponent<ModelInfo>();
                    info.ModelPath = ModelPath;
                    info.Checksum = CalcHdrChecksum(bytes);
                    ModelRoot.name = info.ModelName;

                    CreateSkeleton3(pStudioModel);
                    CreateSkinedMesh();
                    Debug.Log($"Create skeleton for {ModelRoot.name} successfully.");
                }
                else
                {
                    Debug.LogError($"ERROR: {ModelPath} isn't is a valid goldsrc model file.");
                }
            }
        }
        private string CalcHdrChecksum(byte[] modeldata)
        {
            var sha1algo = System.Security.Cryptography.SHA1.Create();
            var sha1 = sha1algo.ComputeHash(modeldata, 0, sizeof(studiohdr_t));
            return sha1.Aggregate("", (a, next) => a + next.ToString("X2"));
        }
        private void CreateSkinedMesh()
        {
            for (int i = 0; i < m_pstudiohdr->numbodyparts; i++)
            {
                var model = SetupModel(i);
                CreateModelMeshes(model);
            }
        }

        private void CreateModelMeshes(mstudiomodel_t* model)
        {

        }

        private mstudiomodel_t* SetupModel(int bodypart)
        {
            int index;

            if (bodypart > m_pstudiohdr->numbodyparts)
            {
                // Con_DPrintf ("StudioModel::SetupModel: no such bodypart %d\n", bodypart);
                bodypart = 0;
            }

            mstudiobodyparts_t* pbodypart = (mstudiobodyparts_t*)((byte*)m_pstudiohdr + m_pstudiohdr->bodypartindex) + bodypart;

            int m_bodynum = 0;// *TODO:
            index = m_bodynum / pbodypart->@base;
            index = index % pbodypart->nummodels;

            return (mstudiomodel_t*)((byte*)m_pstudiohdr + pbodypart->modelindex) + index;
        }
        public static Matrix4x4 GetGoldsrcBoneLocalMatrix(float* bonevalue)
        {
            Quaternion q = new Quaternion();
            Matrix4x4 boneMatrix = new Matrix4x4();
            // rotation
            MyUtil.AngleQuaternion(new Vector3(bonevalue[3], bonevalue[4], bonevalue[5]), out q);
            MyUtil.QuaternionMatrix(q, out boneMatrix);
            // translation
            boneMatrix[0, 3] = bonevalue[0];
            boneMatrix[1, 3] = bonevalue[1];
            boneMatrix[2, 3] = bonevalue[2];
            return boneMatrix;
        }
        public static Vector3 GetTranslation(Matrix4x4 m)
        {
            return new Vector3(m.m03, m.m13, m.m23);
        }
        public static void SetTranslation(ref Matrix4x4 m, Vector3 translation)
        {
            m.m03 = translation.x;
            m.m13 = translation.y;
            m.m23 = translation.z;
        }


        Matrix4x4[] _boneTransform = new Matrix4x4[128];
        void FillBoneTransform(studiohdr_t* phdr)
        {
            mstudiobone_t* pbones = (mstudiobone_t*)((byte*)phdr + phdr->boneindex);
            for (int i = 0; i < phdr->numbones; i++)
            {
                if (pbones[i].parent == -1)
                {
                    _boneTransform[i] = GetGoldsrcBoneLocalMatrix(pbones[i].value);
                }
                else
                {
                    _boneTransform[i] = _boneTransform[pbones[i].parent] *
                        GetGoldsrcBoneLocalMatrix(pbones[i].value);
                }
            }
        }
        private void CreateSkeleton3(studiohdr_t* pStudioModel)
        {
            GameObject[] gameObjects = new GameObject[128];
            mstudiobone_t* pbones = (mstudiobone_t*)((byte*)pStudioModel + pStudioModel->boneindex);
            FillBoneTransform(pStudioModel);
            for (int i = 0; i < pStudioModel->numbones; i++)
            {
                if (pbones[i].parent == -1)
                {
                    var gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var boneComponent = gameObject.AddComponent<StudioBone>();
                    string name = Marshal.PtrToStringAnsi((System.IntPtr)pbones[i].name);
                    gameObject.name = name;
                    gameObject.transform.parent = ModelRoot.transform;
                    boneComponent.Index = i;
                    var bonevalue = pbones[i].value;

                    Matrix4x4 boneMatrix = GetGoldsrcBoneLocalMatrix(bonevalue);

                    var boneWorldMatrix = (ModelRoot.transform.localToWorldMatrix.ToGoldsrc() * boneMatrix).ToUnity();

                    boneWorldMatrix.WriteToTransform(gameObject.transform);

                    gameObjects[i] = gameObject;
                }
                else
                {
                    var parentObject = gameObjects[pbones[i].parent];
                    var gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    var boneComponent = gameObject.AddComponent<StudioBone>();
                    string name = Marshal.PtrToStringAnsi((System.IntPtr)pbones[i].name);
                    gameObject.name = name;
                    gameObject.transform.parent = parentObject.transform;
                    boneComponent.Index = i;
                    var bonevalue = pbones[i].value;

                    Matrix4x4 boneMatrix = GetGoldsrcBoneLocalMatrix(bonevalue);

                    var boneWorldMatrix = (parentObject.transform.localToWorldMatrix.ToGoldsrc() * boneMatrix).ToUnity();

                    boneWorldMatrix.WriteToTransform(gameObject.transform);

                    gameObjects[i] = gameObject;
                }
            }

            // assertion
            for (int i = 0; i < pStudioModel->numbones; i++)
            {
                Matrix4x4 local1, local2, local3;
                if (pbones[i].parent == -1)
                {
                    local1 = gameObjects[i].transform.localToWorldMatrix.ToGoldsrc();
                    local2 = gameObjects[i].transform.localToWorldMatrix.ToGoldsrc();
                    local3 = _boneTransform[i];
                }
                else
                {
                    local1 = (gameObjects[pbones[i].parent].transform.localToWorldMatrix.inverse *
                        gameObjects[i].transform.localToWorldMatrix).ToGoldsrc();
                    local2 = gameObjects[pbones[i].parent].transform.localToWorldMatrix.ToGoldsrc().inverse *
                        gameObjects[i].transform.localToWorldMatrix.ToGoldsrc();
                    local3 = _boneTransform[pbones[i].parent].inverse * _boneTransform[i];
                }
                Debug.Assert(local1.Equal(local2));
                Debug.Assert(local2.Equal(local3));
                Debug.Assert(local1.Equal(local3));
            }
        }
        bool MatrixEqual(in Matrix4x4 lhs, in Matrix4x4 rhs)
        {
            for (int i = 0; i < 16; i++)
            {
                if ((Mathf.Abs(lhs[i]) - Mathf.Abs(rhs[i])) > 1E-6)
                    return false;
            }
            return true;
        }
        private void OnEnable()
        {

        }
        static GameObject _tempObject;


        private bool Validation(studiohdr_t* pStudioModel)
        {
            byte* p = (byte*)pStudioModel;
            return p[0] == (byte)'I' && p[1] == (byte)'D' && p[2] == (byte)'S' && p[3] == (byte)'T' && pStudioModel->version == 10;
        }

        void OnWizardOtherButton()
        {
            Debug.Log("OnWizardOtherButton");
        }
        private unsafe void LoadMdlBones2()
        {
            var bytes = File.ReadAllBytes(ModelPath);
            (GameObject gameObject, Matrix4x4 worldMatrix)[] unityBones = new (GameObject, Matrix4x4)[128];
            fixed (byte* p = bytes)
            {
                studiohdr_t* pStudioModel = (studiohdr_t*)p;
                mstudiobone_t* bones = (mstudiobone_t*)((byte*)pStudioModel + pStudioModel->boneindex);
                float[][] val = new float[pStudioModel->numbones][];
                for (int i = 0; i < pStudioModel->numbones; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        val[i] = new float[6];
                        val[i][j] = bones[i].value[j];
                    }
                    if (i == 0)
                    {

                        string name = Marshal.PtrToStringAnsi((System.IntPtr)bones[i].name);
                        unityBones[i].gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        unityBones[i].gameObject.name = name;
                        var bonevalue = bones[i].value;

                        var boneMatrix = MakeMatrix(new Vector3(bonevalue[0], bonevalue[1], bonevalue[2]),
                            new Vector3(bonevalue[3], bonevalue[4], bonevalue[5]));

                        var urootMatrix = boneMatrix;
                        unityBones[i].worldMatrix = urootMatrix;

                    }
                    else
                    {
                        string name = Marshal.PtrToStringAnsi((System.IntPtr)bones[i].name);
                        unityBones[i].gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        unityBones[i].gameObject.name = name;

                        var bonevalue = bones[i].value;

                        var boneLocalMatrix = MakeMatrix(new Vector3(bonevalue[0], bonevalue[1], bonevalue[2]),
                            new Vector3(bonevalue[3], bonevalue[4], bonevalue[5]));

                        var boneWorldMatrix = unityBones[bones[i].parent].worldMatrix * boneLocalMatrix;
                        unityBones[i].worldMatrix = boneWorldMatrix;
                    }
                }
                for (int i = 0; i < pStudioModel->numbones; i++)
                {
                    Matrix4x4 rebaseTransform = new Matrix4x4(
                            new Vector4(1, 0, 0, 0),
                            new Vector4(0, 0, 1, 0),
                            new Vector4(0, 1, 0, 0),
                            new Vector4(0, 0, 0, 1));

                    var unityMatrix = rebaseTransform * unityBones[i].worldMatrix;
                    MyUtil.SetTransformFromMatrix(unityBones[i].gameObject.transform, ref unityMatrix);
                }
            }
        }
        private unsafe void LoadMdlBones()
        {
            var bytes = File.ReadAllBytes(ModelPath);
            (GameObject, Matrix4x4)[] unityBones = new (GameObject, Matrix4x4)[128];
            fixed (byte* p = bytes)
            {
                studiohdr_t* pStudioModel = (studiohdr_t*)p;
                mstudiobone_t* bones = (mstudiobone_t*)((byte*)pStudioModel + pStudioModel->boneindex);
                for (int i = 0; i < pStudioModel->numbones; i++)
                {
                    if (i == 0)
                    {
                        Matrix4x4 rebaseTransform = new Matrix4x4(
                            new Vector4(1, 0, 0, 0),
                            new Vector4(0, 0, 1, 0),
                            new Vector4(0, 1, 0, 0),
                            new Vector4(0, 0, 0, 1));
                        string name = Marshal.PtrToStringAnsi((System.IntPtr)bones[i].name);
                        unityBones[i].Item1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        unityBones[i].Item1.name = name;
                        var bonevalue = bones[i].value;

                        //var boneMatrix = MakeMatrix(new Vector3(bonevalue[0], bonevalue[1], bonevalue[2]),
                        //    new Vector3(bonevalue[3], bonevalue[4], bonevalue[5]));
                        Quaternion q = new Quaternion();
                        Matrix4x4 boneMatrix = new Matrix4x4();
                        MyUtil.AngleQuaternion(new Vector3(bonevalue[3], bonevalue[4], bonevalue[5]), out q);
                        MyUtil.QuaternionMatrix(q, out boneMatrix);
                        boneMatrix[0, 3] = bonevalue[0];
                        boneMatrix[1, 3] = bonevalue[1];
                        boneMatrix[2, 3] = bonevalue[2];

                        var urootMatrix = rebaseTransform * boneMatrix;
                        unityBones[i].Item2 = urootMatrix;
                        MyUtil.SetTransformFromMatrix(unityBones[i].Item1.transform, ref urootMatrix);

                    }
                    else
                    {
                        string name = Marshal.PtrToStringAnsi((System.IntPtr)bones[i].name);
                        unityBones[i].Item1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        unityBones[i].Item1.name = name;

                        var bonevalue = bones[i].value;

                        Quaternion q = new Quaternion();
                        Matrix4x4 boneLocalMatrix = new Matrix4x4();
                        MyUtil.AngleQuaternion(new Vector3(bonevalue[3], bonevalue[4], bonevalue[5]), out q);
                        MyUtil.QuaternionMatrix(q, out boneLocalMatrix);
                        boneLocalMatrix[0, 3] = bonevalue[0];
                        boneLocalMatrix[1, 3] = bonevalue[1];
                        boneLocalMatrix[2, 3] = bonevalue[2];

                        var boneWorldMatrix = unityBones[bones[i].parent].Item1.transform.localToWorldMatrix * boneLocalMatrix;
                        unityBones[i].Item2 = boneWorldMatrix;
                        MyUtil.SetTransformFromMatrix(unityBones[i].Item1.transform, ref boneWorldMatrix);
                    }
                }
            }
        }

        public Matrix4x4 MakeMatrix(Vector3 origin, Vector3 angle)
        {
            var m = AngleIMatrix(angle);
            m[0, 3] = origin.x;
            m[1, 3] = origin.y;
            m[2, 3] = origin.z;
            return m;

        }
        public static Matrix4x4 AngleIMatrix(Vector3 angles)
        {
            Matrix4x4 matrix = Matrix4x4.identity;
            int YAW = 1;
            int PITCH = 0;
            int ROLL = 2;
            float angle;
            float sr, sp, sy, cr, cp, cy;

            angle = angles[YAW] * (Mathf.PI * 2 / 360);
            sy = Mathf.Sin(angle);
            cy = Mathf.Cos(angle);
            angle = angles[PITCH] * (Mathf.PI * 2 / 360);
            sp = Mathf.Sin(angle);
            cp = Mathf.Cos(angle);
            angle = angles[ROLL] * (Mathf.PI * 2 / 360);
            sr = Mathf.Sin(angle);
            cr = Mathf.Cos(angle);

            // matrix = (YAW * PITCH) * ROLL
            matrix[0, 0] = cp * cy;
            matrix[0, 1] = cp * sy;
            matrix[0, 2] = -sp;
            matrix[1, 0] = sr * sp * cy + cr * -sy;
            matrix[1, 1] = sr * sp * sy + cr * cy;
            matrix[1, 2] = sr * cp;
            matrix[2, 0] = (cr * sp * cy + -sr * -sy);
            matrix[2, 1] = (cr * sp * sy + -sr * cy);
            matrix[2, 2] = cr * cp;
            matrix[0, 3] = 0.0f;
            matrix[1, 3] = 0.0f;
            matrix[2, 3] = 0.0f;
            matrix[3, 3] = 1;
            return matrix;
        }
    }
}


public static class MyUtil
{
    public static void AngleQuaternion(Vector3 angles, out Quaternion quaternion)
    {
        quaternion = new Quaternion();
        float angle;
        float sr, sp, sy, cr, cp, cy;

        // FIXME: rescale the inputs to 1/2 angle
        angle = angles[2] * 0.5f;
        sy = (float)Mathf.Sin(angle);
        cy = (float)Mathf.Cos(angle);
        angle = angles[1] * 0.5f;
        sp = (float)Mathf.Sin(angle);
        cp = (float)Mathf.Cos(angle);
        angle = angles[0] * 0.5f;
        sr = (float)Mathf.Sin(angle);
        cr = (float)Mathf.Cos(angle);

        quaternion[0] = sr * cp * cy - cr * sp * sy; // X
        quaternion[1] = cr * sp * cy + sr * cp * sy; // Y
        quaternion[2] = cr * cp * sy - sr * sp * cy; // Z
        quaternion[3] = cr * cp * cy + sr * sp * sy; // W
    }
    public static void QuaternionMatrix(Quaternion quaternion, out Matrix4x4 result)
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix[0 * 4 + 0] = (float)(1.0 - 2.0 * quaternion[1] * quaternion[1] - 2.0 * quaternion[2] * quaternion[2]);
        matrix[1 * 4 + 0] = (float)(2.0 * quaternion[0] * quaternion[1] + 2.0 * quaternion[3] * quaternion[2]);
        matrix[2 * 4 + 0] = (float)(2.0 * quaternion[0] * quaternion[2] - 2.0 * quaternion[3] * quaternion[1]);

        matrix[0 * 4 + 1] = (float)(2.0 * quaternion[0] * quaternion[1] - 2.0 * quaternion[3] * quaternion[2]);
        matrix[1 * 4 + 1] = (float)(1.0 - 2.0 * quaternion[0] * quaternion[0] - 2.0 * quaternion[2] * quaternion[2]);
        matrix[2 * 4 + 1] = (float)(2.0 * quaternion[1] * quaternion[2] + 2.0 * quaternion[3] * quaternion[0]);

        matrix[0 * 4 + 2] = (float)(2.0 * quaternion[0] * quaternion[2] + 2.0 * quaternion[3] * quaternion[1]);
        matrix[1 * 4 + 2] = (float)(2.0 * quaternion[1] * quaternion[2] - 2.0 * quaternion[3] * quaternion[0]);
        matrix[2 * 4 + 2] = (float)(1.0 - 2.0 * quaternion[0] * quaternion[0] - 2.0 * quaternion[1] * quaternion[1]);

        matrix.m33 = 1;
        result = matrix.transpose;
    }

    /// <summary>
    /// Extract translation from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Translation offset.
    /// </returns>
    public static Vector3 ExtractTranslationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 translate;
        translate.x = matrix.m03;
        translate.y = matrix.m13;
        translate.z = matrix.m23;
        return translate;
    }

    /// <summary>
    /// Extract rotation quaternion from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Quaternion representation of rotation transform.
    /// </returns>
    public static Quaternion ExtractRotationFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    /// <summary>
    /// Extract scale from transform matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <returns>
    /// Scale vector.
    /// </returns>
    public static Vector3 ExtractScaleFromMatrix(ref Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }

    /// <summary>
    /// Extract position, rotation and scale from TRS matrix.
    /// </summary>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    /// <param name="localPosition">Output position.</param>
    /// <param name="localRotation">Output rotation.</param>
    /// <param name="localScale">Output scale.</param>
    public static void DecomposeMatrix(ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
    {
        localPosition = ExtractTranslationFromMatrix(ref matrix);
        localRotation = ExtractRotationFromMatrix(ref matrix);
        localScale = ExtractScaleFromMatrix(ref matrix);
    }

    /// <summary>
    /// Set transform component from TRS matrix.
    /// </summary>
    /// <param name="transform">Transform component.</param>
    /// <param name="matrix">Transform matrix. This parameter is passed by reference
    /// to improve performance; no changes will be made to it.</param>
    public static void SetTransformFromMatrix(Transform transform, ref Matrix4x4 matrix)
    {
        transform.position = ExtractTranslationFromMatrix(ref matrix);
        transform.rotation = ExtractRotationFromMatrix(ref matrix);
        transform.localScale = ExtractScaleFromMatrix(ref matrix);
    }
    public static Quaternion MatrixToRotation(Matrix4x4 m)
    {
        // Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }

    public static Quaternion MatrixToQuaternion(Matrix4x4 m)
    {
        float tr = m.m00 + m.m11 + m.m22;
        float w, x, y, z;
        if (tr > 0f)
        {
            float s = Mathf.Sqrt(1f + tr) * 2f;
            w = 0.25f * s;
            x = (m.m21 - m.m12) / s;
            y = (m.m02 - m.m20) / s;
            z = (m.m10 - m.m01) / s;
        }
        else if ((m.m00 > m.m11) && (m.m00 > m.m22))
        {
            float s = Mathf.Sqrt(1f + m.m00 - m.m11 - m.m22) * 2f;
            w = (m.m21 - m.m12) / s;
            x = 0.25f * s;
            y = (m.m01 + m.m10) / s;
            z = (m.m02 + m.m20) / s;
        }
        else if (m.m11 > m.m22)
        {
            float s = Mathf.Sqrt(1f + m.m11 - m.m00 - m.m22) * 2f;
            w = (m.m02 - m.m20) / s;
            x = (m.m01 + m.m10) / s;
            y = 0.25f * s;
            z = (m.m12 + m.m21) / s;
        }
        else
        {
            float s = Mathf.Sqrt(1f + m.m22 - m.m00 - m.m11) * 2f;
            w = (m.m10 - m.m01) / s;
            x = (m.m02 + m.m20) / s;
            y = (m.m12 + m.m21) / s;
            z = 0.25f * s;
        }

        Quaternion quat = new Quaternion(x, y, z, w);
        //Debug.Log("Quat is " + quat.ToString() );
        return quat;
    }
    // EXTRAS!

    /// <summary>
    /// Identity quaternion.
    /// </summary>
    /// <remarks>
    /// <para>It is faster to access this variation than <c>Quaternion.identity</c>.</para>
    /// </remarks>
    public static readonly Quaternion IdentityQuaternion = Quaternion.identity;
    /// <summary>
    /// Identity matrix.
    /// </summary>
    /// <remarks>
    /// <para>It is faster to access this variation than <c>Matrix4x4.identity</c>.</para>
    /// </remarks>
    public static readonly Matrix4x4 IdentityMatrix = Matrix4x4.identity;

    /// <summary>
    /// Get translation matrix.
    /// </summary>
    /// <param name="offset">Translation offset.</param>
    /// <returns>
    /// The translation transform matrix.
    /// </returns>
    public static Matrix4x4 TranslationMatrix(Vector3 offset)
    {
        Matrix4x4 matrix = IdentityMatrix;
        matrix.m03 = offset.x;
        matrix.m13 = offset.y;
        matrix.m23 = offset.z;
        return matrix;
    }
}