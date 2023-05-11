using EFT;
using EFT.Interactive;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;
using System.Linq;
using System.Collections.Concurrent;

namespace SIT.Core.Coop
{
    internal class Door_Interact_Patch : ModulePatch
    {
        public static Type InstanceType => typeof(EFT.Interactive.Door);

        public static string MethodName => "Door_Interact";

        public static List<string> CallLocally = new();

        static ConcurrentBag<long> ProcessedCalls = new ConcurrentBag<long>();

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
            if(HasProcessed(packet))
                return;
            
            //Logger.LogDebug("Door_Interact_Patch:Replicated");
            if (Enum.TryParse<EInteractionType>(packet["type"].ToString(), out EInteractionType interactionType))
            {

                WorldInteractiveObject door;
                door = CoopGameComponent.GetCoopGameComponent().ListOfInteractiveObjects.FirstOrDefault(x => x.Id == packet["doorId"].ToString());
                if (door != null)
                {
                    if (interactionType == EInteractionType.Unlock)
                    {
                        //ReflectionHelpers.GetMethodForType(typeof(WorldInteractiveObject), "Unlock").Invoke(door, new object[] {});
                        ReflectionHelpers.InvokeMethodForObject(door, "Unlock");
                    }
                    else 
                    {
                        CallLocally.Add(packet["doorId"].ToString());
                        door.Interact(new InteractionResult(interactionType));
                    }
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
        public static bool Prefix(EFT.Interactive.Door __instance)
        {
            if(CallLocally.Contains(__instance.Id))
                return true;

            return false;
        }

        [PatchPostfix]
        public static void Postfix(EFT.Interactive.Door __instance, InteractionResult interactionResult)
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

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("serverId", CoopGameComponent.GetServerId());
            dictionary.Add("doorId", __instance.Id);
            dictionary.Add("type", interactionResult.InteractionType.ToString());
            dictionary.Add("m", Door_Interact_Patch.MethodName);

            var packetJson = dictionary.SITToJson();
            //Logger.LogDebug(packetJson);

            Request.Instance.PostJsonAndForgetAsync("/coop/server/update", packetJson);
        }
    }
}
