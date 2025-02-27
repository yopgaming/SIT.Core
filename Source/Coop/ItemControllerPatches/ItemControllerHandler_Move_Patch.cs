﻿using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.InventoryLogic;
using SIT.Coop.Core.Web;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Concurrent;
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

        public static HashSet<string> CallLocally = new();

        public static HashSet<string> DisableForPlayer = new();

        public static HashSet<string> AlreadySent = new();

        private ManualLogSource GetLogger()
        {
            return GetLogger(typeof(ItemControllerHandler_Move_Patch));
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (DisableForPlayer.Contains(player.ProfileId))
            {
                GetLogger().LogDebug("Not receiving item move for replication. Currently Disabled.");
                return;
            }

            Replicated(ref dict);

        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, "Move");
        }

        public override void Enable()
        {
            base.Enable();

            GetLogger().LogDebug("Clearing cached data");
            AlreadySent.Clear();
            CallLocally.Clear();
            DisableForPlayer.Clear();

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

            if (to is StackSlotItemAddress stackSlotItemAddress)
            {
                StackSlotItemAddressDescriptor stackSlotItemAddressDescriptor = new();
                stackSlotItemAddressDescriptor.Container = new();
                stackSlotItemAddressDescriptor.Container.ContainerId = to.Container.ID;
                stackSlotItemAddressDescriptor.Container.ParentId = to.Container.ParentItem != null ? to.Container.ParentItem.Id : null;
                dictionary.Add("ssad", stackSlotItemAddressDescriptor);
            }

            dictionary.Add("id", item.Id);
            dictionary.Add("tpl", item.TemplateId);
            dictionary.Add("icId", itemController.ID);
            dictionary.Add("icCId", itemController.CurrentId);
            dictionary.Add("m", "IC_Move");

            var json = dictionary.ToJson();
            if (AlreadySent.Contains(json))
                return;

            Logger.LogDebug(json);
            AkiBackendCommunication.Instance.SendDataToPool(json);
            AlreadySent.Add(json);
        }



        public void Replicated(ref Dictionary<string, object> packet)
        {
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated");

            var itemControllerId = packet["icId"].ToString();
            GetLogger().LogDebug($"Item Controller Id: {itemControllerId}");
            GetLogger().LogDebug($"Item Controller Current Id: {packet["icCId"]}");

            var itemId = packet["id"].ToString();
            if (!ItemFinder.TryFindItem(itemId, out Item item))
            {
                GetLogger().LogError("Item not found!");
                return;
            }

            // -----------------------------------------------------------------------------------
            // Destroy the Loose Item if it exists
            //
            var lootItems = Singleton<GameWorld>.Instance.LootItems.Where(x => x.ItemId == itemId);
            if (lootItems.Any())
            {
                foreach(var i in lootItems)
                {
                    i.Kill();
                }
            }
            //
            // -----------------------------------------------------------------------------------

            //GetGetLogger(typeof(ItemControllerHandler_Move_Patch))(typeof(ItemControllerHandler_Move_Patch)).LogInfo(item);


            try
            {
                ItemController itemController = null;
                ItemAddress address = null;
                AbstractDescriptor descriptor = null;
                if (packet.ContainsKey("grad"))
                {
                    GetLogger().LogDebug(packet["grad"].ToString());
                    descriptor = packet["grad"].ToString().SITParseJson<GridItemAddressDescriptor>();
                }

                if (packet.ContainsKey("sitad"))
                {
                    GetLogger().LogInfo(packet["sitad"].ToString());
                    descriptor = packet["sitad"].ToString().SITParseJson<SlotItemAddressDescriptor>();

                }

                if (packet.ContainsKey("ssad"))
                {
                    GetLogger().LogError("ssad has not been handled!");
                    GetLogger().LogInfo(packet["ssad"].ToString());

                    descriptor = packet["ssad"].ToString().SITParseJson<StackSlotItemAddressDescriptor>();
                }

                if (descriptor == null)
                {
                    GetLogger().LogError($"Unable to find Descriptor for {item.Id}");
                    return;
                }

                if (!ItemFinder.TryFindItemController(descriptor.Container.ParentId, out itemController))
                {
                    if (!ItemFinder.TryFindItemController(itemControllerId, out itemController))
                    {
                        if (!ItemFinder.TryFindItemController(Singleton<GameWorld>.Instance.MainPlayer.ProfileId, out itemController))
                        {
                            GetLogger().LogError("Unable to find ItemController");
                            return;
                        }
                    }
                }

                if (itemController == null)
                {
                    GetLogger().LogError($"Unable to find Item Controller for {item.Id}");
                    return;
                }

                address = itemController.ToItemAddress(descriptor);

                if (address == null)
                {
                    GetLogger().LogError($"Unable to find Address for {item.Id}");
                    return;
                }

                if (CallLocally.Contains(itemControllerId))
                {
                    GetLogger().LogError($"CallLocally already contains {itemControllerId}");
                    return;
                }

                CallLocally.Add(itemController.ID);

                ItemMovementHandler.Move(item, address, itemController, false);

            }
            catch (Exception)
            {

            }
            finally
            {
                CallLocally.RemoveWhere(x => x != itemControllerId);
            }

        }
    }
}
