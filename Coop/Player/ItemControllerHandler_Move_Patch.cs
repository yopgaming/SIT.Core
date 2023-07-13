using EFT.InventoryLogic;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.Player
{
    internal class ItemControllerHandler_Move_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(ItemMovementHandler);

        public override string MethodName => "IC_Move";

        public static List<string> CallLocally = new();

        public static List<string> DisableForPlayer = new();

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated");

            if (HasProcessed(this.GetType(), player, dict))
                return;

            if (DisableForPlayer.Contains(player.ProfileId))
            {
                GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Not receiving item move for replication. Currently Disabled.");
                return;
            }

            var inventoryController = ItemFinder.GetPlayerInventoryController(player);
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated." + inventoryController.GetType());

            if (!ItemFinder.TryFindItem(dict["id"].ToString(), out Item item))
                return;

            //GetGetLogger(typeof(ItemControllerHandler_Move_Patch))(typeof(ItemControllerHandler_Move_Patch)).LogInfo(item);
            if (CallLocally.Contains(player.ProfileId))
                return;

            CallLocally.Add(player.ProfileId);
            ReplicatedGrid(dict, inventoryController, item);

        }

        private static void ReplicatedGrid(Dictionary<string, object> dict, EFT.Player.PlayerInventoryController inventoryController, Item item)
        {
            if (dict.ContainsKey("grad"))
            {
                GridItemAddressDescriptor gridItemAddressDescriptor = PatchConstants.SITParseJson<GridItemAddressDescriptor>(dict["grad"].ToString());
                ItemMovementHandler.Move(item, inventoryController.ToItemAddress(gridItemAddressDescriptor), inventoryController, false);
            }
            else
            {
                ReplicatedSlot(dict, inventoryController, item);
            }
        }

        private static void ReplicatedSlot(Dictionary<string, object> dict, EFT.Player.PlayerInventoryController inventoryController, Item item)
        {
            SlotItemAddressDescriptor slotItemAddressDescriptor = PatchConstants.SITParseJson<SlotItemAddressDescriptor>(dict["sitad"].ToString());
            ItemMovementHandler.Move(item, inventoryController.ToItemAddress(slotItemAddressDescriptor), inventoryController, false);
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, "Move");
        }

        [PatchPostfix]
        public static void Postfix(
            object __instance,
            Item item
            , ItemAddress to
            , ItemController itemController
            , bool simulate = false
            )
        {
            if (simulate)
                return;

            CoopGameComponent coopGameComponent = null;

            if (!CoopGameComponent.TryGetCoopGameComponent(out coopGameComponent))
                return;

            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogInfo("ItemControllerHandler_Move_Patch.Postfix");
            var inventoryController = itemController as EFT.Player.PlayerInventoryController;
            var player = coopGameComponent.Players.First(x => x.Key == inventoryController.Profile.AccountId).Value;

            if (DisableForPlayer.Contains(player.ProfileId))
            {
                GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Not sending item move for replication. Currently Disabled.");
                return;
            }

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            SlotItemAddressDescriptor slotItemAddressDescriptor = new();
            slotItemAddressDescriptor.Container = new();
            slotItemAddressDescriptor.Container.ContainerId = to.Container.ID;
            slotItemAddressDescriptor.Container.ParentId = to.Container.ParentItem != null ? to.Container.ParentItem.Id : null;

            Dictionary<string, object> dictionary = new()
            {
                    { "t", DateTime.Now.Ticks.ToString("G") }
                };

            if (to is GridItemAddress gridItemAddress)
            {
                GridItemAddressDescriptor gridItemAddressDescriptor = new();
                gridItemAddressDescriptor.Container = new();
                gridItemAddressDescriptor.Container.ContainerId = to.Container.ID;
                gridItemAddressDescriptor.Container.ParentId = to.Container.ParentItem != null ? to.Container.ParentItem.Id : null;
                gridItemAddressDescriptor.LocationInGrid = gridItemAddress.LocationInGrid;
                dictionary.Add("grad", gridItemAddressDescriptor);
            }

            dictionary.Add("id", item.Id);
            dictionary.Add("tpl", item.TemplateId);
            dictionary.Add("sitad", slotItemAddressDescriptor);
            dictionary.Add("m", "IC_Move");

            HasProcessed(typeof(ItemControllerHandler_Move_Patch), player, dictionary);

            AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(player, dictionary);
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogInfo("Sent");
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogInfo(dictionary.ToJson());

        }

    }
}
