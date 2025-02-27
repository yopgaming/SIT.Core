﻿using EFT;
using EFT.Interactive;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.World
{
    internal class Door_Interact_Patch : ModulePatch
    {
        public static Type InstanceType => typeof(Door);

        public static string MethodName => "Door_Interact";

        public static List<string> CallLocally = new();

        static ConcurrentBag<long> ProcessedCalls = new();

        protected static bool HasProcessed(Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());

            if (!ProcessedCalls.Contains(timestamp))
            {
                ProcessedCalls.Add(timestamp);
                return false;
            }

            return true;
        }

        public static void Replicated(Dictionary<string, object> packet)
        {
            if (HasProcessed(packet))
                return;

            if (Enum.TryParse(packet["type"].ToString(), out EInteractionType interactionType))
            {
                CoopGameComponent coopGameComponent = CoopGameComponent.GetCoopGameComponent();

                WorldInteractiveObject door = coopGameComponent.ListOfInteractiveObjects.FirstOrDefault(x => x.Id == packet["doorId"].ToString());
                if (door != null)
                {
                    string methodName = string.Empty;
                    switch (interactionType)
                    {
                        case EInteractionType.Open:
                            methodName = "Open";
                            break;
                        case EInteractionType.Close:
                            methodName = "Close";
                            break;
                        case EInteractionType.Unlock:
                            methodName = "Unlock";
                            break;
                        case EInteractionType.Breach:
                            methodName = "Breach";
                            break;
                        case EInteractionType.Lock:
                            methodName = "Lock";
                            break;
                    }

                    string profileId = packet["player"].ToString();

                    if (!profileId.IsNullOrEmpty())
                    {
                        EFT.Player player = Comfort.Common.Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(profileId);
                        if (player != null)
                        {
                            if (!coopGameComponent.HighPingMode && !player.IsYourPlayer)
                            {
                                if (SIT.Coop.Core.Matchmaker.MatchmakerAcceptPatches.IsClient || coopGameComponent.PlayerUsers.Contains(player))
                                {
                                    WorldInteractiveObject.InteractionParameters interactionParameters = door.GetInteractionParameters(player.Transform.position);
                                    player.SendHandsInteractionStateChanged(true, interactionParameters.AnimationId);
                                    player.HandsController.Interact(true, interactionParameters.AnimationId);
                                }
                            }
                        }
                    }

                    ReflectionHelpers.InvokeMethodForObject(door, methodName);
                }
                else
                {
                    Logger.LogDebug("Door_Interact_Patch:Replicated: Couldn't find Door in at all in world?");
                }


            }
            else
            {
                Logger.LogError("Door_Interact_Patch:Replicated:EInteractionType did not parse correctly!");
            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(InstanceType)
                .FirstOrDefault(x => x.Name == "Interact" && x.GetParameters().Length == 1 && x.GetParameters()[0].Name == "interactionResult");
        }

        [PatchPrefix]
        public static bool Prefix(Door __instance)
        {
            if (CallLocally.Contains(__instance.Id))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void Postfix(Door __instance, InteractionResult interactionResult)
        {
            if (CallLocally.Contains(__instance.Id))
            {
                CallLocally.Remove(__instance.Id);
                return;
            }

            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC == null)
                return;

            //Logger.LogDebug($"Door_Interact_Patch:Postfix:Door Id:{__instance.Id}");

            Dictionary<string, object> packet = new()
            {
                { "t", DateTime.Now.Ticks.ToString("G") },
                { "serverId", CoopGameComponent.GetServerId() },
                { "doorId", __instance.Id },
                { "player", __instance.InteractingPlayer.ProfileId },
                { "type", interactionResult.InteractionType.ToString() },
                { "m", MethodName }
            };

            //var packetJson = packet.SITToJson();
            //Logger.LogDebug(packetJson);

            //Request.Instance.PostJsonAndForgetAsync("/coop/server/update", packetJson);
            AkiBackendCommunication.Instance.PostDownWebSocketImmediately(packet);
        }
    }
}
