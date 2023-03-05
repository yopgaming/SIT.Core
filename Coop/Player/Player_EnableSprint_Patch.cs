using SIT.Coop.Core.Web;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player
{
    public class Player_EnableSprint_Patch : ModuleReplicationPatch
    {
        private static Dictionary<string, bool> LastSprintEn = new();
        private static List<long> ProcessedCalls = new();
        public static Dictionary<string, bool> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "EnableSprint";

        protected override MethodBase GetTargetMethod()
        {
            var method = PatchConstants.GetMethodForType(InstanceType, MethodName);

            return method;
        }



        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
           bool enable
            )
        {
            var player = __instance;

            if (LastSprintEn.ContainsKey(player.Profile.AccountId))
            {
                if (LastSprintEn[player.Profile.AccountId] == enable)
                    return;
            }


            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("e", enable.ToString());
            dictionary.Add("m", "EnableSprint");
            ServerCommunication.PostLocalPlayerData(player, dictionary);

            if (!LastSprintEn.ContainsKey(player.Profile.AccountId))
                LastSprintEn.Add(player.Profile.AccountId, enable);

            LastSprintEn[player.Profile.AccountId] = enable;
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());

            if (!ProcessedCalls.Contains(timestamp))
                ProcessedCalls.Add(timestamp);
            else
            {
                ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddHours(-1).Ticks);
                return;
            }

            try
            {
                var enable = bool.Parse(dict["e"].ToString());
                CallLocally.Add(player.Profile.AccountId, true);
                player.EnableSprint(enable);
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }
        }
    }
}

