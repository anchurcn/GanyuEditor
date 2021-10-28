using System.Linq;
using UnityEditor;
using UnityEngine;


namespace GanyuEditor.Physics
{
    class ExportRagdollWizard:ScriptableWizard
    {
        public string Path;
        public string Name;
        private GameObject ModelRoot;
        private void OnWizardCreate()
        {
            
        }
        public ExportRagdollWizard():base()
        {
            var modelObjects = Selection.GetFiltered<GameObject>(SelectionMode.Unfiltered);
            var modelRoot = modelObjects.FirstOrDefault(x => x.GetComponent<ModelInfo>() != null);
            if(modelRoot!=null)
            {
                var modelInfo = modelRoot.GetComponent<ModelInfo>();
                Debug.Log($"Export physics data to {modelInfo.OutputPath}.");
            }
            else
            {
                Debug.LogWarning("select a model to export.");
            }
            foreach (var i in modelObjects)
            {
                var modelInfo = i.GetComponent<ModelInfo>();
                if (modelInfo != null)
                {
                    Debug.Log($"Export physics data to {modelInfo.OutputPath}.");
                }
            }
        }
    }
}
