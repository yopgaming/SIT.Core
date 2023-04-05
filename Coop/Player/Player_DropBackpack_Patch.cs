using Newtonsoft.Json;
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

        public static Dictionary<string, bool> CallLocally
          = new Dictionary<string, bool>();

        private static List<long> ProcessedCalls = new List<long>();

        public override bool DisablePatch => base.DisablePatch;

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            //Logger.LogInfo($"PlayerOnHealPatch:{t.Name}:{method.Name}");
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            Logger.LogInfo("Player_DropBackpack_Patch:PrePatch");

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

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("p.equip", JsonConvert.SerializeObject(__instance.Profile.Inventory.Equipment, PatchConstants.GetJsonSerializerSettings()));
            dictionary.Add("m", "DropBackpack");
            ServerCommunication.PostLocalPlayerData(__instance, dictionary);
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

            if (dict.ContainsKey("p.equip"))
            {
                //player.Profile.Inventory.Equipment = dict["p.equip"].ToString().SITParseJson<Equipment>();
            }

            CallLocally.Add(player.Profile.AccountId, true);
            Logger.LogInfo("Replicated: Calling Drop Backpack");
            player.DropBackpack();
        }
    }
}
