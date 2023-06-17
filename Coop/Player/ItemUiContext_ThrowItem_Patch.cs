using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using SIT.Coop.Core.Web;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static SIT.Core.Coop.Player.FirearmControllerPatches.FirearmController_SetTriggerPressed_Patch;

namespace SIT.Core.Coop.Player
{
    internal class ItemUiContext_ThrowItem_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(ItemUiContext);

        public override string MethodName => "ItemUiContext_ThrowItem";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "ThrowItem", false, true);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(
            ItemController __instance
            , ref Task __result
            , ref Item item)
        {
            Logger.LogInfo("ItemUiContext_ThrowItem_Patch:PrePatch");
            var result = false;
            __result = Task.Run(() =>
            {
            });
            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            ItemController __instance
            , ref Task __result
            , ref Item item
            , Profile ___profile_0)
        {
            var _item = item;
            ItemPlayerPacket itemPacket = new ItemPlayerPacket(___profile_0.AccountId, _item.Id, _item.TemplateId, "ItemUiContext_ThrowItem");
            var serialized = itemPacket.Serialize();
            //Logger.LogInfo("ItemUiContext_ThrowItem_Patch:PostPatch");
            //Logger.LogInfo(serialized);
            Request.Instance.SendDataToPool(serialized);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //Logger.LogInfo($"ItemUiContext_ThrowItem_Patch.Replicated");

            ItemPlayerPacket itemPacket = new(null, null, null, null);

            if (dict.ContainsKey("data"))
            {
                itemPacket = itemPacket.DeserializePacketSIT(dict["data"].ToString());
            }
            else
            {
                return;
            }

            if (HasProcessed(GetType(), player, itemPacket))
                return;

            //Logger.LogInfo($"ItemUiContext_ThrowItem_Patch.Replicated Profile Id {itemPacket.AccountId}");

            var fieldInfoInvController = ReflectionHelpers.GetFieldFromTypeByFieldType(player.GetType(), typeof(InventoryController));
            if (fieldInfoInvController != null)
            {
                var invController = (InventoryController)fieldInfoInvController.GetValue(player);
                if (invController != null)
                {
                    if (ItemFinder.TryFindItemOnPlayer(player, itemPacket.TemplateId, itemPacket.ItemId, out Item item))
                    {
                        //Logger.LogInfo($"Throwing Item {item.Id}!");
                        invController.ThrowItem(item, new List<ItemsCount>());
                    }
                    else
                    {
                        Logger.LogError($"ItemUiContext_ThrowItem_Patch.Replicated. Unable to find Inventory Controller item {item.Id}");
                    }
                }
                else
                {
                    Logger.LogError("ItemUiContext_ThrowItem_Patch.Replicated. Unable to find Inventory Controller object");
                }
            }
            else
            {
                Logger.LogError("ItemUiContext_ThrowItem_Patch.Replicated. Unable to find Inventory Controller");
            }

        }


    }
}
