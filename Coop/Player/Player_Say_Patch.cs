using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player
{
    internal class Player_Say_Patch : ModuleReplicationPatch
    {
        public static List<string> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "Say";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.Contains(__instance.Profile.AccountId))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
            object @event
            )
        {
            var player = __instance;

            if (CallLocally.Contains(player.Profile.AccountId))
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("event", @event);
            dictionary.Add("m", "Say");
            ServerCommunication.PostLocalPlayerData(player, dictionary);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (CallLocally.Contains(player.Profile.AccountId))
                return;

            try
            {
                CallLocally.Add(player.Profile.AccountId);
                player.Say((EPhraseTrigger)Enum.Parse(typeof(EPhraseTrigger), dict["event"].ToString()));
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }

        }
    }
}
