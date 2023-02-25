using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SIT.Coop.Core.Web;
using SIT.Tarkov.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_ReloadMag_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "ReloadMag";
        public override bool DisablePatch => true;

        protected override MethodBase GetTargetMethod()
        {
            var method = PatchConstants.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new Dictionary<string, bool>();


        [PatchPrefix]
        public static bool PrePatch(EFT.Player.FirearmController __instance, EFT.Player ____player)
        {
            var player = ____player;
            //var player = PatchConstants.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            Logger.LogInfo("FirearmController_ReloadMag_Patch:PrePatch");

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , MagazineClass magazine
            , GridItemAddress gridItemAddress
            , EFT.Player ____player)
        {
            var player = ____player;
            //var player = PatchConstants.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            var giadNewAddress_I_Think = new GridItemAddressDescriptor()
            {
                LocationInGrid = gridItemAddress.LocationInGrid,
                Container = new ContainerDescriptor() 
                { 
                    ContainerId = gridItemAddress.Container.ID
                    , ParentId = gridItemAddress.Container.ParentItem.Id
                }
            };

            var giadOldAddress_I_Think = new GridItemAddressDescriptor()
            {
                LocationInGrid = gridItemAddress.LocationInGrid,
                Container = new ContainerDescriptor()
                {
                    ContainerId = magazine.CurrentAddress.Container.ID
                    ,
                    ParentId = magazine.CurrentAddress.Container.ParentItem.Id
                }
            };

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("mg.id", magazine.Id);
            dictionary.Add("mg.tpl", magazine.Template);
            dictionary.Add("a.old", giadOldAddress_I_Think.SITToJson());
            dictionary.Add("a.new", giadNewAddress_I_Think.SITToJson());
            dictionary.Add("m", "ReloadMag");
            ServerCommunication.PostLocalPlayerData(player, dictionary);
            Logger.LogInfo("FirearmController_ReloadMag_Patch:PostPatch");

        }

        private static List<long> ProcessedCalls = new List<long>();

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //Logger.LogInfo("FirearmController_ReloadMag_Patch:Replicated");

            var timestamp = long.Parse(dict["t"].ToString());
            if (!ProcessedCalls.Contains(timestamp))
                ProcessedCalls.Add(timestamp);
            else
            {
                ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddHours(-1).Ticks);
                return;
            }

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    //player.ToUnloadMagOperation().
                    
                    // this is not working, maybe try and find it in the inventory instead???
                    var magazine = new MagazineClass(dict["mg.id"].ToString(), JObject.Parse(dict["mg.tpl"].ToString()).ToObject<MagazineTemplate>());
                    var gridItemAddressNewDesc = JObject.Parse(dict["a.new"].ToString()).ToObject<GridItemAddressDescriptor>();
                    var gridItemAddressNew = new GridItemAddress(
                            (Grid)player.Inventory.Equipment.FindContainer(gridItemAddressNewDesc.Container.ContainerId, gridItemAddressNewDesc.Container.ParentId)
                            , gridItemAddressNewDesc.LocationInGrid
                            );

                    var gridItemAddressOldDesc = JObject.Parse(dict["a.old"].ToString()).ToObject<GridItemAddressDescriptor>();
                    var gridItemAddressOld = new GridItemAddress(
                            (Grid)player.Inventory.Equipment.FindContainer(gridItemAddressOldDesc.Container.ContainerId, gridItemAddressOldDesc.Container.ParentId)
                            , gridItemAddressOldDesc.LocationInGrid
                            );

                    magazine.CurrentAddress = gridItemAddressOld;
                    CallLocally.Add(player.Profile.AccountId, true);
                    Logger.LogInfo("Replicated: Calling Reload Mag");
                    firearmCont.ReloadMag(magazine
                        //, gridItemAddressNew
                        , gridItemAddressOld
                        , null);
                }
                catch(Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
        }
    }
}
