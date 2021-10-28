using UnityEngine;


namespace GanyuEditor.Physics
{
    public class ConeTwistConstraint : Constraint
    {
        public bool ShowLimitHandles;
        public bool ShowRotationHandle;


        // limits
        /// <summary>
        /// X
        /// </summary>
        public float TwistSpan;
        /// <summary>
        /// Z
        /// </summary>
        public float SwingSpan1;
        /// <summary>
        /// Y
        /// </summary>
        public float SwingSpan2;
        /// <summary>
        /// World transform matrix in studio space.
        /// </summary>

    }
}