using Assimp;

namespace GanyuEditor.ModelPreview
{
    /// <summary>
    /// Assimp 导入封装。调用方负责捕获异常或使用 TryImport。
    /// </summary>
    public static class AssimpModelImportService
    {
        private const PostProcessSteps DefaultPostProcessSteps =
            PostProcessSteps.Triangulate |
            PostProcessSteps.GenerateNormals |
            PostProcessSteps.JoinIdenticalVertices;

        public static Scene Import(string modelPath)
        {
            using (var importer = new AssimpContext())
            {
                return importer.ImportFile(modelPath, DefaultPostProcessSteps);
            }
        }

        public static bool TryImport(string modelPath, out Scene scene, out string error)
        {
            scene = null;
            error = null;

            try
            {
                scene = Import(modelPath);
                if (scene == null)
                {
                    error = "Assimp returned null scene.";
                    return false;
                }

                if (!scene.HasMeshes)
                {
                    error = "Assimp scene has no meshes.";
                    return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
