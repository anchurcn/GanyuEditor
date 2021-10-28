using UnityEditor;


namespace GanyuEditor
{
    [CustomEditor(typeof(StudioBone)), CanEditMultipleObjects]
    public class StudioBoneEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            Tools.hidden = true;
        }
        private void OnDisable()
        {
            Tools.hidden = false;
        }
    }
}