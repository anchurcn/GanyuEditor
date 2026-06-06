using System.IO;
using UnityEngine;

namespace GanyuEditor
{
    public class ModelInfo : MonoBehaviour
    {
        public string ModelPath;
        public string Checksum;

        public string ModelName =>
            string.IsNullOrEmpty(ModelPath) ? string.Empty : Path.GetFileNameWithoutExtension(ModelPath);

        public string OutputPath =>
            string.IsNullOrEmpty(ModelPath) ? string.Empty : Path.ChangeExtension(ModelPath, "gpd");
    }
}