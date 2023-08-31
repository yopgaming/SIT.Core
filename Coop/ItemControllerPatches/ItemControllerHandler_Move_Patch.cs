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

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (DisableForPlayer.Contains(player.ProfileId))
            {
                GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Not receiving item move for replication. Currently Disabled.");
                return;
            }

            Replicated(dict);

        }

        ///// <summary>
        ///// TODO: This method can error if it cant find the item address (needs further improvement)
        ///// </summary>
        ///// <param name="dict"></param>
        ///// <param name="inventoryController"></param>
        ///// <param name="item"></param>
        //private static void ReplicatedGrid(Dictionary<string, object> dict, EFT.Player.PlayerInventoryController inventoryController, Item item)
        //{
        //    if (!dict.ContainsKey("grad"))
        //        return;

        //    if (item == null)
        //    {
        //        GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError("Item is NULL");
        //        return;
        //    }

        //    Logger.LogDebug(dict["grad"].ToString());

        //    GridItemAddressDescriptor gridItemAddressDescriptor = dict["grad"].ToString().SITParseJson<GridItemAddressDescriptor>();

        //    try
        //    {
        //        // If container exists on this player / inventory controller
        //        if (
        //            gridItemAddressDescriptor != null
        //            && gridItemAddressDescriptor.Container != null
        //            && inventoryController != null
        //            && inventoryController.Inventory != null
        //            && inventoryController.Inventory.Equipment != null
        //            && inventoryController.Inventory.Equipment
        //                .FindContainer(
        //                gridItemAddressDescriptor.Container.ContainerId
        //                , gridItemAddressDescriptor.Container.ParentId)
        //                != null
        //            )
        //        {
        //            GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Moving item to/in Player");

        //            ItemMovementHandler.Move(item, inventoryController.ToItemAddress(gridItemAddressDescriptor), inventoryController, false, false);
        //        }
        //        else
        //        {
        //            // TODO: This is bad. Need to somehow avoid this loop.
        //            // Check whether this is on a player
        //            foreach (var player in CoopGameComponent.GetCoopGameComponent().Players.Values) 
        //            {
        //                var containerOnPlayer = player.Inventory.Equipment.FindContainer(gridItemAddressDescriptor.Container.ContainerId, gridItemAddressDescriptor.Container.ParentId);
        //                if(containerOnPlayer != null)
        //                {
        //                    GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Found player to place item");
        //                    var playerInventoryController = ItemFinder.GetPlayerInventoryController(player);
        //                    if(playerInventoryController != null)
        //                        ItemMovementHandler.Move(item, playerInventoryController.ToItemAddress(gridItemAddressDescriptor), playerInventoryController, false, false);

        //                    return;
        //                }
        //            }

        //            // This must be placing an item into a world container. Lets find out where.
        //            // Find the Controller by the ParentId of the Container
        //            var contrByParentId = Singleton<GameWorld>.Instance.FindControllerById(gridItemAddressDescriptor.Container.ParentId);
        //            if (contrByParentId != null)
        //                GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Found ItemController by Parent Id");

        //            if (contrByParentId == null)
        //            {
        //                GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError($"Could not find ItemController by Parent Id");
        //                return;
        //            }

        //            var itemAddress = contrByParentId.ToItemAddress(gridItemAddressDescriptor);
        //            if (itemAddress == null)
        //            {
        //                GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError($"Could not find Item Address in {contrByParentId.ContainerName}");
        //                return;
        //            }

        //            // Move the item to the Controller to the Item Address in the Descriptor
        //            ItemMovementHandler.Move(item, contrByParentId.ToItemAddress(gridItemAddressDescriptor), contrByParentId, false, true);

        //            //contrByParentId.Add(item, contrByParentId.ToItemAddress(gridItemAddressDescriptor));

        //            //var itemOwnerById = Singleton<GameWorld>.Instance.FindOwnerById(gridItemAddressDescriptor.Container.ParentId);
        //            //if (itemOwnerById != null)
        //            //    GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Found ItemOwner by Parent Id");

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError($"An error occurred in ReplicatedGrid with the Message {ex.Message}");
        //        GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError($"{ex.ToString()}");
        //    }
        //}

        ///// <summary>
        ///// TODO: This method can error if it cant find the item address (needs further improvement)
        ///// </summary>
        ///// <param name="dict"></param>
        ///// <param name="inventoryController"></param>
        ///// <param name="item"></param>
        //private static void ReplicatedSlot(Dictionary<string, object> dict, EFT.Player.PlayerInventoryController inventoryController, Item item)
        //{
        //    if (!dict.ContainsKey("sitad"))
        //        return;

        //    Logger.LogDebug(dict["sitad"].ToString());

        //    SlotItemAddressDescriptor slotItemAddressDescriptor = dict["sitad"].ToString().SITParseJson<SlotItemAddressDescriptor>();

        //    try
        //    {
        //        // If slot exists on this player / inventory controller
        //        if (
        //            slotItemAddressDescriptor.Container != null
        //            && inventoryController.Inventory.Equipment.FindContainer(
        //                slotItemAddressDescriptor.Container.ContainerId, slotItemAddressDescriptor.Container.ParentId)
        //             != null
        //            )
        //            ItemMovementHandler.Move(item, inventoryController.ToItemAddress(slotItemAddressDescriptor), inventoryController, false);
        //    }
        //    catch (Exception ex)
        //    {
        //        GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug($"An error occurred in ReplicatedSlot with the Message {ex.Message}");
        //    }
        //}

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
            //AkiBackendCommunicationCoop.PostLocalPlayerData(player, dictionary);
            //AkiBackendCommunicationCoop.PostLocalPlayerData(coopGameComponent.Players.First().Value, dictionary);
            AkiBackendCommunication.Instance.SendDataToPool(dictionary.ToJson());

        }

        public void Replicated(Dictionary<string, object> packet)
        {
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated");

            var itemControllerId = packet["icId"].ToString();
            GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug($"Item Controller Id: {itemControllerId}");
            GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug($"Item Controller Current Id: {packet["icCId"]}");



            //if (HasProcessed(GetType(), player, dict))
            //    return;

            //var inventoryController = ItemFinder.GetPlayerInventoryController(player);
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated." + inventoryController.GetType());

            if (!ItemFinder.TryFindItem(packet["id"].ToString(), out Item item))
            {
                GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError("Item not found!");
                return;
            }

            //GetGetLogger(typeof(ItemControllerHandler_Move_Patch))(typeof(ItemControllerHandler_Move_Patch)).LogInfo(item);
            if (CallLocally.Contains(itemControllerId))
            {
                GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogError($"CallLocally already contains {itemControllerId}");
                return;
            }

            CallLocally.Add(itemControllerId);

            //if(!ItemFinder.TryFindItemController(itemControllerId, out var itemController))
            //{
            //    GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("Unable to find Item Controller");
            //    return;
            //}

            try
            {
                if (packet.ContainsKey("grad"))
                {
                    GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug(packet["grad"].ToString());

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

                //if (dict.ContainsKey("sitad"))
                //{
                //    GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug(dict["sitad"].ToString());

                //    SlotItemAddressDescriptor slotItemAddressDescriptor = dict["sitad"].ToString().SITParseJson<SlotItemAddressDescriptor>();

                //    ItemMovementHandler.Move(item, itemController.ToItemAddress(slotItemAddressDescriptor), itemController, false, true);
                //}

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
