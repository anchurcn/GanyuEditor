using UnityEngine;


namespace GanyuEditor.Physics
{
    public class HingeConstraint : PhysicsConstraint
    {
        public bool ShowLimitHandles;
        public bool ShowRotationHandle;


        // limits
        public float High;
        public float Low;

    }
}
