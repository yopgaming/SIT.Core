using EFT;
using Newtonsoft.Json;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ReloadWithAmmo_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ReloadWithAmmo";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally = new();


        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            return false;

            //var player = ____player;
            //if (player == null)
            //    return false;

            //var result = false;
            //if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            //    result = true;

            ////Logger.LogInfo("FirearmController_ReloadMag_Patch:PrePatch");

            //return result;
        }

        //[PatchPostfix]
        //public static void PostPatch(
        //    EFT.Player.FirearmController __instance
        //    , AmmoPack ammoPack
        //    , EFT.Player ____player)
        //{
        //    var player = ____player;
        //    //var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
        //    if (player == null)
        //        return;

        //    if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
        //    {
        //        CallLocally.Remove(player.Profile.AccountId);
        //        return;
        //    }

        //    Dictionary<string, object> magAddressDict = new();
        //    ItemAddressHelpers.ConvertItemAddressToDescriptor(magazine.CurrentAddress, ref magAddressDict);

        //    Dictionary<string, object> gridAddressDict = new();
        //    ItemAddressHelpers.ConvertItemAddressToDescriptor(gridItemAddress, ref gridAddressDict);

        //    Dictionary<string, object> dictionary = new Dictionary<string, object>
        //    {
        //        { "t", DateTime.Now.Ticks },
        //        { "fa.id", __instance.Item.Id },
        //        { "fa.tpl", __instance.Item.TemplateId },
        //        { "ap.id", ammoPack..Id },
        //        { "ap.tpl", magazine.TemplateId },
        //        { "ma", magAddressDict },
        //        { "ga", gridAddressDict },
        //        { "m", "ReloadMag" }
        //    };
        //    ServerCommunication.PostLocalPlayerData(player, dictionary);
        //    //Logger.LogInfo("FirearmController_ReloadMag_Patch:PostPatch");

        //}

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            //Logger.LogInfo("FirearmController_ReloadMag_Patch:Replicated");

            //if (player.HandsController is EFT.Player.FirearmController firearmCont)
            //{
            //    try
            //    {

            //        var ma = JsonConvert.DeserializeObject<Dictionary<string, object>>(dict["ma"].ToString());
            //        ItemAddressHelpers.ConvertDictionaryToAddress(ma, out var magAddressGrid, out var magAddressSlot);

            //        //Logger.LogInfo("FirearmController_ReloadMag_Patch:Replicated:ma");

            //        var ga = JsonConvert.DeserializeObject<Dictionary<string, object>>(dict["ga"].ToString());
            //        ItemAddressHelpers.ConvertDictionaryToAddress(ga, out var gridAddressGrid, out var gridAddressSlot);

            //        //Logger.LogInfo("FirearmController_ReloadMag_Patch:Replicated:ga");

            //        var magazine = player.Profile.Inventory.GetAllItemByTemplate(dict["mg.tpl"].ToString())
            //            .FirstOrDefault(x => x.Id == dict["mg.id"].ToString()) as MagazineClass;
            //        if (magazine == null)
            //        {
            //            Logger.LogError("FirearmController_ReloadMag_Patch:Replicated:Unable to find Magazine!");
            //            return;
            //        }

            //        if(!ReplicatedGridAddressGrid(player, firearmCont, gridAddressGrid, magazine))
            //            ReplicatedGridAddressSlot(player, firearmCont, gridAddressSlot, magazine);

            //    }
            //    catch (Exception e)
            //    {
            //        Logger.LogError(e);
            //    }
            //}
        }

        //bool ReplicatedGridAddressGrid(EFT.Player player, EFT.Player.FirearmController firearmCont, GridItemAddressDescriptor gridAddressGrid, MagazineClass magazine)
        //{
        //    if (gridAddressGrid == null)
        //        return false;

        //    StashGrid grid = player.Profile.Inventory.Equipment.FindContainer(gridAddressGrid.Container.ContainerId, gridAddressGrid.Container.ParentId) as StashGrid;
        //    if (grid == null)
        //    {
        //        Logger.LogError("FirearmController_ReloadMag_Patch:Replicated:Unable to find grid!");
        //        return false;
        //    }

        //    if (!CallLocally.ContainsKey(player.Profile.AccountId))
        //        CallLocally.Add(player.Profile.AccountId, true);

        //    try
        //    {

        //        firearmCont.ReloadMag(magazine, new GridItemAddress(grid, gridAddressGrid.LocationInGrid), (IResult) =>
        //        {

        //            //Logger.LogDebug($"ReloadMag:Succeed?:{IResult.Succeed}");


        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError($"FirearmController_ReloadMag_Patch:Replicated:{ex}!");
        //        return false;
        //    }

        //    return true;
        //}

        //void ReplicatedGridAddressSlot(EFT.Player player, EFT.Player.FirearmController firearmCont, SlotItemAddressDescriptor gridAddressSlot, MagazineClass magazine)
        //{
        //    if (gridAddressSlot == null)
        //        return;

        //    StashGrid grid = player.Profile.Inventory.Equipment.FindContainer(gridAddressSlot.Container.ContainerId, gridAddressSlot.Container.ParentId) as StashGrid;
        //    if (grid == null)
        //    {
        //        Logger.LogError("FirearmController_ReloadMag_Patch:Replicated:Unable to find grid!");
        //        return;
        //    }

        //    if (!CallLocally.ContainsKey(player.Profile.AccountId))
        //        CallLocally.Add(player.Profile.AccountId, true);

        //    try
        //    {

        //        firearmCont.ReloadMag(magazine, grid.FindLocationForItem(magazine), (IResult) =>
        //        {

        //            //Logger.LogDebug($"ReloadMag:Succeed?:{IResult.Succeed}");


        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError($"FirearmController_ReloadMag_Patch:Replicated:{ex}!");
        //        return;
        //    }
        //}
    }
}
