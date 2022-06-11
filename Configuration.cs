using Dalamud.Configuration;

namespace mMove
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public bool MoveModeOverride = true;
        public Types.MoveMode MoveModeOverrideValue = Types.MoveMode.Legacy;
        public bool CameraModeOverride = true;
        public Types.CameraMode CameraModeOverrideValue = Types.CameraMode.Standard;
        public bool CameraArcOverride = true;
        public bool RightClickOverride = true;
        public bool StrafeOverride = true;
    }
}
