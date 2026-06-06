namespace GanyuEditor.Editor.Physics.Wizards
{
    /// <summary>
    /// Constants for ragdoll body proportions and constraint limits
    /// </summary>
    public static class RagdollConstants
    {
        // 身体比例常量
        public const float BodyToHeadWidthRatio = 2.5f;
        public const float HeadAspectRatio = 5.85f / 8.1f;
        
        // 肢体比例
        public const float UpperArmWidthRatio = 0.55f;
        public const float LowerArmWidthRatio = 0.5f;
        public const float UpperLegWidthRatio = 0.55f;
        public const float LowerLegWidthRatio = 0.5f;
        
        // 躯干比例
        public const float PelvisHeightRatio = 1.1f;
        public const float SpineDepthRatio = 0.7f;
        public const float ChestDepthRatio = 0.7f;
        
        /// <summary>
        /// Constraint angle limits (in degrees)
        /// </summary>
        public static class ConstraintLimits
        {
            // Spine约束
            public const float SpineTwistSpan = 20f;
            public const float SpineSwingSpan1 = 35f;
            public const float SpineSwingSpan2 = 10f;
            
            // Chest约束
            public const float ChestTwistSpan = 25f;
            public const float ChestSwingSpan1 = 15f;
            public const float ChestSwingSpan2 = 5f;
            
            // Head约束
            public const float HeadTwistSpan = 30f;
            public const float HeadSwingSpan1 = 30f;
            public const float HeadSwingSpan2 = 8f;
            
            // Arm约束
            public const float ArmTwistSpan = 25f;
            public const float ArmSwingSpan1 = 90f;
            public const float ArmSwingSpan2 = 50f;
            
            // Elbow约束
            public const float ElbowLow = -140f;
            
            // Hip约束
            public const float HipTwistSpan = 3f;
            public const float HipSwingSpan1 = 40f;
            public const float HipSwingSpan2 = 20f;
            
            // Knee约束
            public const float KneeHigh = 135f;
        }
    }
}
