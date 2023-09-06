using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SIT.Coop.Core.Web;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SIT.Core.Coop.ItemControllerPatches
{
    internal class ItemControllerHandler_Move_Patch : ModuleReplicationPatch, IModuleReplicationWorldPatch
    {
        public override Type InstanceType => typeof(ItemMovementHandler);

        public override string MethodName => "IC_Move";

        public static List<string> CallLocally = new();

        public static List<string> DisableForPlayer = new();

        private ManualLogSource GetLogger()
        {
            return GetLogger(typeof(ItemControllerHandler_Move_Patch));
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (DisableForPlayer.Contains(player.ProfileId))
            {
                GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Not receiving item move for replication. Currently Disabled.");
                return;
            }

            Replicated(ref dict);

        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, "Move");
        }

        [PatchPrefix]
        public static bool Prefix(
            object __instance,
            Item item
            , ItemAddress to
            , ItemController itemController
            , bool simulate = false
            )
        {
            if (simulate)
                return true;

            return true;
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
            var playerInventoryController = itemController as EFT.Player.PlayerInventoryController;
            if (playerInventoryController != null)
            {

                if (!coopGameComponent.Players.Any(x => x.Key == playerInventoryController.Profile.ProfileId))
                {
                    GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError($"Unable to find player of Id {playerInventoryController.Profile.ProfileId} in Raid.");
                    return;
                }

                var player = coopGameComponent.Players.First(x => x.Key == playerInventoryController.Profile.ProfileId).Value;

                if (DisableForPlayer.Contains(player.ProfileId))
                {
                    GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Not sending item move for replication. Currently Disabled.");
                    return;
                }

          
            }

            if (CallLocally.Contains(itemController.ID))
            {
                CallLocally.Remove(itemController.ID);
                return;
            }


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

            if (to is SlotItemAddress slotItemAddress)
            {
                SlotItemAddressDescriptor slotItemAddressDescriptor = new();
                slotItemAddressDescriptor.Container = new();
                slotItemAddressDescriptor.Container.ContainerId = to.Container.ID;
                slotItemAddressDescriptor.Container.ParentId = to.Container.ParentItem != null ? to.Container.ParentItem.Id : null;
                dictionary.Add("sitad", slotItemAddressDescriptor);
            }

            dictionary.Add("id", item.Id);
            dictionary.Add("tpl", item.TemplateId);
            dictionary.Add("icId", itemController.ID);
            dictionary.Add("icCId", itemController.CurrentId);
            dictionary.Add("m", "IC_Move");

            Logger.LogInfo(dictionary.ToJson());
            AkiBackendCommunication.Instance.SendDataToPool(dictionary.ToJson());

        }

        public void Replicated(ref Dictionary<string, object> packet)
        {
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated");

            var itemControllerId = packet["icId"].ToString();
            GetLogger().LogDebug($"Item Controller Id: {itemControllerId}");
            GetLogger().LogDebug($"Item Controller Current Id: {packet["icCId"]}");



            //if (HasProcessed(GetType(), player, dict))
            //    return;

            //var inventoryController = ItemFinder.GetPlayerInventoryController(player);
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated." + inventoryController.GetType());

            var itemId = packet["id"].ToString();
            if (!ItemFinder.TryFindItem(itemId, out Item item))
            {
                GetLogger().LogError("Item not found!");
                return;
            }

            var lootItems = Singleton<GameWorld>.Instance.LootItems.Where(x => x.ItemId == itemId);
            if (lootItems.Any())
            {
                foreach(var i in lootItems)
                {
                    i.Kill();
                }
            }

            //GetGetLogger(typeof(ItemControllerHandler_Move_Patch))(typeof(ItemControllerHandler_Move_Patch)).LogInfo(item);
            if (CallLocally.Contains(itemControllerId))
            {
                GetLogger().LogError($"CallLocally already contains {itemControllerId}");
                return;
            }

            CallLocally.Add(itemControllerId);

            try
            {
                if (packet.ContainsKey("grad"))
                {
                    GetLogger().LogDebug(packet["grad"].ToString());

                    GridItemAddressDescriptor gridItemAddressDescriptor = packet["grad"].ToString().SITParseJson<GridItemAddressDescriptor>();
                    if (!ItemFinder.TryFindItemController(gridItemAddressDescriptor.Container.ParentId, out var itemController))
                    {
                        if (!ItemFinder.TryFindItemController(itemControllerId, out itemController))
                        {
                            GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError("Unable to find ItemController");
                            return;
                        }
                    }

                    ItemMovementHandler.Move(item, itemController.ToItemAddress(gridItemAddressDescriptor), itemController, false, true);
                }

                if (packet.ContainsKey("sitad"))
                {
                    //GetLogger().LogError("sitad has not been handled!");
                    GetLogger().LogInfo(packet["sitad"].ToString());

                    SlotItemAddressDescriptor slotItemAddressDescriptor = packet["sitad"].ToString().SITParseJson<SlotItemAddressDescriptor>();

                    if (!ItemFinder.TryFindItemController(slotItemAddressDescriptor.Container.ParentId, out var itemController))
                    {
                        if (!ItemFinder.TryFindItemController(itemControllerId, out itemController))
                        {
                            GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError("Unable to find ItemController");
                            return;
                        }
                    }

                    ItemMovementHandler.Move(item, itemController.ToItemAddress(slotItemAddressDescriptor), itemController, false, true);
                }

            }
            catch (Exception)
            {

            }
            finally
            {
                CallLocally = CallLocally.Where(x => x != itemControllerId).ToList();
            }

        }
    }
}
