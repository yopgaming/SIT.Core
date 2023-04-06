using EFT;
using Newtonsoft.Json;
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class Player_DropBackpack_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.LocalPlayer);

        public override string MethodName => "DropBackpack";

        public static Dictionary<string, bool> CallLocally
          = new Dictionary<string, bool>();

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

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            Logger.LogDebug($"PatchPostfix: Current Equipment Item Count {__instance.Profile.Inventory.Equipment.GetAllItems().Count()}");
            dictionary.Add("p.equip", __instance.Profile.Inventory.Equipment.SITToJson());
            dictionary.Add("m", "DropBackpack");
            ServerCommunication.PostLocalPlayerData(__instance, dictionary);

            //var invPacket = __instance.Profile.Inventory.ToInventorySyncPacket();
            //foreach (var item in invPacket.SlotsItemInfo)
            //{
            //    var It = item.ItemJson.SITParseJson<EFT.InventoryLogic.Item>();
            //    Logger.LogDebug($"PatchPostfix: Equipment Item {It.Id}");
            //}
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (dict.ContainsKey("p.equip"))
            {
                // TODO: Spin up a new Equipment using the Equipment Template

                // Equipment from Replication
                var equipment = dict["p.equip"].ToString().SITParseJson<Equipment>();
                Logger.LogDebug($"Replicated: Deserialized Equipment Item Count {equipment.GetAllItems().Count()}");

                // You can get the GClass2267 from the Equipment Constructor
                //var equipmentTemplate = new GClass2267();
                //equipmentTemplate.Slots = equipment.Slots;
                //equipmentTemplate.Grids = equipment.Grids;
                //equipmentTemplate.CanPutIntoDuringTheRaid = true;
                //equipmentTemplate.CantRemoveFromSlotsDuringRaid = new EFT.InventoryLogic.EquipmentSlot[0];

                //var nEquipment = new Equipment(equipment.Id, equipmentTemplate);
                //Logger.LogDebug($"Replicated: New Equipment Item Count {nEquipment.GetAllItems().Count()}");
                //player.Profile.Inventory.Equipment = nEquipment;
                player.Profile.Inventory.Equipment.Grids = equipment.Grids;
               
            }

            CallLocally.Add(player.Profile.AccountId, true);
            Logger.LogDebug("Replicated: Calling Drop Backpack");
            player.DropBackpack();
        }
    }
}
