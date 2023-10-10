using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Player;
using SIT.Core.Coop.World;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.Components
{
    public class ActionPacketHandlerComponent : MonoBehaviour
    {
        public BlockingCollection<Dictionary<string, object>> ActionPackets { get; } = new BlockingCollection<Dictionary<string, object>>();
        public BlockingCollection<Dictionary<string, object>> ActionPacketsMovement { get; private set; } = new();
        public ConcurrentDictionary<string, EFT.Player> Players => CoopGameComponent.Players;
        public ManualLogSource Logger { get; private set; }

        private List<string> RemovedFromAIPlayers = new();
        
        private CoopGame CoopGame { get; } = (CoopGame)Singleton<AbstractGame>.Instance;

        private CoopGameComponent CoopGameComponent { get; set; }

        void Awake()
        {
            // ----------------------------------------------------
            // Create a BepInEx Logger for ActionPacketHandlerComponent
            Logger = BepInEx.Logging.Logger.CreateLogSource("ActionPacketHandlerComponent");
            Logger.LogDebug("Awake");

            CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<CoopGameComponent>(); 
            ActionPacketsMovement = new();
        }

        void Start()
        {
            CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<CoopGameComponent>();
            ActionPacketsMovement = new();
        }

        void Update()
        {
            ProcessActionPackets();
        }


        public static ActionPacketHandlerComponent GetThisComponent()
        {
            if (CoopPatches.CoopGameComponentParent == null)
                return null;

            if (CoopPatches.CoopGameComponentParent.TryGetComponent<ActionPacketHandlerComponent>(out var component))
                return component;

            return null;
        }

        private void ProcessActionPackets()
        {
            if (CoopGameComponent == null)
            {
                if (CoopPatches.CoopGameComponentParent != null)
                {
                    CoopGameComponent = CoopPatches.CoopGameComponentParent.GetComponent<CoopGameComponent>();
                    if (CoopGameComponent == null)
                        return;
                }
            }

            if (Singleton<GameWorld>.Instance == null)
                return;

            if (ActionPackets == null)
                return;

            if (Players == null)
                return;

            if (ActionPackets.Count > 0)
            {
                while (ActionPackets.TryTake(out var result))
                {
                    ProcessLastActionDataPacket(result);
                }
            }

            if (ActionPacketsMovement == null)
                return;

            if (ActionPacketsMovement.Count > 0)
            {
                while (ActionPacketsMovement.TryTake(out var result))
                {
                    ProcessLastActionDataPacket(result);
                }
            }

            return;
        }

        void ProcessLastActionDataPacket(Dictionary<string, object> packet)
        {
            if (Singleton<GameWorld>.Instance == null)
                return;

            if (packet == null || packet.Count == 0)
            {
                Logger.LogInfo("No Data Returned from Last Actions!");
                return;
            }

            if (!ProcessPlayerPacket(packet))
            {
                ProcessWorldPacket(ref packet);
            }

        }

        void ProcessWorldPacket(ref Dictionary<string, object> packet)
        {
            if (packet.ContainsKey("profileId"))
                return;

            if (!packet.ContainsKey("m"))
                return;

            foreach (var coopPatch in CoopPatches.NoMRPPatches)
            {
                var imrwp = coopPatch as IModuleReplicationWorldPatch;
                if (imrwp != null)
                {
                    if (imrwp.MethodName == packet["m"].ToString())
                    {
                        imrwp.Replicated(ref packet);
                    }
                }
            }



            switch (packet["m"].ToString())
            {
                case "WIO_Interact":
                    WorldInteractiveObject_Interact_Patch.Replicated(packet);
                    break;
                case "Door_Interact":
                    Door_Interact_Patch.Replicated(packet);
                    break;
                case "Switch_Interact":
                    Switch_Interact_Patch.Replicated(packet);
                    break;

            }
        }


        bool ProcessPlayerPacket(Dictionary<string, object> packet)
        {

            if (packet == null)
                return false;

            if (!packet.ContainsKey("profileId"))
                return false;

            var profileId = packet["profileId"].ToString();

            if (Players == null)
            {
                Logger.LogDebug("Players is Null");
                return false;
            }

            if (Players.Count == 0)
            {
                Logger.LogDebug("Players is Empty");
                return false;
            }

            var profilePlayers = Players.Where(x => x.Key == profileId && x.Value != null).ToArray();
            bool processed = false;

            foreach (var plyr in profilePlayers)
            {
                if (plyr.Value.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                {
                    prc.ProcessPacket(packet);
                    processed = true;
                }
                else
                {
                    Logger.LogError($"Player {profileId} doesn't have a PlayerReplicatedComponent!");
                }

                if (packet.ContainsKey("Extracted"))
                {
                    if (CoopGame != null)
                    {
                        //Logger.LogInfo($"Received Extracted ProfileId {packet["profileId"]}");
                        if (!CoopGame.ExtractedPlayers.Contains(packet["profileId"].ToString()))
                            CoopGame.ExtractedPlayers.Add(packet["profileId"].ToString());

                        if (!MatchmakerAcceptPatches.IsClient)
                        {
                            var botController = (BotControllerClass)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(BaseLocalGame<GamePlayerOwner>), typeof(BotControllerClass)).GetValue(Singleton<AbstractGame>.Instance);
                            if (botController != null)
                            {
                                if (!RemovedFromAIPlayers.Contains(plyr.Key))
                                {
                                    RemovedFromAIPlayers.Add(plyr.Key);
                                    Logger.LogDebug("Removing Client Player to Enemy list");
                                    var botSpawner = (BotSpawner)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(BotControllerClass), typeof(BotSpawner)).GetValue(botController);
                                    botSpawner.DeletePlayer(plyr.Value);
                                }
                            }
                        }
                    }

                    processed = true;
                }
            }

            return processed;
        }

        async Task WaitForPlayerAndProcessPacket(string profileId, Dictionary<string, object> packet)
        {
            // Start the timer.
            var startTime = DateTime.Now;
            var maxWaitTime = TimeSpan.FromMinutes(2);

            while (true)
            {
                // Check if maximum wait time has been reached.
                if (DateTime.Now - startTime > maxWaitTime)
                {
                    Logger.LogError($"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")}: WaitForPlayerAndProcessPacket waited for {maxWaitTime.TotalMinutes} minutes, but player {profileId} still did not exist after timeout period.");
                    return;
                }

                if (Players == null)
                    continue;

                var registeredPlayers = Singleton<GameWorld>.Instance.RegisteredPlayers;

                // If the player now exists, process the packet and end the thread.
                if (Players.Any(x => x.Key == profileId) || registeredPlayers.Any(x => x.Profile.ProfileId == profileId))
                {
                    // Logger.LogDebug($"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff")}: WaitForPlayerAndProcessPacket waited for {(DateTime.Now - startTime).TotalSeconds}s");
                    ProcessPlayerPacket(packet);
                    return;
                }

                // Wait for a short period before checking again.
                await Task.Delay(1000);
            }
        }
    }
}
