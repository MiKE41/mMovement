using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using Dalamud.Game;

namespace mMove
{
    public class Plugin : IDalamudPlugin
    {
        public string Name => "mMove";
        private const string SettingsCommand = "/mmove";

        [PluginService] internal DalamudPluginInterface Interface { get; private set; }
        [PluginService] internal SigScanner SigScanner { get; private set; }
        [PluginService] internal ClientState ClientState { get; private set; }
        [PluginService] internal CommandManager CommandManager { get; private set; }
        [PluginService] internal Condition Condition { get; private set; }

        internal Configuration Config { get; }
        internal Memory Memory { get; }
        internal PluginUi Ui { get; }
        internal Hooks Hooks { get; }
        private Commands Commands { get; }

        public Plugin()
        {
            this.Config = this.Interface!.GetPluginConfig() as Configuration ?? new Configuration();

            // Info gathering //Camera Memory //Is Player Moving //Mouse State
            this.Memory = new Memory(this);

            // Hooks //Camera Location //Move Type //Camera Type
            this.Hooks = new Hooks(this);

            //Create Windows
            this.Ui = new PluginUi(this);

            //Create Commands
            this.Commands = new Commands(this);
        }

        public void Dispose()
        {
            this.Commands.Dispose();
            this.Ui.Dispose();
            //this.Memory.Dispose();
            this.Hooks.Dispose();
        }
        internal void SaveConfig()
        {
            this.Interface.SavePluginConfig(this.Config);
        }
    }
}
