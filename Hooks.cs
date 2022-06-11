using System;
using System.Runtime.InteropServices;
using Dalamud.Utility.Signatures;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Game.ClientState.Conditions;

namespace mMove
{
    internal class Hooks : IDisposable
    {
        private Plugin Plugin { get; }
        private static class Signatures
        {
            internal const string GetCameraMode = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B F9 BB";
            internal const string GetMoveMode = "48 83 EC ?? 0F B6 41 ?? 3C ?? 74";
            internal const string GetCameraArc = "E8 ?? ?? ?? ?? 0F 28 F0 E8 ?? ?? ?? ?? 0F 28 F8 0F 28 C6 E8 ?? ?? ?? ?? 0F 28 CF";
            internal const string IsInputIDKeyPress = "E8 ?? ?? ?? ?? 84 C0 48 63 03";
            internal const string IsInputIDKeyPress2 = "E8 ?? ?? ?? ?? 48 63 13";
            internal const string ShouldRotatePlayerCamera = "E8 ?? ?? ?? ?? 48 8B CB 85 C0 0F 84 ?? ?? ?? ?? 83 E8 ";
        }
        #region Delegates
        private delegate Types.CameraMode GetCameraModeDelegate(IntPtr a1);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate Types.MoveMode GetMoveModeDelegate(IntPtr a1);

        private delegate float GetCameraArcDelegate(IntPtr a1);

        private delegate bool IsInputIDKeyPressDelegate(IntPtr a1, int a2);

        private delegate bool IsInputIDKeyPress2Delegate(IntPtr a1, int a2);

        private delegate byte ShouldRotatePlayerCameraDelegate(IntPtr a1, IntPtr a2, byte a3, byte a4);
        #endregion
        #region Hooks
        [Signature(Signatures.GetCameraMode, DetourName = nameof(GetCameraModeDetour))]
        private readonly Hook<GetCameraModeDelegate>? GetCameraModeHook = null;

        [Signature(Signatures.GetMoveMode, DetourName = nameof(GetMoveModeDetour))]
        private readonly Hook<GetMoveModeDelegate>? GetMoveModeHook = null;

        [Signature(Signatures.GetCameraArc, DetourName = nameof(GetCameraArcDetour))]
        private readonly Hook<GetCameraArcDelegate>? GetCameraArcHook = null;

        [Signature(Signatures.IsInputIDKeyPress, DetourName = nameof(IsInputIDKeyPressDetour))]
        private readonly Hook<IsInputIDKeyPressDelegate>? IsInputIDKeyPressHook = null;

        [Signature(Signatures.IsInputIDKeyPress2, DetourName = nameof(IsInputIDKeyPress2Detour))]
        private readonly Hook<IsInputIDKeyPress2Delegate>? IsInputIDKeyPress2Hook = null;

        [Signature(Signatures.ShouldRotatePlayerCamera, DetourName = nameof(ShouldRotatePlayerCameraDetour))]
        private readonly Hook<ShouldRotatePlayerCameraDelegate>? ShouldRotatePlayerCameraHook = null;
        #endregion
        internal Hooks(Plugin plugin)
        {
            this.Plugin = plugin;

            SignatureHelper.Initialise(this);

            this.GetCameraModeHook?.Enable();
            this.GetCameraArcHook?.Enable();
            this.GetMoveModeHook?.Enable();
            this.IsInputIDKeyPressHook?.Enable(); // this one only seems to handle the strafe key presses -- and all it seems to do is force the character to backpeddle when holding move back and strafe at the same time.
            this.IsInputIDKeyPress2Hook?.Enable();
            this.ShouldRotatePlayerCameraHook?.Enable();

            PluginLog.Verbose($"GetCameraMode: {GetCameraModeHook.Address.ToInt64():X}");
            PluginLog.Verbose($"GetMoveMode: {GetMoveModeHook.Address.ToInt64():X}");
            PluginLog.Verbose($"GetCameraArc: {GetCameraArcHook.Address.ToInt64():X}");
            PluginLog.Verbose($"IsInputIDKeyPress: {IsInputIDKeyPressHook.Address.ToInt64():X}");
            PluginLog.Verbose($"IsInputIDKeyPress2: {IsInputIDKeyPress2Hook.Address.ToInt64():X}");
            PluginLog.Verbose($"ShouldRotatePlayerCamera: {ShouldRotatePlayerCameraHook.Address.ToInt64():X}");
        }
        public void Dispose()
        {
            this.GetCameraModeHook?.Dispose();
            this.GetCameraArcHook?.Dispose();
            this.GetMoveModeHook?.Dispose();
            this.IsInputIDKeyPressHook?.Dispose();
            this.IsInputIDKeyPress2Hook?.Dispose();
            this.ShouldRotatePlayerCameraHook?.Dispose();
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
        internal Types.MoveMode MoveModeValue;
        private Types.MoveMode GetMoveModeDetour(IntPtr a1)
        {
            Types.MoveMode ret = this.GetMoveModeHook.Original(a1);

            if (this.Plugin.ClientState.IsLoggedIn && this.Plugin.Config.MoveModeOverride)
            {
                    ret = this.Plugin.Config.MoveModeOverrideValue;
            }

            MoveModeValue = ret;
            return ret;
        }
        internal float CameraArcLeftRight;
        private float GetCameraArcDetour(IntPtr a1)
        {
            float ret = this.GetCameraArcHook.Original(a1);

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

            if (this.Plugin.Config.StrafeOverride)
            {
                if (!this.Plugin.Memory.IsAutoRunning() && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.MoveBack))
                {
                    if (a2 == Keybind.StrafeLeft || a2 == Keybind.StrafeRight)
                    {
                        ret = false;
                    }

                    if (a2 == Keybind.TurnLeft && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.StrafeLeft))
                    {
                        ret = true;
                    }

                    if (a2 == Keybind.TurnRight && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.StrafeRight))
                    {
                        ret = true;
                    }
                }

                if (this.Plugin.Memory.IsAutoRunning())
                {
                    if (a2 == Keybind.TurnLeft || a2 == Keybind.TurnRight)
                    {
                        ret = false;
                    }

                    if (a2 == Keybind.StrafeLeft && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.TurnLeft))
                    {
                        ret = true;
                    }

                    if (a2 == Keybind.StrafeRight && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.TurnRight))
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }

        private bool IsInputIDKeyPress2Detour(IntPtr a1, Keybind a2)
        {
            bool ret = IsInputIDKeyPress2Hook.Original(a1, (int)a2);

            if (this.Plugin.Config.StrafeOverride)
            {
                if (!this.Plugin.Memory.IsAutoRunning() && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.MoveBack))
                {
                    if (a2 == Keybind.StrafeLeft || a2 == Keybind.StrafeRight)
                    {
                        ret = false;
                    }

                    if (a2 == Keybind.TurnLeft && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.StrafeLeft))
                    {
                        ret = true;
                    }

                    if (a2 == Keybind.TurnRight && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.StrafeRight))
                    {
                        ret = true;
                    }
                }

                if (this.Plugin.Memory.IsAutoRunning())
                {
                    if (a2 == Keybind.TurnLeft || a2 == Keybind.TurnRight)
                    {
                        ret = false;
                    }

                    if (a2 == Keybind.StrafeLeft && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.TurnLeft))
                    {
                        ret = true;
                    }

                    if (a2 == Keybind.StrafeRight && IsInputIDKeyPress2Hook.Original(a1, (int)Keybind.TurnRight))
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }
        
        private byte ShouldRotatePlayerCameraDetour(IntPtr a1, IntPtr a2, byte a3, byte a4)
        {
            byte ret = ShouldRotatePlayerCameraHook.Original(a1, a2, a3, a4);

            //PluginLog.Verbose($"ShouldRot: a1: {a1}; a2: {a2}; a3: {a3}; a4: {a4}; ret: {ret};");

            if (this.Plugin.Memory.RightClick() && !this.Plugin.Memory.IsCharacterMoving() && this.Plugin.Config.RightClickOverride)
            {
                ret = 1;
            }

            return ret;
        }
    }
}
