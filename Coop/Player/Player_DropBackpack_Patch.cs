using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class Player_DropBackpack_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.LocalPlayer);

        public override string MethodName => "DropBackpack";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            Logger.LogDebug("Player_DropBackpack_Patch:PrePatch");

            return result;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            EFT.Player __instance)
        {
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(__instance.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new()
            {
                { "t", DateTime.Now.Ticks.ToString("G") },
                { "m", "DropBackpack" }
            };
            AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(__instance, dictionary);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            CallLocally.Add(player.Profile.AccountId, true);
            Logger.LogDebug("Replicated: Calling Drop Backpack");
            player.DropBackpack();
        }
    }
}
