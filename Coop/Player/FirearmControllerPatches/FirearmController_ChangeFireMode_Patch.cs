using EFT.InventoryLogic;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ChangeFireMode_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ChangeFireMode";
        //public override bool DisablePatch => true;

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            //Logger.LogInfo("FirearmController_ChangeFireMode_Patch:PrePatch");

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , Weapon.EFireMode fireMode
            , EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new();
            dictionary.Add("f", fireMode.ToString());
            dictionary.Add("m", "ChangeFireMode");
            AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(player, dictionary);
            //Logger.LogInfo("FirearmController_ChangeFireMode_Patch:PostPatch");

        }

        //private static List<long> ProcessedCalls = new List<long>();

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());
            //if (!ProcessedCalls.Contains(timestamp))
            //    ProcessedCalls.Add(timestamp);
            //else
            //{
            //    ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddHours(-1).Ticks);
            //    return;
            //}
            if (HasProcessed(GetType(), player, dict))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    CallLocally.Add(player.Profile.AccountId, true);
                    if (Enum.TryParse<Weapon.EFireMode>(dict["f"].ToString(), out var firemode))
                    {
                        //Logger.LogInfo("Replicated: Calling Change FireMode");
                        firearmCont.ChangeFireMode(firemode);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
        }
    }
}
