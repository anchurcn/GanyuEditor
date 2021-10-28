using UnityEngine;


namespace GanyuEditor.Physics
{
    public class HingeConstraint : Constraint
    {
        public bool ShowLimitHandles;
        public bool ShowRotationHandle;


        // limits
        public float High;
        public float Low;

    }
}
