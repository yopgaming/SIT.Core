using EFT;
using EFT.InventoryLogic;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Player
{
    internal class PlayerInventoryController_ToggleItem_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => ReflectionHelpers.SearchForType("EFT.Player+PlayerInventoryController", false);

        public override string MethodName => "PlayerInventoryController_ToggleItem";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "ToggleItem", false, true);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(
            object __instance
            , TogglableComponent togglable
            , Profile ___profile_0
            )
        {
            Logger.LogDebug("PlayerInventoryController_ToggleItem_Patch:PrePatch");

            var result = false;

            if (CallLocally.TryGetValue(___profile_0.AccountId, out _))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            ItemController __instance
            , TogglableComponent togglable
            , Profile ___profile_0)
        {
            if (CallLocally.TryGetValue(___profile_0.AccountId, out _))
            {
                CallLocally.Remove(___profile_0.AccountId);
                return;
            }

            TogglablePacket togglablePacket = new(___profile_0.AccountId, togglable.Item.Id, togglable.Item.TemplateId, "PlayerInventoryController_ToggleItem", togglable.Item.Parent.Item.Id);
            var serialized = togglablePacket.Serialize();
            AkiBackendCommunication.Instance.SendDataToPool(serialized);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogDebug("PlayerInventoryController_ToggleItem_Patch:Replicated");

            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            taskScheduler.Do((s) =>
            {

                TogglablePacket itemPacket = new(null, null, null, null, null);

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
                        if (!ItemFinder.TryFindItem(itemPacket.ItemId, out Item togglableItem))
                        {
                            Logger.LogError($"PlayerInventoryController_ToggleItem_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.ItemId}");
                            return;
                        }

                        if (!ItemFinder.TryFindItem(itemPacket.ParentId, out Item parentItem))
                        {
                            Logger.LogError($"PlayerInventoryController_ToggleItem_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.ParentId}");
                            return;
                        }

                        CallLocally.Add(player.Profile.AccountId, true);
                        //Logger.LogInfo($"PlayerInventoryController_ToggleItem_Patch.Replicated. Calling ToggleItem ({magazine.Id})");
                        var method = ReflectionHelpers.GetMethodForType(invController.GetType(), "ToggleItem");
                        if (method == null)
                            return;

                        TogglableComponent togglableComponent = (TogglableComponent)ItemFinder.GetItemComponentsInChildren(togglableItem, typeof(TogglableComponent)).Single();
                        if (togglableComponent != null)
                        {
                            //this.itemUiContext_1.ToggleItem(togglableComponent);
                            Logger.LogInfo("PlayerInventoryController_ToggleItem_Patch.Replicated. TogglableComponent object found");
                        }
                        else
                        {
                            Logger.LogError("PlayerInventoryController_ToggleItem_Patch.Replicated. Unable to find TogglableComponent object");
                        }


                    }
                    else
                    {
                        Logger.LogError("PlayerInventoryController_ToggleItem_Patch.Replicated. Unable to find Inventory Controller object");
                    }
                }
                else
                {
                    Logger.LogError("PlayerInventoryController_ToggleItem_Patch.Replicated. Unable to find Inventory Controller");
                }
            });

        }

        public class TogglablePacket : ItemPlayerPacket
        {
            public TogglablePacket(string accountId, string itemId, string templateId, string method, string parentId) : base(accountId, itemId, templateId, method)
            {
                ParentId = parentId;
            }

            public string ParentId { get; set; }
        }
    }
}
