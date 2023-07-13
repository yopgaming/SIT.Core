using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    internal class FirearmController_Pickup_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "FCPickup";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "Pickup", false, true);
            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            )
        {
            //Logger.LogInfo("FirearmController_Pickup_Patch.PrePatch");

            if (CoopGameComponent.GetCoopGameComponent() == null)
                return false;

            if (CoopGameComponent.GetCoopGameComponent().HighPingMode && ____player.IsYourPlayer)
            {
                return true;
            }

            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            , bool p)
        {
            var player = ____player;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            FCPickupPicket pickupPicket = new(player.Profile.AccountId, p);
            AkiBackendCommunication.Instance.SendDataToPool(pickupPicket.Serialize());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogInfo("FirearmController_Pickup_Patch.Replicated");

            if (CoopGameComponent.GetCoopGameComponent().HighPingMode && player.IsYourPlayer)
            {
                // You would have already run this. Don't bother
                return;
            }

            FCPickupPicket pp = new(null, false);

            if (dict.ContainsKey("data"))
            {
                pp = pp.DeserializePacketSIT(dict["data"].ToString());
            }

            if (HasProcessed(GetType(), player, pp))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                CallLocally.Add(player.Profile.AccountId, true);
                firearmCont.Pickup(pp.Pickup);
            }
        }

        public class FCPickupPicket : BasePlayerPacket
        {
            public bool Pickup { get; set; }

            public FCPickupPicket(string accountId, bool pickup)
            {
                AccountId = accountId;
                Pickup = pickup;
                Method = "FCPickup";
            }
        }
    }
}
