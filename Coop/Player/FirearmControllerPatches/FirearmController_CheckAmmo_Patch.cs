using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_CheckAmmo_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "CheckAmmo";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.ContainsKey(player.ProfileId))
                result = true;


            //Logger.LogInfo($"CheckAmmo_prefix:{result}");


            return result;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player.FirearmController __instance)
        {
            //Logger.LogInfo($"CheckAmmo_postfix");

            var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return;

            if (CallLocally.ContainsKey(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            Dictionary<string, object> dictionary = new();
            dictionary.Add("m", "CheckAmmo");
            AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(player, dictionary);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //Logger.LogInfo($"CheckAmmo_replicated");

            var timestamp = long.Parse(dict["t"].ToString());
            if (HasProcessed(GetType(), player, dict))
            {
                //Logger.LogInfo($"CheckAmmo_hasprocessed!");
                return;
            }

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                //Logger.LogInfo($"CheckAmmo_calllocally!");

                CallLocally.Add(player.ProfileId, true);
                //Logger.LogInfo("FirearmControllerCheckAmmoPatch:Replicated:CheckAmmo");
                firearmCont.CheckAmmo();
            }
        }
    }
}
