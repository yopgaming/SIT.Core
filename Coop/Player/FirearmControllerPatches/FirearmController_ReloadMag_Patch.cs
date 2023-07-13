using Newtonsoft.Json;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ReloadMag_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ReloadMag";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static List<string> CallLocally = new();

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , MagazineClass magazine
            , GridItemAddress gridItemAddress
            , EFT.Player ____player)
        {
            var player = ____player;
            //var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
            {
                Logger.LogError("Unable to obtain Player variable from Firearm Controller!");
                return;
            }

            if (CallLocally.Contains(player.ProfileId))
            {
                CallLocally.Remove(player.ProfileId);
                return;
            }

            Dictionary<string, object> magAddressDict = new();
            ItemAddressHelpers.ConvertItemAddressToDescriptor(magazine.CurrentAddress, ref magAddressDict);

            Dictionary<string, object> gridAddressDict = new();
            ItemAddressHelpers.ConvertItemAddressToDescriptor(gridItemAddress, ref gridAddressDict);

            Dictionary<string, object> dictionary = new()
            {
                { "fa.id", __instance.Item.Id },
                { "fa.tpl", __instance.Item.TemplateId },
                { "mg.id", magazine.Id },
                { "mg.tpl", magazine.TemplateId },
                { "ma", magAddressDict },
                { "ga", gridAddressDict },
                { "m", "ReloadMag" }
            };
            AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(player, dictionary, true);
            GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug("FirearmController_ReloadMag_Patch:PostPatch");

            // ---------------------------------------------------------------------------------------------------------------------
            // Note. If the player is AI or High Ping. Stop the loop caused by the sent packet above
            //if (IsHighPingOrAI(player))
            //{
            HasProcessed(typeof(FirearmController_ReloadMag_Patch), player, dictionary);
            //}
        }



        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug("FirearmController_ReloadMag_Patch:Replicated");

            if (HasProcessed(GetType(), player, dict))
            {
                GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug("FirearmController_ReloadMag_Patch:Replicated. Has been Processed. Ignoring");
                return;
            }

            GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug("FirearmController_ReloadMag_Patch:Replicated. Doing things!");

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    var ma = JsonConvert.DeserializeObject<Dictionary<string, object>>(dict["ma"].ToString());
                    ItemAddressHelpers.ConvertDictionaryToAddress(ma, out var magAddressGrid, out var magAddressSlot);

                    var ga = JsonConvert.DeserializeObject<Dictionary<string, object>>(dict["ga"].ToString());
                    ItemAddressHelpers.ConvertDictionaryToAddress(ga, out var gridAddressGrid, out var gridAddressSlot);

                    var magTemplateId = dict["mg.tpl"].ToString();
                    var magItemId = dict["mg.id"].ToString();
                    if (ItemFinder.TryFindItemOnPlayer(player, magTemplateId, magItemId, out var magazine))
                    {
                        try
                        {
                            firearmCont.StartCoroutine(Reload(player, firearmCont, gridAddressGrid, gridAddressSlot, magAddressGrid, magAddressSlot, (MagazineClass)magazine));
                        }
                        catch
                        {
                            GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug($"{player.ProfileId} Notify to use ICH Move Patch");
                            ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(player.ProfileId);
                        }
                    }
                }
                catch (Exception e)
                {
                    GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogError(e);
                }
            }
            else
            {
                GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogError("FirearmController_ReloadMag_Patch:Replicated. HandsController is not a Firearm Controller?!");
                GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogError(player.HandsController.GetType());
            }
        }

        private IEnumerator Reload(EFT.Player player
            , EFT.Player.FirearmController firearmCont
            , GridItemAddressDescriptor gridAddressGrid
            , SlotItemAddressDescriptor gridAddressSlot
            , GridItemAddressDescriptor magAddressGrid
            , SlotItemAddressDescriptor magAddressSlot
            , MagazineClass magazine)
        {
            while (!firearmCont.CanStartReload())
            {
                yield return new WaitForSeconds(1);
                //GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug($"{player.ProfileId} can't reload!");
                yield return new WaitForEndOfFrame();
            }

            GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug($"{player.ProfileId} Notify to not use ICH Move Patch");
            ItemControllerHandler_Move_Patch.DisableForPlayer.Add(player.ProfileId);

            ReplicatedGridAddressGrid(player, firearmCont, gridAddressGrid, (MagazineClass)magazine

                , () =>
                {
                    GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug($"{player.ProfileId} Notify to use ICH Move Patch");
                    ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(player.ProfileId);
                    firearmCont.StopCoroutine(nameof(Reload));
                }
                , () =>
                {

                    if (ReplicatedGridAddressSlot(player, firearmCont, gridAddressSlot, (MagazineClass)magazine))
                    {
                        firearmCont.StopCoroutine(nameof(Reload));
                        GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug($"{player.ProfileId} Notify to use ICH Move Patch");
                        ItemControllerHandler_Move_Patch.DisableForPlayer.Remove(player.ProfileId);
                    }
                    else
                    {
                        firearmCont.StartCoroutine(nameof(Reload));
                    }
                }
                );
        }

        bool ReplicatedGridAddressGrid(EFT.Player player
            , EFT.Player.FirearmController firearmCont
            , GridItemAddressDescriptor gridAddressGrid
            , MagazineClass magazine
            , Action successCallback
            , Action failureCallback
            )
        {
            if (gridAddressGrid == null)
            {
                GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug("ReplicatedGridAddressGrid.GridAddressGrid is Null");
                return false;
            }
            var inventoryController = ItemFinder.GetPlayerInventoryController(player);
            GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug("FirearmController_ReloadMag_Patch.ReplicatedGridAddressSlot." + inventoryController.GetType());


            StashGrid grid = player.Profile.Inventory.Equipment.FindContainer(gridAddressGrid.Container.ContainerId, gridAddressGrid.Container.ParentId) as StashGrid;
            if (grid == null)
            {
                //Logger.LogError("FirearmController_ReloadMag_Patch:Replicated:Unable to find grid!");
                return false;
            }

            if (!CallLocally.Contains(player.ProfileId))
                CallLocally.Add(player.ProfileId);

            try
            {
                firearmCont.ReloadMag(magazine, new GridItemAddress(grid, gridAddressGrid.LocationInGrid), (IResult) =>
                {
                    GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug($"ReloadMag:Succeed?:{IResult.Succeed}");
                    if (IResult.Failed)
                    {
                        GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogDebug($"ReloadMag:IResult:Error:{IResult.Error}");
                        failureCallback();
                    }
                    else
                    {
                        successCallback();
                    }
                });
            }
            catch (Exception ex)
            {
                GetLogger(typeof(FirearmController_ReloadMag_Patch)).LogError($"FirearmController_ReloadMag_Patch:Replicated:{ex}!");
                return false;
            }

            return true;
        }

        bool ReplicatedGridAddressSlot(EFT.Player player, EFT.Player.FirearmController firearmCont, SlotItemAddressDescriptor gridAddressSlot, MagazineClass magazine)
        {
            if (gridAddressSlot == null)
            {
                Logger.LogDebug("ReplicatedGridAddressGrid.GridAddressSlot is Null");
                return false;
            }

            //var inventoryController = ReflectionHelpers.GetFieldFromTypeByFieldType(player.GetType(), typeof(InventoryController)).GetValue(player);
            var inventoryController = ItemFinder.GetPlayerInventoryController(player);
            Logger.LogInfo("FirearmController_ReloadMag_Patch.ReplicatedGridAddressSlot." + inventoryController.GetType());

            //if (inventoryController is SinglePlayerInventoryController singlePlayerInventoryController)
            {
                //var itemAddress = singlePlayerInventoryController.ToItemAddress(gridAddressSlot);

                StashGrid grid = player.Profile.Inventory.Equipment.FindContainer(gridAddressSlot.Container.ContainerId, gridAddressSlot.Container.ParentId) as StashGrid;
                if (grid == null)
                {
                    //Logger.LogError("FirearmController_ReloadMag_Patch:Replicated:Unable to find grid!");
                    return false;
                }

                if (!CallLocally.Contains(player.ProfileId))
                    CallLocally.Add(player.ProfileId);

                try
                {

                    firearmCont.ReloadMag(magazine, grid.FindLocationForItem(magazine), (IResult) =>
                    {
                        Logger.LogDebug($"ReloadMag:Succeed?:{IResult.Succeed}");
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError($"FirearmController_ReloadMag_Patch:Replicated:{ex}!");
                    return false;
                }
            }

            return true;
        }
    }
}
