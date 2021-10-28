using UnityEngine;
using UnityEditor;


namespace GanyuEditor.Physics
{
    public class ToolMenu
    {
        [MenuItem("GoldsrcPhysics/ImportStudioModelBones(.mdl)")]
        public static void Import()
        {
            ScriptableWizard.DisplayWizard<ImportStudioBoneWizard>("导入模型骨骼", "确认");
        }

        /// <summary>
        /// Export selected ragdoll.
        /// </summary>
        /// <remarks>
        /// Call for each selected object if multi-selected.
        /// But ignore the children if their parents are also selected.
        /// </remarks>
        /// <param name="menuCommand"></param>
        [MenuItem("GameObject/ExportRagdoll (same path)", priority = 11)]
        static void ExportRagdoll(MenuCommand menuCommand)
        {
            if (menuCommand.context is GameObject modelRoot &&
                modelRoot.GetComponent<ModelInfo>() is var modelInfo &&
                !ReferenceEquals(modelInfo, null))
            {
                PhysicsDataExporter exporter = new PhysicsDataExporter()
                {
                    ModelRoot = modelRoot,
                    OutputPath = modelInfo.OutputPath
                };
                exporter.Export();
                Debug.Log($"Export physics data to {modelInfo.OutputPath} successfully.");
            }
            else
            {
                Debug.Log("Nothing export. Please select a model to export.");
            }
        }
        //[MenuItem("GameObject/ExportRagdoll...", priority = 11)]
        //static void ExportRagdollEx(MenuCommand menuCommand)
        //{

        //}
        //[MenuItem("GoldsrcPhysics/ExportGoldsrcPhysicsDataFile(.gpd)")]
        //public static void Export()
        //{
        //    var modelObjects = Selection.GetFiltered<GameObject>(SelectionMode.Unfiltered);
        //    foreach (var i in modelObjects)
        //    {
        //        var modelInfo = i.GetComponent<ModelInfo>();
        //        if (modelInfo != null)
        //        {
        //            Debug.Log($"Export physics data to {modelInfo.OutputPath}.");
        //            ExportPhysicsData exportPhysicsData = new ExportPhysicsData();
        //            exportPhysicsData.OutputPath = modelInfo.OutputPath;
        //            exportPhysicsData.GameObjectWithModelInfo = i;
        //            exportPhysicsData.Export();
        //        }
        //    }
        //}
        //[MenuItem("GoldsrcPhysics/CreateSkined")]
        //public static unsafe void CreateSkined()
        //{
        //    var model = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    model.name = "model";
        //    var meshFilter = model.GetComponent<MeshFilter>();

        //    var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    root.name = "root";
        //    var bone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    bone.name = "bone";

        //    var vertices = new Vector3[]
        //    {
        //        new Vector3(0,0,0),
        //        new Vector3(0,10,0),
        //        new Vector3(10,0,0)
        //    };
        //    var indeces = new int[]
        //    {
        //        0, 1, 2
        //    };
        //    var indeces2 = new int[]
        //    {
        //        0,2,1
        //    };
        //    var mesh = new Mesh()
        //    {
        //        vertices = vertices,
        //        triangles = indeces,
        //        name = "SingleTri",
        //    };
        //    meshFilter.mesh = mesh;

        //    Assimp.AssimpContext context = new Assimp.AssimpContext();
        //    Debug.Log("imp zombie");
        //    var scene = context.ImportFile(@"F:\Steam\steamapps\common\Half-Life\valve\models\player\zombie\zombie.mdl");
        //    var p = (AiScene*)Assimp.Unmanaged.AssimpLibrary.Instance.ImportFile("", Assimp.PostProcessSteps.Debone, IntPtr.Zero);
        //    var meshes = scene.Meshes;
        //}

    }
}
