using EFT;
using EFT.Interactive;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using Comfort.Common;

namespace SIT.Core.Coop
{
    internal class WorldInteractiveObject_Interact_Patch : ModulePatch
    {
        public static Type InstanceType => typeof(WorldInteractiveObject);

        public static string MethodName => "WIO_Interact";

        public static void Replicated(Dictionary<string, object> packet)
        {
            Logger.LogDebug("WIO_Interact:Replicated");
            Enum.TryParse<EInteractionType>(packet["type"].ToString(), out EInteractionType interactionType);
            var door = Singleton<GameWorld>.Instance.FindDoor(packet["doorId"].ToString());
            door.Interact(new InteractionResult(interactionType));
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, "Interact", false, false);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return true;
        }

        [PatchPostfix]
        public static void Postfix(WorldInteractiveObject __instance, InteractionResult interactionResult)
        {
            Logger.LogDebug("WIO_Interact:Postfix");

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("doorId", __instance.Id);
            dictionary.Add("type", interactionResult.InteractionType.ToString());
            dictionary.Add("m", "WIO_Interact");
            Request.Instance.SendDataToPool("/coop/server/update", dictionary.SITToJson());
        }
    }
}
