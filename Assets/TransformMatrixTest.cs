using GanyuEditor;
using GanyuEditor.Extensions;
using UnityEngine;


[ExecuteInEditMode]
public class TransformMatrixTest : MonoBehaviour
{


    //public GameObject Target;
    // Start is called before the first frame update
    void Start()
    {
        var m = Constants.RebaseMatrix;
        m.WriteToTransform(transform);
        var mm = transform.localToWorldMatrix;
        Debug.Log(m == mm);
    }

    // Update is called once per frame
    //void Update()
    //{
    //    Matrix4x4 worldMatrix = transform.localToWorldMatrix;

    //    var pos4 = worldMatrix.GetColumn(3);
    //    Vector3 pos = new Vector3(pos4.x, pos4.y, pos4.z);
    //    Quaternion rot = worldMatrix.rotation;
    //    Vector3 sca = worldMatrix.lossyScale;




    //    Target.transform.position = pos;
    //    Target.transform.rotation = rot;
    //    Target.transform.localScale = sca;

    //}
    bool MatrixEqual(in Matrix4x4 lhs, in Matrix4x4 rhs)
    {
        for (int i = 0; i < 16; i++)
        {
            if ((Mathf.Abs(lhs[i]) - Mathf.Abs(rhs[i])) > 1E-6)
                return false;
        }
        return true;
    }
}
