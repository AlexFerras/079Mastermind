using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Interactables.Interobjects.DoorUtils;
using System.Collections.Generic;
using UnityEngine;
using System;
using MEC;

namespace Plugin
{
    public class Plugin : Plugin<Config>
    {

        public override string Author => "Ferrasick";
        public override Version Version => new Version(0, 0, 8);
        public override Version RequiredExiledVersion => new Version(7, 0, 0, 0);

        public static Plugin plugin;

        public override PluginPriority Priority { get; } = PluginPriority.Default;

        public EventHandlers handlers;

        public int minPlayers;

        public override void OnEnabled()
        {
            plugin = this;
            minPlayers = Config.MinPlayers;
            handlers = new EventHandlers();
            Exiled.Events.Handlers.Player.InteractingDoor += handlers.OnDoorInteraction;
            Exiled.Events.Handlers.Player.VoiceChatting += handlers.OnVoiceChatting;
            Exiled.Events.Handlers.Server.RoundStarted += handlers.OnRoundStarted;
            Exiled.Events.Handlers.Server.EndingRound += handlers.OnEndingRound;
            Exiled.Events.Handlers.Server.RoundEnded += handlers.OnRoundEnded;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.InteractingDoor -= handlers.OnDoorInteraction;
            Exiled.Events.Handlers.Player.VoiceChatting -= handlers.OnVoiceChatting;
            Exiled.Events.Handlers.Server.RoundStarted -= handlers.OnRoundStarted;
            Exiled.Events.Handlers.Server.EndingRound -= handlers.OnEndingRound;
            Exiled.Events.Handlers.Server.RoundEnded -= handlers.OnRoundEnded;
            base.OnDisabled();
            handlers = null;
            plugin = null;
        }
    }
}
