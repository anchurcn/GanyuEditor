using UnityEngine;


namespace GanyuEditor.Physics
{
    public class HingeConstraintComponent : PhysicsConstraintComponent
    {
        public bool ShowLimitHandles;
        public bool ShowRotationHandle;


        // limits
        public float High;
        public float Low;

    }
}
