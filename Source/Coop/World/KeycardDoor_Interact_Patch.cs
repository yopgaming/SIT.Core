using EFT;
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
    internal class KeycardDoor_Interact_Patch : ModulePatch
    {
        public static Type InstanceType => typeof(KeycardDoor);

        public static string MethodName => "KeycardDoor_Interact";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(InstanceType).FirstOrDefault(x => x.Name == "Interact" && x.GetParameters().Length == 1 && x.GetParameters()[0].Name == "interactionResult");
        }

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

        [PatchPrefix]
        public static bool Prefix(KeycardDoor __instance)
        {
            return false;
        }

        [PatchPostfix]
        public static void Postfix(KeycardDoor __instance, InteractionResult interactionResult)
        {
            Dictionary<string, object> packet = new()
            {
                { "t", DateTime.Now.Ticks.ToString("G") },
                { "serverId", CoopGameComponent.GetServerId() },
                { "m", MethodName },
                { "keycardDoorId", __instance.Id },
                { "type", interactionResult.InteractionType.ToString() }
            };

            if (__instance.InteractingPlayer != null)
                packet.Add("player", __instance.InteractingPlayer.ProfileId);

            if (interactionResult.InteractionType == EInteractionType.Unlock)
                if (interactionResult is GClass2761 keyInteractionResult)
                    packet.Add("succeed", keyInteractionResult.Succeed.ToString());

            AkiBackendCommunication.Instance.PostDownWebSocketImmediately(packet);
        }

        public static async void Replicated(Dictionary<string, object> packet)
        {
            Logger.LogInfo("KeycardDoor_Interact_Patch:Replicated");

            if (HasProcessed(packet))
                return;

            if (Enum.TryParse(packet["type"].ToString(), out EInteractionType interactionType))
            {
                CoopGameComponent coopGameComponent = CoopGameComponent.GetCoopGameComponent();
                KeycardDoor keycardDoor = coopGameComponent.ListOfInteractiveObjects.FirstOrDefault(x => x.Id == packet["keycardDoorId"].ToString()) as KeycardDoor;

                if (keycardDoor != null)
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
                            methodName = "Breath";
                            break;
                        case EInteractionType.Lock:
                            methodName = "Lock";
                            break;
                    }

                    EFT.Player player = null;
                    if (packet.ContainsKey("player"))
                    {
                        player = Comfort.Common.Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(packet["player"].ToString());
                        if (player != null)
                        {
                            if (!coopGameComponent.HighPingMode && !player.IsYourPlayer)
                            {
                                if (SIT.Coop.Core.Matchmaker.MatchmakerAcceptPatches.IsClient || coopGameComponent.PlayerUsers.Contains(player))
                                {
                                    WorldInteractiveObject.InteractionParameters interactionParameters = keycardDoor.GetInteractionParameters(player.Transform.position);
                                    player.SendHandsInteractionStateChanged(true, interactionParameters.AnimationId);
                                    player.HandsController.Interact(true, interactionParameters.AnimationId);
                                }
                            }
                        }
                    }

                    if (methodName == "Unlock" && packet.ContainsKey("succeed"))
                    {
                        bool succeed = bool.Parse(packet["succeed"].ToString());
                        if (!succeed)
                        {
                            await System.Threading.Tasks.Task.Delay(500);

                            GripPose gripPose = keycardDoor.GetClosestGrip(player.Transform.position);
                            MonoBehaviourSingleton<BetterAudio>.Instance.PlayAtPoint((gripPose != null) ? gripPose.transform.position : keycardDoor.transform.position, keycardDoor.DeniedBeep, FPSCamera.Instance.Distance(keycardDoor.transform.position), BetterAudio.AudioSourceGroupType.Environment, 15, 0.7f, EOcclusionTest.Fast, null, false);

                            InteractiveProxy[] proxies = keycardDoor.Proxies;
                            for (int i = 0; i < proxies.Length; i++)
                                proxies[i].StartFlicker();

                            keycardDoor.DoorState = EDoorState.Locked;
                        }
                        else
                        {
                            ReflectionHelpers.InvokeMethodForObject(keycardDoor, methodName);
                        }
                    }
                    else
                    {
                        ReflectionHelpers.InvokeMethodForObject(keycardDoor, methodName);
                    }
                }
                else
                {
                    Logger.LogDebug("KeycardDoor_Interact_Patch:Replicated: Couldn't find KeycardDoor in at all in world?");
                }
            }
            else
            {
                Logger.LogError("KeycardDoor_Interact_Patch:Replicated:EInteractionType did not parse correctly!");
            }
        }
    }
}