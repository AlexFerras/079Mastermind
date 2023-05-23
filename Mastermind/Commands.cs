
namespace Plugin.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CommandSystem;

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class MMminplayers : ICommand
    {
        public string Command { get; } = "mmminplayers";

        public string[] Aliases { get; } = new string[0];

        public string Description => "sets min players for gamemode";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!int.TryParse(arguments.First(), out int count))
            {
                response = "wrong argument type";
                return false;

            }
            Plugin.plugin.minPlayers = count;
            response = "successfully changed.";
            return true;
        }



    }

    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class MMforcemode : ICommand
    {
        public string Command { get; } = "mmforcemode";

        public string[] Aliases { get; } = new string[0];

        public string Description => "sets modeStarted to param";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!int.TryParse(arguments.First(), out int value) && value >= 0 && value <= 1)
            {
                response = "wrong argument type";
                return false;
            }

            Plugin.plugin.handlers.modeStarted = value == 1;
            response = "successfully changed.";
            return true;
        }
    }
}
