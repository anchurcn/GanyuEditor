using System;
using UnityEngine;


namespace GanyuEditor.Physics
{
    [RequireComponent(typeof(CollisionShape))]
    [DisallowMultipleComponent]
    public class PhysicsBody : MonoBehaviour
    {
        public bool IsAttachment;
        public int BoneIndex => GetComponent<StudioBone>().Index;
    }
}
