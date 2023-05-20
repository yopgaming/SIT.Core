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
    public class FirearmController_ReloadMag_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ReloadMag";
        //public override bool DisablePatch => true;

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally = new();


        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            return true;
            //var player = ____player;
            ////var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            //if (player == null)
            //    return false;

            //var result = false;
            //if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            //    result = true;

            //Logger.LogInfo("FirearmController_ReloadMag_Patch:PrePatch");

            //return result;
        }

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
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> magAddressDict = new();
            ItemAddressHelpers.ConvertItemAddressToDescriptor(magazine.CurrentAddress, ref magAddressDict);

            Dictionary<string, object> gridAddressDict = new();
            ItemAddressHelpers.ConvertItemAddressToDescriptor(gridItemAddress, ref gridAddressDict);

            Dictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { "t", DateTime.Now.Ticks },
                { "mg.id", magazine.Id },
                { "mg.tpl", magazine.Template },
                { "ma", magAddressDict },
                { "ga", gridAddressDict },
                { "m", "ReloadMag" }
            };
            ServerCommunication.PostLocalPlayerData(player, dictionary);
            //Logger.LogInfo("FirearmController_ReloadMag_Patch:PostPatch");

        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    var ma = JsonConvert.DeserializeObject<Dictionary<string, object>>(dict["ma"].ToString());
                    ItemAddressHelpers.ConvertDictionaryToAddress(ma, out var magAddressGrid, out var magAddressSlot);

                    var ga = JsonConvert.DeserializeObject<Dictionary<string, object>>(dict["ga"].ToString());
                    ItemAddressHelpers.ConvertDictionaryToAddress(ga, out var gridAddressGrid, out var gridAddressSlot);

                    //player.ToUnloadMagOperation().
                    var magazine = player.Profile.Inventory.GetAllItemByTemplate(dict["mg.tpl"].ToString()).FirstOrDefault() as MagazineClass;
                    if (magazine == null)
                        return;

                    StashGrid grid = player.Profile.Inventory.Equipment.FindContainer(gridAddressGrid.Container.ContainerId, gridAddressGrid.Container.ParentId) as StashGrid;

                    // this is not working, maybe try and find it in the inventory instead???
                    //var magazine = new MagazineClass(dict["mg.id"].ToString(), JObject.Parse(dict["mg.tpl"].ToString()).ToObject<MagazineTemplate>());
                    //var gridItemAddressNewDesc = JObject.Parse(dict["a.new"].ToString()).ToObject<GridItemAddressDescriptor>();
                    //var gridItemAddressNew = new GridItemAddress(
                    //        (Grid)player.Inventory.Equipment.FindContainer(gridItemAddressNewDesc.Container.ContainerId, gridItemAddressNewDesc.Container.ParentId)
                    //        , gridItemAddressNewDesc.LocationInGrid
                    //        );

                    //var gridItemAddressOldDesc = JObject.Parse(dict["a.old"].ToString()).ToObject<GridItemAddressDescriptor>();
                    //var gridItemAddressOld = new GridItemAddress(
                    //        (Grid)player.Inventory.Equipment.FindContainer(gridItemAddressOldDesc.Container.ContainerId, gridItemAddressOldDesc.Container.ParentId)
                    //        , gridItemAddressOldDesc.LocationInGrid
                    //        );



                    firearmCont.ReloadMag(magazine, new GridItemAddress(grid, gridAddressGrid.LocationInGrid), (IResult) => { });

                    //magazine.CurrentAddress = gridItemAddressOld;
                    //CallLocally.Add(player.Profile.AccountId, true);
                    //Logger.LogInfo("Replicated: Calling Reload Mag");
                    //firearmCont.ReloadMag(magazine
                    //    //, gridItemAddressNew
                    //    , gridItemAddressOld
                    //    , null);
                }
                catch (Exception e)
                {
                    Logger.LogError(e);
                }
            }
        }
    }
}
