using EFT.InventoryLogic;
using SIT.Coop.Core.Web;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    /// <summary>
    ///  LightAndSoundShot(Vector3 point, Vector3 direction, AmmoTemplate ammoTemplate);
    /// </summary>
    public class FirearmController_LightAndSoundShot_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "LightAndSoundShot";
        //public override bool DisablePatch => true;
        public MethodInfo Method { get; set; } = null;

        protected override MethodBase GetTargetMethod()
        {
            Method = PatchConstants.GetMethodForType(InstanceType, MethodName);
            return Method;
        }

        public static Dictionary<string, bool> CallLocally
            = new Dictionary<string, bool>();


        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            return true;
            //var player = ____player;
            //if (player == null)
            //    return false;

            //var result = false;
            //if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            //    result = true;

            //Logger.LogInfo("FirearmController_LightAndSoundShot_Patch:PrePatch");

            //return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , Vector3 point, Vector3 direction, AmmoTemplate ammoTemplate
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

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("p.x", point.x.ToString());
            dictionary.Add("p.y", point.y.ToString());
            dictionary.Add("p.z", point.z.ToString());
            dictionary.Add("m", "LightAndSoundShot");
            ServerCommunication.PostLocalPlayerData(player, dictionary);
            Logger.LogInfo("FirearmController_LightAndSoundShot_Patch:PostPatch");

        }

        private static List<long> ProcessedCalls = new List<long>();

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

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    CallLocally.Add(player.Profile.AccountId, true);
                    //if (Enum.TryParse<Weapon.EFireMode>(dict["f"].ToString(), out var firemode))
                    //{
                    Logger.LogInfo("Replicated: Calling Change LightAndSoundShot");
                    //Method.Invoke(firearmCont, new object[] { foundDoor, new InteractionResult(interactionType) });
                    //}
                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
        }
    }
}
