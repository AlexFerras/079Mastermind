using Exiled.API.Interfaces;
using System;

namespace Plugin
{
    public sealed class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        public bool Debug { get; set; } = true;

        public int MinPlayers { get; set; } = 7;
    }
}
