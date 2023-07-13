using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player
{
    public class Player_SetInventoryOpened_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "SetInventoryOpened";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new();

        [PatchPrefix]
        public static bool PrePatch(ref EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
           ref EFT.Player __instance,
            ref bool opened
            )
        {
            var player = __instance;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("o", opened.ToString());
            dictionary.Add("m", "SetInventoryOpened");
            AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(player, dictionary);
            //dictionary.Clear();
            //dictionary = null;
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (CallLocally.ContainsKey(player.Profile.AccountId))
                return;

            try
            {
                CallLocally.Add(player.Profile.AccountId, true);
                var opened = Convert.ToBoolean(dict["o"].ToString());
                player.SetInventoryOpened(opened);
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }
        }
    }
}

