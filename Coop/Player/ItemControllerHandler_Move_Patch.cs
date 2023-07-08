using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Player
{
    internal class ItemControllerHandler_Move_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(GClass2669);

        public override string MethodName => "IC_Move";

        public static List<string> CallLocally = new();

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated");

            if (HasProcessed(this.GetType(), player, dict))
                return;

            var inventoryController = ItemFinder.GetPlayerInventoryController(player);
            //GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug("ItemControllerHandler_Move_Patch.Replicated." + inventoryController.GetType());

            //if (inventoryController is SinglePlayerInventoryController singlePlayerInventoryController)
            {

                Item item = null;
                var itemFindResult = Singleton<GameWorld>.Instance.FindItemById(dict["id"].ToString());
                if (itemFindResult.Succeeded)
                {
                    item = itemFindResult.Value;

                    if (item.CurrentAddress == null || item.CurrentAddress.Container == null)
                    {
                        GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug($"Item of Id {item.Id} isn't in a box");
                    }
                }
                else
                {
                    GetLogger(typeof(ItemControllerHandler_Move_Patch)).LogDebug($"Unable to find Item Id:{dict["id"]} in world. Attempting to find on Player.");
                    ItemFinder.TryFindItemOnPlayer(player, dict["id"].ToString(), dict["tpl"].ToString(), out item);
                }

                if (item == null)
                {
                    return;
                }

                //GetGetLogger(typeof(ItemControllerHandler_Move_Patch))(typeof(ItemControllerHandler_Move_Patch)).LogInfo(item);
                if (CallLocally.Contains(player.ProfileId))
                    return;

                CallLocally.Add(player.ProfileId);

                if (dict.ContainsKey("grad"))
                {
                    GridItemAddressDescriptor gridItemAddressDescriptor = PatchConstants.SITParseJson<GridItemAddressDescriptor>(dict["grad"].ToString());
                    GClass2669.Move(item, inventoryController.ToItemAddress(gridItemAddressDescriptor), inventoryController, false);
                }
                else
                {
                    SlotItemAddressDescriptor slotItemAddressDescriptor = PatchConstants.SITParseJson<SlotItemAddressDescriptor>(dict["sitad"].ToString());
                    GClass2669.Move(item, inventoryController.ToItemAddress(slotItemAddressDescriptor), inventoryController, false);
                }
            }

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
            var inventoryController = itemController as PlayerInventoryController;
                var player = coopGameComponent.Players.First(x => x.Key == inventoryController.Profile.AccountId).Value;

                if (CallLocally.Contains(player.ProfileId))
                {
                    CallLocally.Remove(player.ProfileId);
                    return;
                }

                SlotItemAddressDescriptor slotItemAddressDescriptor = new();
                slotItemAddressDescriptor.Container = new();
                slotItemAddressDescriptor.Container.ContainerId = to.Container.ID;
                slotItemAddressDescriptor.Container.ParentId = to.Container.ParentItem != null ? to.Container.ParentItem.Id : null;

                Dictionary<string, object> dictionary = new Dictionary<string, object>
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

        //[PatchPrefix]
        //public static bool Prefix(
        //     object __instance,
        //    Item item
        //    , ItemAddress to
        //    , ItemController itemController
        //    , bool simulate = false)
        //{
        //    if (simulate)
        //        return true;

        //    if (itemController is EFT.Player.SinglePlayerInventoryController inventoryController)
        //    {
        //        if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
        //        {
        //            var player = coopGameComponent.Players.First(x => x.Key == inventoryController.Profile.AccountId).Value;
        //            if (CallLocally.Contains(player.ProfileId))
        //                return true;

        //        }
        //    }

        //    return false;
        //}
    }
}
