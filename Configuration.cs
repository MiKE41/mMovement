using Dalamud.Configuration;

namespace mMovement
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public bool MovementModeOverride = true;
        public Types.MovementMode MovementModeOverrideValue = Types.MovementMode.MiKE;
        public bool CameraModeOverride = true;
        public Types.CameraMode CameraModeOverrideValue = Types.CameraMode.Standard;
        public bool CameraArcOverride = true;
    }
}
