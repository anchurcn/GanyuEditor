using System.Collections.Generic;
using Assimp;
using UnityEngine;
using UnityEngine.Rendering;
using UnityMesh = UnityEngine.Mesh;

namespace GanyuEditor.ModelPreview
{
    /// <summary>
    /// 将 Assimp Mesh 转成 Unity Mesh。第一阶段只处理静态无贴图预览。
    /// </summary>
    public static class AssimpUnityMeshBuilder
    {
        public static UnityMesh Build(Assimp.Mesh assimpMesh, bool convertGoldsrcToUnity = true)
        {
            var unityMesh = new UnityMesh
            {
                name = string.IsNullOrEmpty(assimpMesh.Name) ? "AssimpMesh" : assimpMesh.Name
            };

            var vertices = new List<Vector3>(assimpMesh.VertexCount);
            for (int i = 0; i < assimpMesh.VertexCount; i++)
            {
                vertices.Add(ToUnityVector(assimpMesh.Vertices[i], convertGoldsrcToUnity));
            }

            var indices = new List<int>(assimpMesh.FaceCount * 3);
            foreach (var face in assimpMesh.Faces)
            {
                if (face.IndexCount != 3)
                {
                    continue;
                }

                // 进行 GoldSrc -> Unity 坐标转换时会交换坐标轴，反转绕序以避免背面剔除方向错误。
                if (convertGoldsrcToUnity)
                {
                    indices.Add(face.Indices[0]);
                    indices.Add(face.Indices[2]);
                    indices.Add(face.Indices[1]);
                }
                else
                {
                    indices.Add(face.Indices[0]);
                    indices.Add(face.Indices[1]);
                    indices.Add(face.Indices[2]);
                }
            }

            if (vertices.Count > 65535)
            {
                unityMesh.indexFormat = IndexFormat.UInt32;
            }

            unityMesh.SetVertices(vertices);
            unityMesh.SetTriangles(indices, 0);

            if (assimpMesh.HasNormals && assimpMesh.Normals.Count == assimpMesh.VertexCount)
            {
                var normals = new List<Vector3>(assimpMesh.VertexCount);
                for (int i = 0; i < assimpMesh.VertexCount; i++)
                {
                    normals.Add(ToUnityVector(assimpMesh.Normals[i], convertGoldsrcToUnity).normalized);
                }
                unityMesh.SetNormals(normals);
            }
            else
            {
                unityMesh.RecalculateNormals();
            }

            if (assimpMesh.HasTextureCoords(0))
            {
                var uvs = new List<Vector2>(assimpMesh.VertexCount);
                var channel = assimpMesh.TextureCoordinateChannels[0];
                for (int i = 0; i < assimpMesh.VertexCount; i++)
                {
                    uvs.Add(new Vector2(channel[i].X, channel[i].Y));
                }
                unityMesh.SetUVs(0, uvs);
            }

            unityMesh.RecalculateBounds();
            return unityMesh;
        }

        private static Vector3 ToUnityVector(Vector3D vector, bool convertGoldsrcToUnity)
        {
            return convertGoldsrcToUnity
                ? new Vector3(vector.X, vector.Z, vector.Y)
                : new Vector3(vector.X, vector.Y, vector.Z);
        }
    }
}
