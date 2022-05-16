using System;
using Dalamud.Utility.Signatures;
using Dalamud.Logging;
using System.Runtime.InteropServices;

namespace mMovement
{
    internal class Memory
    {
        private Plugin Plugin { get; }
        private static class Signatures
        {
            internal const string CameraSignature = "48 8B 3D ?? ?? ?? ?? 48 85 FF 0F 84 ?? ?? ?? ?? F3 0F 10 81"; //Client__Game__Camera3_vf10 -> g_Client::Game::ControlSystem::CameraManager_Instance
            internal const string g_PlayerMoveControllerSignature = "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 8B ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 83 3D"; //Client::Game::GameMain_Update
            internal const string g_InputManager_MouseButtonHoldStateSignature = "39 1D ?? ?? ?? ?? 74 ?? 48 8D 0D";
        }

        [Signature(Signatures.CameraSignature, ScanType = ScanType.StaticAddress)]
        internal readonly IntPtr CameraPtr;
        internal readonly IntPtr CameraAddress;

        [Signature(Signatures.g_PlayerMoveControllerSignature, ScanType = ScanType.StaticAddress)]
        private readonly IntPtr g_PlayerMoveControllerAddress;

        [Signature(Signatures.g_InputManager_MouseButtonHoldStateSignature, ScanType = ScanType.StaticAddress)]
        private readonly IntPtr g_InputManager_MouseButtonHoldStateAddress;

        [StructLayout(LayoutKind.Explicit)]
        public struct CameraMemoryStruct
        {
            [FieldOffset(0x60)] public float camera_x;
            [FieldOffset(0x64)] public float camera_z;
            [FieldOffset(0x68)] public float camera_y;
            [FieldOffset(0x90)] public float camera_focus_x;
            [FieldOffset(0x94)] public float camera_focus_z;
            [FieldOffset(0x98)] public float camera_focus_y;
            [FieldOffset(0x108)] public Int32 cameraState;          //seems to be 1 when moving, 0 when not moving?
            [FieldOffset(0x114)] public float zoomCurrent;
            [FieldOffset(0x118)] public float zoomMin;
            [FieldOffset(0x11C)] public float zoomMax;
            [FieldOffset(0x120)] public float fovCurrent;
            [FieldOffset(0x124)] public float fovMin;
            [FieldOffset(0x128)] public float fovMax;
            [FieldOffset(0x12C)] public float fov2;
            [FieldOffset(0x130)] public float arc_left_right;
            [FieldOffset(0x134)] public float arc_up_down;
            [FieldOffset(0x150)] public float pan;
            [FieldOffset(0x154)] public float tilt;
            [FieldOffset(0x160)] public float rotation;
            [FieldOffset(0x174)] public Int32 cameraMode1;
            [FieldOffset(0x178)] public Int32 cameraMode2;
        }

        internal Memory(Plugin plugin)
        {
            this.Plugin = plugin;
            SignatureHelper.Initialise(this);

            CameraAddress = Marshal.ReadIntPtr(CameraPtr);

            PluginLog.Verbose($"CameraAddress {CameraAddress.ToInt64():X}");
            PluginLog.Verbose($"g_PlayerMoveControllerAddress {g_PlayerMoveControllerAddress.ToInt64():X}");
            PluginLog.Verbose($"g_InputManager_MouseButtonHoldStateAddress {g_InputManager_MouseButtonHoldStateAddress.ToInt64():X}");
        }

        private bool GetBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        public bool RightClick()
        {
            return GetBit(Marshal.ReadByte(g_InputManager_MouseButtonHoldStateAddress), 1);
        }

        public bool LeftClick()
        {
            return GetBit(Marshal.ReadByte(g_InputManager_MouseButtonHoldStateAddress), 0);
        }

        public bool IsCharacterMoving()
        {
            IntPtr a = Marshal.ReadIntPtr(g_PlayerMoveControllerAddress + 0x20);

            if (a == IntPtr.Zero)
            {
                return false;
            }

            return Marshal.ReadByte(a + 0x1FD) == 1;
        }

        public CameraMemoryStruct Camera()
        {
            return Marshal.PtrToStructure<CameraMemoryStruct>(CameraAddress);
        }

        public void SetCharacterRotationToCamera()
        {
            Marshal.WriteByte(g_PlayerMoveControllerAddress + 0x3F, 1);
        }
    }
}
