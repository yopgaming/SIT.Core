using Comfort.Common;
using EFT;
using EFT.Interactive;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.World
{
    internal class WorldInteractiveObject_Interact_Patch : ModulePatch
    {
        public static Type InstanceType => typeof(WorldInteractiveObject);

        public static string MethodName => "WIO_Interact";

        public static void Replicated(Dictionary<string, object> packet)
        {
            Logger.LogDebug("WIO_Interact:Replicated");
            Enum.TryParse(packet["type"].ToString(), out EInteractionType interactionType);
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

            Dictionary<string, object> dictionary = new()
            {
                { "t", DateTime.Now.Ticks },
                { "doorId", __instance.Id },
                { "type", interactionResult.InteractionType.ToString() },
                { "m", "WIO_Interact" }
            };
            AkiBackendCommunication.Instance.SendDataToPool(string.Empty, dictionary);
        }
    }
}
