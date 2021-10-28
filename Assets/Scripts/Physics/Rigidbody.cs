using System;
using UnityEngine;


namespace GanyuEditor.Physics
{
    [RequireComponent(typeof(CollisionShape))]
    [DisallowMultipleComponent]
    public class Rigidbody : MonoBehaviour
    {
        public bool IsAttachment;
        public int BoneIndex => GetComponent<StudioBone>().Index;
    }
}
