using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Player
{
    internal class PlayerInventoryController_UnloadMagazine_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => ReflectionHelpers.SearchForType("EFT.Player+PlayerInventoryController", false);

        public override string MethodName => "PlayerInventoryController_UnloadMagazine";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "UnloadMagazine", false, true);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(
            object __instance
            , ref Task<IResult> __result
            , MagazineClass magazine
            , Profile ___profile_0
            )
        {
            //Logger.LogInfo("PlayerInventoryController_UnloadMagazine:PrePatch");
            var result = false;

            if (CallLocally.TryGetValue(___profile_0.AccountId, out _))
                result = true;

            __result = new Task<IResult>(() => { return null; });
            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            ItemController __instance
            , MagazineClass magazine
            , Profile ___profile_0)
        {
            //Logger.LogInfo("PlayerInventoryController_UnloadMagazine:PostPatch");

            if (CallLocally.TryGetValue(___profile_0.AccountId, out _))
            {
                CallLocally.Remove(___profile_0.AccountId);
                return;
            }

            UnloadMagazinePacket unloadMagazinePacket = new(___profile_0.AccountId, magazine.Id, magazine.TemplateId);
            var serialized = unloadMagazinePacket.Serialize();
            AkiBackendCommunication.Instance.SendDataToPool(serialized);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            taskScheduler.Do((s) =>
            {
                //Logger.LogInfo($"PlayerInventoryController_LoadMagazine_Patch.Replicated");

                UnloadMagazinePacket itemPacket = new(null, null, null);

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

                //if (CallLocally.ContainsKey(player.Profile.AccountId))
                //    return;

                ////Logger.LogInfo($"ItemUiContext_ThrowItem_Patch.Replicated Profile Id {itemPacket.AccountId}");

                var fieldInfoInvController = ReflectionHelpers.GetFieldFromTypeByFieldType(player.GetType(), typeof(InventoryController));
                if (fieldInfoInvController != null)
                {
                    var invController = (InventoryController)fieldInfoInvController.GetValue(player);
                    if (invController != null)
                    {
                        if (ItemFinder.TryFindItem(itemPacket.MagazineId, out Item magazine))
                        {
                            CallLocally.Add(player.Profile.AccountId, true);
                            //Logger.LogInfo($"PlayerInventoryController_UnloadMagazine.Replicated. Calling UnloadMagazine ({magazine.Id})");
                            invController.UnloadMagazine((MagazineClass)magazine);
                        }
                        else
                        {
                            Logger.LogError($"PlayerInventoryController_UnloadMagazine.Replicated. Unable to find Inventory Controller item {itemPacket.MagazineId}");
                        }

                    }
                    else
                    {
                        Logger.LogError("PlayerInventoryController_LoadMagazine_Patch.Replicated. Unable to find Inventory Controller object");
                    }
                }
                else
                {
                    Logger.LogError("PlayerInventoryController_UnloadMagazine.Replicated. Unable to find Inventory Controller");
                }
            });

        }

        public class UnloadMagazinePacket : BasePlayerPacket
        {
            public string MagazineId { get; set; }
            public string MagazineTemplateId { get; set; }

            public UnloadMagazinePacket(
                string accountId
                , string magazineId
                , string magazineTemplateId
                )
            {
                AccountId = accountId;
                Method = "PlayerInventoryController_UnloadMagazine";
                this.MagazineId = magazineId;
                this.MagazineTemplateId = magazineTemplateId;
            }
        }


    }
}
