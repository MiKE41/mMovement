﻿using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;
using System;

namespace mMove
{
    internal class PluginUi : IDisposable
    {
        internal Plugin Plugin { get; }
        private MainInterface MainInterface { get; }
        internal PluginUi(Plugin plugin)
        {
            this.Plugin = plugin;

            this.MainInterface = new MainInterface(this);

            this.Plugin.Interface.UiBuilder.Draw += this.Draw;
            this.Plugin.Interface.UiBuilder.OpenConfigUi += this.OpenMainInterface;
        }

        internal void OpenMainInterface()
        {
            this.MainInterface.Toggle();
        }

        internal void Draw()
        {
            this.MainInterface.Draw();
        }

        public void Dispose()
        {
            this.Plugin.Interface.UiBuilder.Draw -= this.Draw;
            this.Plugin.Interface.UiBuilder.OpenConfigUi -= this.OpenMainInterface;
        }
    }

    public class MainInterface : Window
    {
        private PluginUi Ui { get; }
        public static string Name = "Settings";
        public new string WindowName => Name;
        internal MainInterface(PluginUi Ui) : base(Name)
        {
            this.Ui = Ui;
            IsOpen = false;
            Size = new Vector2(810, 520);
            SizeCondition = ImGuiCond.FirstUseEver;
        }

        public override void Draw()
        {
            if (!IsOpen) return;

            ImGui.Checkbox("Move Type Override Enabled", ref this.Ui.Plugin.Config.MoveModeOverride);
            if (ImGui.BeginCombo("MoveType:", $"{this.Ui.Plugin.Config.MoveModeOverrideValue}"))
            {
                foreach (var v in Enum.GetValues(typeof(Types.MoveMode)))
                {
                    if (ImGui.Selectable($"{v}"))
                    {
                        this.Ui.Plugin.Config.MoveModeOverrideValue = (Types.MoveMode)v;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.Checkbox("Camera Type Override Enabled", ref this.Ui.Plugin.Config.CameraModeOverride);
            if (ImGui.BeginCombo("CameraType:", $"{this.Ui.Plugin.Config.CameraModeOverrideValue}"))
            {
                foreach (var v in Enum.GetValues(typeof(Types.CameraMode)))
                {
                    if (ImGui.Selectable($"{v}"))
                    {
                        this.Ui.Plugin.Config.CameraModeOverrideValue = (Types.CameraMode)v;
                    }
                }
                ImGui.EndCombo();
            }

            ImGui.Text($"LegacyCameraLocation: {this.Ui.Plugin.Hooks.CameraArcLeftRight}");
            ImGui.Checkbox("LegacyCameraLocation Override Enabled", ref this.Ui.Plugin.Config.CameraArcOverride);

            ImGui.Checkbox("Right Click Override Enabled", ref this.Ui.Plugin.Config.RightClickOverride);
            ImGui.Checkbox("Strafe Override Enabled", ref this.Ui.Plugin.Config.StrafeOverride);

            if (ImGui.Button("Save")) { this.Ui.Plugin.SaveConfig();  }

            ImGui.Text("Memory things");
            ImGui.Text($"RightClick: {this.Ui.Plugin.Memory.RightClick()}");
            ImGui.Text($"LeftClick: {this.Ui.Plugin.Memory.LeftClick()}");
            ImGui.Text($"IsCharacterMoving: {this.Ui.Plugin.Memory.IsCharacterMoving()}");
            ImGui.Text($"IsAutoRunning: {this.Ui.Plugin.Memory.IsAutoRunning()}");
            var Camera = this.Ui.Plugin.Memory.Camera();
            foreach (var field in typeof(Memory.CameraMemoryStruct).GetFields())
            {
                ImGui.Text($"{field.Name}: {field.GetValue(Camera)}");
            }

            ImGui.Text("Hooks");
            ImGui.Text($"MoveModeValue: {this.Ui.Plugin.Hooks.MoveModeValue}");
            ImGui.Text($"CameraModeValue: {this.Ui.Plugin.Hooks.CameraModeValue}");
            ImGui.Text($"CameraArcLeftRightValue: {this.Ui.Plugin.Hooks.CameraArcLeftRight}");
        }
    }
}
