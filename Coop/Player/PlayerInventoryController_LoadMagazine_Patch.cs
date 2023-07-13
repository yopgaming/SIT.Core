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
    internal class PlayerInventoryController_LoadMagazine_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => ReflectionHelpers.SearchForType("EFT.Player+PlayerInventoryController", false);

        public override string MethodName => "PlayerInventoryController_LoadMagazine";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "LoadMagazine", false, true);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(
            object __instance
            , ref Task<IResult> __result
            , BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions
            , Profile ___profile_0
            )
        {
            //Logger.LogInfo("PlayerInventoryController_LoadMagazine_Patch:PrePatch");
            var result = false;

            if (CallLocally.TryGetValue(___profile_0.AccountId, out _))
                result = true;

            __result = new Task<IResult>(() => { return null; });
            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            ItemController __instance
            , BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions
            , Profile ___profile_0)
        {
            //Logger.LogInfo("PlayerInventoryController_LoadMagazine_Patch:PostPatch");

            if (CallLocally.TryGetValue(___profile_0.AccountId, out _))
            {
                CallLocally.Remove(___profile_0.AccountId);
                return;
            }

            LoadMagazinePacket itemPacket = new(___profile_0.AccountId, sourceAmmo.Id, sourceAmmo.TemplateId, magazine.Id, magazine.TemplateId
                , loadCount > 0 ? loadCount : sourceAmmo.StackObjectsCount
                , ignoreRestrictions);



            var serialized = itemPacket.Serialize();
            //Logger.LogInfo(serialized);
            AkiBackendCommunication.Instance.SendDataToPool(serialized);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            taskScheduler.Do((s) =>
            {
                //Logger.LogInfo($"PlayerInventoryController_LoadMagazine_Patch.Replicated");

                LoadMagazinePacket itemPacket = new(null, null, null, null, null, 0, false);

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
                        if (ItemFinder.TryFindItem(itemPacket.SourceAmmoId, out Item bullet))
                        {
                            if (ItemFinder.TryFindItem(itemPacket.MagazineId, out Item magazine))
                            {
                                CallLocally.Add(player.Profile.AccountId, true);
                                //Logger.LogInfo($"PlayerInventoryController_LoadMagazine_Patch.Replicated. Calling LoadMagazine ({bullet.Id}:{magazine.Id}:{itemPacket.LoadCount})");
                                invController.LoadMagazine((BulletClass)bullet, (MagazineClass)magazine, itemPacket.LoadCount);
                            }
                            else
                            {
                                Logger.LogError($"PlayerInventoryController_LoadMagazine_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.MagazineId}");
                            }
                        }
                        else
                        {
                            Logger.LogError($"PlayerInventoryController_LoadMagazine_Patch.Replicated. Unable to find Inventory Controller item {itemPacket.SourceAmmoId}");
                        }

                    }
                    else
                    {
                        Logger.LogError("PlayerInventoryController_LoadMagazine_Patch.Replicated. Unable to find Inventory Controller object");
                    }
                }
                else
                {
                    Logger.LogError("PlayerInventoryController_LoadMagazine_Patch.Replicated. Unable to find Inventory Controller");
                }
            });

        }

        public class LoadMagazinePacket : BasePlayerPacket
        {
            public string SourceAmmoId { get; set; }
            public string SourceTemplateId { get; set; }

            public string MagazineId { get; set; }
            public string MagazineTemplateId { get; set; }

            public int LoadCount { get; set; }

            public bool IgnoreRestrictions { get; set; }

            public LoadMagazinePacket(
                string accountId
                , string sourceAmmoId
                , string sourceTemplateId
                , string magazineId
                , string magazineTemplateId
                , int loadCount
                , bool ignoreRestrictions)
            {
                AccountId = accountId;
                Method = "PlayerInventoryController_LoadMagazine";
                this.SourceAmmoId = sourceAmmoId;
                this.SourceTemplateId = sourceTemplateId;
                this.MagazineId = magazineId;
                this.MagazineTemplateId = magazineTemplateId;
                this.LoadCount = loadCount;
                this.IgnoreRestrictions = ignoreRestrictions;
            }
        }


    }
}
