using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GanyuEditor
{
    public class ModelInfo : MonoBehaviour
    {

        public string ModelPath;
        public string Checksum;
        public string ModelName
        {
            get
            {
                var arr = ModelPath.Split('\\');
                var fileName = arr[arr.Length - 1];
                return fileName.Substring(0, fileName.Length - 4);
            }
        }
        public string OutputPath
        {
            get
            {
                var arr = ModelPath.Split('.');
                arr[arr.Length - 1] = "gpd";
                return string.Join(".", arr);
            }
        }

    }
}

