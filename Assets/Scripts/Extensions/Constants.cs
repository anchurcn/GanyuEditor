using System.Collections;
using UnityEngine;


namespace GanyuEditor
{
    public class Constants
    {
        public static readonly Matrix4x4 RebaseMatrix = new Matrix4x4(
                        new Vector4(1, 0, 0, 0),
                        new Vector4(0, 0, 1, 0),
                        new Vector4(0, 1, 0, 0),
                        new Vector4(0, 0, 0, 1));

    }
}
