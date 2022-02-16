namespace Units.Player
{
    public class PlayerMovementData
    {
        #region Movement

        public float MaximumSpeed { get; }
        public float Acceleration { get; }
        public float Deceleration { get; }
        public float ApexBonusControl { get; }

        #endregion

        #region Jump

        public float JumpHeight { get; }
        public float MinFallAcceleration { get; }
        public float MaxFallAcceleration { get; }
        public float MaxFallSpeed { get; }
        public float CoyoteTimeThreshold { get; }
        public float JumpApexThreshold { get; }
        public float JumpEndEarlyGravityModifier { get; }
        public float MouseSensitivity { get; }

        #endregion

        public PlayerMovementData(float[] playerMovementConfig)
        {
            MaximumSpeed = playerMovementConfig[0];
            Acceleration = playerMovementConfig[1];
            Deceleration = playerMovementConfig[2];
            ApexBonusControl = playerMovementConfig[3];
            JumpHeight = playerMovementConfig[4];
            MinFallAcceleration = playerMovementConfig[5];
            MaxFallAcceleration = playerMovementConfig[6];
            MaxFallSpeed = playerMovementConfig[7];
            MouseSensitivity = playerMovementConfig[8];
            CoyoteTimeThreshold = playerMovementConfig[9];
            JumpApexThreshold = playerMovementConfig[10];
            JumpEndEarlyGravityModifier = playerMovementConfig[11];
        }
    }
}