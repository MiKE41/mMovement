using System;
using Dalamud.Game.Command;

namespace mMove
{
    internal class Commands : IDisposable
    {
        private Plugin Plugin { get; }

        internal Commands(Plugin plugin)
        {
            this.Plugin = plugin;

            this.Plugin.CommandManager.AddHandler("/mMove", new CommandInfo(this.OnCommand)
            {
                HelpMessage = $"Toggle visibility of the {this.Plugin.Name} window",
            });
        }

        public void Dispose()
        {
            this.Plugin.CommandManager.RemoveHandler("/mMove");
        }

        private void OnCommand(string command, string arguments)
        {
            this.Plugin.Ui.OpenMainInterface();
        }
    }
}
