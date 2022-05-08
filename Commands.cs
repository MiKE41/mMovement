using System;
using Dalamud.Game.Command;

namespace mMovement
{
    internal class Commands : IDisposable
    {
        private Plugin Plugin { get; }

        internal Commands(Plugin plugin)
        {
            this.Plugin = plugin;

            this.Plugin.CommandManager.AddHandler("/mmovement", new CommandInfo(this.OnCommand)
            {
                HelpMessage = $"Toggle visibility of the {this.Plugin.Name} window",
            });
        }

        public void Dispose()
        {
            this.Plugin.CommandManager.RemoveHandler("/mmovement");
        }

        private void OnCommand(string command, string arguments)
        {
            this.Plugin.Ui.OpenMainInterface();
        }
    }
}
