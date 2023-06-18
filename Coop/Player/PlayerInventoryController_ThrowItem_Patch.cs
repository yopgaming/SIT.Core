using EFT.InventoryLogic;
using EFT.UI;
using EFT;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using System.Security.Policy;
using static UnityEngine.UIElements.StyleVariableResolver;

namespace SIT.Core.Coop.Player
{
    internal class PlayerInventoryController_ThrowItem_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => ReflectionHelpers.SearchForType("EFT.Player+PlayerInventoryController", false);

        public override string MethodName => "PlayerInventoryController_ThrowItem";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "ThrowItem", false, true);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(
           object __instance
            , Item item
            , Profile ___profile_0
            )
        {
            Logger.LogInfo("PlayerInventoryController_ThrowItem_Patch:PrePatch");
            var result = false;

            if (CallLocally.TryGetValue(___profile_0.AccountId, out _))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            object __instance
            , Item item
            , Profile ___profile_0
            )
        {
            Logger.LogInfo("PlayerInventoryController_ThrowItem_Patch:PostPatch");

            if (CallLocally.TryGetValue(___profile_0.AccountId, out _))
            {
                CallLocally.Remove(___profile_0.AccountId);
                return;
            }

            var _item = item;
            ItemPlayerPacket itemPacket = new ItemPlayerPacket(___profile_0.AccountId, _item.Id, _item.TemplateId, "PlayerInventoryController_ThrowItem");
            var serialized = itemPacket.Serialize();
            Request.Instance.SendDataToPool(serialized);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            taskScheduler.Do((s) =>
            {
                Logger.LogInfo($"PlayerInventoryController_ThrowItem_Patch.Replicated");

                ItemPlayerPacket itemPacket = new ItemPlayerPacket(null, null, null, "");

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

                var fieldInfoInvController = ReflectionHelpers.GetFieldFromTypeByFieldType(player.GetType(), typeof(InventoryController));
                if (fieldInfoInvController != null)
                {
                    var invController = (InventoryController)fieldInfoInvController.GetValue(player);
                    if (invController != null)
                    {
                        if (ItemFinder.TryFindItem(itemPacket.ItemId, out Item item))
                        {
                            CallLocally.Add(player.Profile.AccountId, true);
                            Logger.LogInfo($"PlayerInventoryController_ThrowItem_Patch.Replicated. Calling ThrowItem ({itemPacket.ItemId})");
                            invController.ThrowItem(item, new List<ItemsCount>());
                        }
                        else
                        {
                            Logger.LogError($"PlayerInventoryController_ThrowItem_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.ItemId}");
                        }

                    }
                    else
                    {
                        Logger.LogError("PlayerInventoryController_ThrowItem_Patch.Replicated. Unable to find Inventory Controller object");
                    }
                }
                else
                {
                    Logger.LogError("PlayerInventoryController_UnloadMagazine.Replicated. Unable to find Inventory Controller");
                }
            });

        }

    }
}
