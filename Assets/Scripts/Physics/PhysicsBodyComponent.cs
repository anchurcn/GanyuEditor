using System;
using UnityEngine;


namespace GanyuEditor.Physics
{
    [RequireComponent(typeof(CollisionShapeComponent))]
    [DisallowMultipleComponent]
    public class PhysicsBody : MonoBehaviour
    {
        public bool IsAttachment;
        public int BoneIndex => GetComponent<StudioBone>().Index;
    }
}
