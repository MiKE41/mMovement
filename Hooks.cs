using System;
using System.Runtime.InteropServices;
using Dalamud.Utility.Signatures;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Game.ClientState.Conditions;

namespace mMovement
{
    internal class Hooks : IDisposable
    {
        private Plugin Plugin { get; }
        private static class Signatures
        {
            internal const string GetCameraMode = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B F9 BB"; //Client__Game__Camera_vf16
            internal const string GetMovementMode = "48 83 EC ?? 0F B6 41 ?? 3C ?? 74";
            internal const string GetCameraArcLeftRight = "E8 ?? ?? ?? ?? 0F 28 F0 E8 ?? ?? ?? ?? 0F 28 F8 0F 28 C6 E8 ?? ?? ?? ?? 0F 28 CF"; //A call from MP_Something_CharacterMovement2 to Client__Game__Camera_MP5_Legacy_Position
            internal const string IsInputIDKeyPress = "E8 ?? ?? ?? ?? 84 C0 48 63 03";
        }
        #region Delegates
        private delegate Types.CameraMode GetCameraModeDelegate(IntPtr a1);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Types.MovementMode GetMovementModeDelegate(IntPtr a1);

        private delegate float GetCameraArcLeftRightDelegate(IntPtr a1);
        private delegate bool IsInputIDKeyPressDelegate(IntPtr a1, int a2);
        #endregion
        #region Hooks
        [Signature(Signatures.GetCameraMode, DetourName = nameof(GetCameraModeDetour))]
        private readonly Hook<GetCameraModeDelegate>? GetCameraModeHook = null;

        [Signature(Signatures.GetMovementMode, DetourName = nameof(GetMovementModeDetour))]
        private readonly Hook<GetMovementModeDelegate>? GetMovementModeHook = null;

        [Signature(Signatures.GetCameraArcLeftRight, DetourName = nameof(GetCameraArcLeftRightDetour))]
        private readonly Hook<GetCameraArcLeftRightDelegate>? GetCameraArcLeftRightHook = null;

        [Signature(Signatures.IsInputIDKeyPress, DetourName = nameof(IsInputIDKeyPressDetour))]
        private readonly Hook<IsInputIDKeyPressDelegate>? IsInputIDKeyPressHook = null;
        #endregion
        internal Hooks(Plugin plugin)
        {
            this.Plugin = plugin;

            SignatureHelper.Initialise(this);

            this.GetCameraModeHook?.Enable();
            this.GetCameraArcLeftRightHook?.Enable();
            this.GetMovementModeHook?.Enable();
            this.IsInputIDKeyPressHook?.Enable(); // this one only seems to handle the strafe key presses -- and all it seems to do is force the character to backpeddle when holding move back and strafe at the same time.

            PluginLog.Verbose($"GetCameraMode: {GetCameraModeHook.Address.ToInt64():X}");
            PluginLog.Verbose($"GetMovementMode: {GetMovementModeHook.Address.ToInt64():X}");
            PluginLog.Verbose($"GetCameraArcLeftRight: {GetCameraArcLeftRightHook.Address.ToInt64():X}");
            PluginLog.Verbose($"IsInputIDKeyPress: {IsInputIDKeyPressHook.Address.ToInt64():X}");
        }
        public void Dispose()
        {
            this.GetCameraModeHook?.Dispose();
            this.GetCameraArcLeftRightHook?.Dispose();
            this.GetMovementModeHook?.Dispose();
            this.IsInputIDKeyPressHook?.Dispose();
        }
        internal Types.CameraMode CameraModeValue;
        private Types.CameraMode GetCameraModeDetour(IntPtr a1)
        {
            Types.CameraMode ret = this.GetCameraModeHook.Original(a1);

            if (this.Plugin.ClientState.IsLoggedIn && this.Plugin.Config.CameraModeOverride && (ret == Types.CameraMode.Legacy || ret == Types.CameraMode.Standard))
            {
                ret = this.Plugin.Config.CameraModeOverrideValue;
            }

            CameraModeValue = ret;
            return ret;
        }
        internal Types.MovementMode MovementModeValue;
        private Types.MovementMode GetMovementModeDetour(IntPtr a1)
        {
            Types.MovementMode ret = this.GetMovementModeHook.Original(a1);

            if (this.Plugin.ClientState.IsLoggedIn && this.Plugin.Config.MovementModeOverride)
            {
                    ret = this.Plugin.Config.MovementModeOverrideValue;
            }

            MovementModeValue = ret;
            return ret;
        }
        internal float CameraArcLeftRight;
        private float GetCameraArcLeftRightDetour(IntPtr a1)
        {
            float ret = this.GetCameraArcLeftRightHook.Original(a1);

            if (this.Plugin.ClientState.IsLoggedIn && this.Plugin.Config.CameraArcOverride)
            {
                if (!this.Plugin.Memory.IsCharacterMoving() || this.Plugin.Memory.RightClick())
                {
                    CameraArcLeftRight = ret;
                }
                else
                {
                    ret = CameraArcLeftRight;
                }
            }

            CameraArcLeftRight = ret;
            return ret;
        }
        private enum Keybind : int
        {
            MoveForward = 321,
            MoveBack = 322,
            TurnLeft = 323,
            TurnRight = 324,
            StrafeLeft = 325,
            StrafeRight = 326,
        }

        private bool IsInputIDKeyPressDetour(IntPtr a1, Keybind a2)
        {
            bool ret = IsInputIDKeyPressHook.Original(a1, (int)a2);

            if (a2 == Keybind.StrafeRight || a2 == Keybind.StrafeLeft) {
                ret = false;
            }

            return ret;
        }
    }
}
