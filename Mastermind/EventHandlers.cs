using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp079;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Plugin
{

    public class Data
    {
        public IDictionary<string, PlayerRoles.RoleTypeId> doorsDict = new Dictionary<string, PlayerRoles.RoleTypeId>
        {
            //{ "079", RoleTypeId.Scp079 },
            {"096", RoleTypeId.Scp096 },
            {"106_PRIMARY", RoleTypeId.Scp106 },
            {"106_SECONDARY", RoleTypeId.Scp106 },
            {"173_GATE", RoleTypeId.Scp173},
            {"HCZ", RoleTypeId.Scp939 },
            {"Unsecured", RoleTypeId.Scp049},
        };
        public IDictionary<RoleTypeId, bool> availableScps = new Dictionary<RoleTypeId, bool>
        {
            {RoleTypeId.Scp096, true},
            {RoleTypeId.Scp173, true },
            {RoleTypeId.Scp049, true },
            {RoleTypeId.Scp106, true },
            {RoleTypeId.Scp939, true }
        };

        public Player master = null;

        public TimeSpan cooldownTime = TimeSpan.Zero;
    }
    public class EventHandlers
    {
        Config config = Plugin.plugin.Config;

        Data data;

        public bool modeStarted = false;
        public void OnRoundStarted()
        {
            data = new Data();
            Log.Info("Starting round");
            if (Player.List.Count() < Plugin.plugin.minPlayers && !modeStarted)
            {
                Log.Info("Not enough players. Mode not started.");
                return;
            }

            Player kicked;
            var scps = Player.Get(Team.SCPs);
            if (scps.Count() > 1)
                kicked = scps.ToList().RandomItem();
            else
            
                kicked = Player.Get(Team.ClassD).ToList().RandomItem();

            if (kicked == null)
            {
                Log.Error("No player for partner found.");
                return;
            }

            kicked.RoleManager.ServerSetRole(RoleTypeId.Spectator, RoleChangeReason.RoundStart);

            data.master = scps.First();
            data.master.RoleManager.ServerSetRole(RoleTypeId.Scp079, RoleChangeReason.RoundStart);

            modeStarted = true;

            Timing.CallDelayed(120f, () => CheckForceSelectSCP(kicked));

            data.master.Broadcast(120,"You have 2 minutes to select your SCP ally. You can do it by opening containment chamber of your desired partner.");

            Log.Info($"Gamemode started. {data.master.DisplayNickname} assigned master, {kicked.DisplayNickname} assigned partner");
        }
        public void OnEndingRound(EndingRoundEventArgs ev)
        {
            if (Round.ElapsedTime < TimeSpan.FromMinutes(2))
            {
                ev.IsRoundEnded = false;
            }
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            data = null;
        }

        public void CheckForceSelectSCP(Player player)
        {
            if (Player.Get(Team.SCPs).Count() >= 2)
                return;

            List<RoleTypeId> SCPs = new List<RoleTypeId>
            {
                RoleTypeId.Scp173,
                RoleTypeId.Scp049,
                RoleTypeId.Scp939
            };
            var scpRole = SCPs.RandomItem();
            
            Log.Info($"{player.Nickname} was freed as as {scpRole}");
            SpawnSCP(player, scpRole);
        }

        public void OnVoiceChatting(VoiceChattingEventArgs ev)
        {
        }
        public void OnDoorInteraction(InteractingDoorEventArgs args)
        {
            //Log.Info($"{args.Player.DisplayNickname} opening door {args.Door.Name}");
            if (!args.IsAllowed)
            {
                //Log.Info($"{args.Player.DisplayNickname} opening door {args.Door.Name} passed: Not Allowed");
                return;
            }

            if (!modeStarted)
            {
                //Log.Info($"{args.Player.DisplayNickname} opening door {args.Door.Name} passed: Gamemode not active");
                return;
            }



            if (!data.doorsDict.ContainsKey(args.Door.Name))
            {
               // Log.Info($"{args.Player.DisplayNickname} opening door {args.Door.Name} passed: Door not in the dict");
                return;
            }


            var door939 = Door.Get("939_CRYO");
            var door049 = Door.Get("049_ARMORY");
            var scpRole = data.doorsDict[args.Door.Name];
            

            if (scpRole == RoleTypeId.Scp939 || scpRole == RoleTypeId.Scp049)
            {
                int stopCount = 0;
                float distanceTo939 = (args.Door.GameObject.transform.position - door939.Transform.position).magnitude;
                if (distanceTo939 > 13f)
                {
                   // Log.Info($"{distanceTo939} meters to 939");
                    stopCount++;

                }
                float distanceTo049 = (args.Door.GameObject.transform.position - door049.Transform.position).magnitude;
                if (distanceTo049 > 10f)
                {
                  //  Log.Info($"{distanceTo049} meters to 049");
                    stopCount++;
                }
              //  Log.Info($"{distanceTo939} meters to 939");
//                Log.Info($"{distanceTo049} meters to 049");
                if (stopCount == 2)
                    return;

            }


            Log.Info($"Looking to spawn {scpRole}");
            var scps = (from p in Player.List.ToList() where p.IsScp select p.Role.Type).ToList();
            if (scps.Contains(scpRole))
            {
                return;
            }

            if (!data.availableScps[scpRole])
            {
                return;
            }


            int scpCount = data.availableScps.Where(i => i.Value == false).Count(); 
            if (scpCount >= 2)
            {
                //Log.Info($"{args.Player.DisplayNickname} opening door {args.Door.Name} passed: Already called two SCPs");
                args.IsAllowed = false;
                return;
            }

            if (scpCount >= 1 && Round.ElapsedTime.TotalMinutes - data.cooldownTime.TotalMinutes < 10.0)
            {
                args.IsAllowed = false;
                return;
            }


            var spectators = from p in Player.List where p.Role == RoleTypeId.Spectator select p;
            if (spectators.Count() == 0)
            {
             //   Log.Info("No spectators.");
                args.IsAllowed = false;
                return;
            }

            var selectedPlayer = spectators.ToList().RandomItem();
            if (args.Player == data.master)
            {
                var cast_master = data.master.Role.As<Exiled.API.Features.Roles.Scp079Role>();
                
                if (scpCount > 1 && cast_master.Level < 3)
                {
                    args.IsAllowed = false;
                    return;
                }

                float requiredEnergy = 80;
                if (scpRole == RoleTypeId.Scp106 || scpRole == RoleTypeId.Scp096)
                    requiredEnergy = 120;
                if (cast_master.Energy > requiredEnergy)
                {
                    cast_master.Energy -= requiredEnergy;
                    cast_master.LoseSignal(3.5f);
                }
                else
                {
                    data.master.Broadcast(2, $"You need {requiredEnergy} energy to call this SCP.", Broadcast.BroadcastFlags.Normal, true);
                    args.IsAllowed = false;
                    return;
                }
            }
            else if (scpRole == RoleTypeId.Scp939 && !Map.IsLczDecontaminated)
            {
                args.IsAllowed = false;
                return;
            }    

            Log.Info($"player {args.Player.Nickname} has freed {selectedPlayer.Nickname} as {scpRole}");
            SpawnSCP(selectedPlayer, scpRole);
        }
        void SpawnSCP(Player player, RoleTypeId role)
        {
            data.cooldownTime = Round.ElapsedTime;
            data.availableScps[role] = false;
            Timing.CallDelayed(2, () => Map.PlayAmbientSound());
            Timing.CallDelayed(3, () => player.RoleManager.ServerSetRole(role, RoleChangeReason.RoundStart));
            Map.TurnOffAllLights(7f);
            var delay = UnityEngine.Random.Range(15, 20f);
            Cassie.DelayedGlitchyMessage($"SCP {role.ToString()[3]} {role.ToString()[4]} {role.ToString()[5]} has breached containment.", delay, 0.65f, 0.86f);
        }
    }
}



