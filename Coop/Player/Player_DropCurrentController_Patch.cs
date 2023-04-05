using EFT.InventoryLogic;
using Newtonsoft.Json;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player
{
    internal class Player_DropCurrentController_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "DropCurrentController";

        public override bool DisablePatch => true;

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            //Logger.LogInfo($"Player_DropCurrentController_Patch:{InstanceType.Name}:{method.Name}");

            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new Dictionary<string, bool>();

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
           bool fastDrop, Item nextControllerItem
            )
        {
            var player = __instance;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("f", fastDrop.ToString());
            if (nextControllerItem != null)
            {
                dictionary.Add("nCI", SerializeObject(nextControllerItem));
                var iaD = new SlotItemAddressDescriptor()
                {
                    Container = new ContainerDescriptor()
                    {
                        ContainerId = nextControllerItem.CurrentAddress.Container.ID
                   ,
                        ParentId = nextControllerItem.CurrentAddress.Container.ParentItem.Id
                    }
                };
                dictionary.Add("addr", SerializeObject(iaD));

            }
            dictionary.Add("m", "DropCurrentController");
            ServerCommunication.PostLocalPlayerData(player, dictionary);
        }

        private static List<long> ProcessedCalls = new List<long>();

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());
            if (!ProcessedCalls.Contains(timestamp))
                ProcessedCalls.Add(timestamp);
            else
            {
                ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddMinutes(-5).Ticks);
                return;
            }

            CallLocally.Add(player.Profile.AccountId, true);

            try
            {

                var fastDrop = bool.Parse(dict["f"].ToString());
                Item nci = null;
                if (dict.ContainsKey("nCI"))
                {
                    nci = DeserializeObject<Item>(dict["nCI"].ToString());
                    var addressDescriptor = JsonConvert.DeserializeObject<SlotItemAddressDescriptor>(dict["addr"].ToString());
                    var gridItemAddress = new GridItemAddress(
                            (Grid)player.Inventory.Equipment.FindContainer(addressDescriptor.Container.ContainerId, addressDescriptor.Container.ParentId)
                            , ((Grid)player.Inventory.Equipment.FindContainer(addressDescriptor.Container.ContainerId, addressDescriptor.Container.ParentId)).GetItemLocation(nci)
                            );
                    if (nci.Parent == null)
                    {
                        // ERROR. Parent not found so this wont work!
                        Logger.LogError("Player_DropCurrentController_Patch:Replicated:Could not find Parent.Just running vanilla!");
                        return;
                    }
                }
                player.DropCurrentController(null, fastDrop, nci);
            }
            catch (Exception e)
            {
                player.DropCurrentController(null, false, null);
                Logger.LogInfo(e);
            }
        }
    }
}
