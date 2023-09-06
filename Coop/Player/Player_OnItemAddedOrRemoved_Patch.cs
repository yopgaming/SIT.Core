using BepInEx.Logging;
using EFT.InventoryLogic;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Player
{
    internal class Player_OnItemAddedOrRemoved_Patch : ModuleReplicationPatch
    {
        public static ManualLogSource Log { get {
            
                return GetLogger(typeof(Player_OnItemAddedOrRemoved_Patch));
            
            } }
        public override Type InstanceType => typeof(EFT.LocalPlayer);

        public override string MethodName => "OnItemAddedOrRemoved";

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            Logger.LogInfo("Replicated");
        }

        [PatchPrefix]
        public static bool Prefix(
            EFT.LocalPlayer __instance,
            Item item, ItemAddress location, bool added)
        {
            Log.LogDebug("prefix");
            Log.LogDebug(location.GetType());
            Log.LogDebug(location.ToJson());
            var player = __instance;
            var profileId = player.ProfileId;
            //if(!added)
            //{
                OnItemAddedOrRemovedPacket itemAddedOrRemovedPacket
                    = new OnItemAddedOrRemovedPacket(profileId, item.Id, item.TemplateId, location.GetType().FullName, location.ToJson(), added);
                AkiBackendCommunication.Instance.SendDataToPool(itemAddedOrRemovedPacket.Serialize());
            //}

            return true;
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        public class OnItemAddedOrRemovedPacket : ItemPlayerPacket
        {
            public string ItemAddressType { get; set; }
            public string ItemAddressJson { get; set; }
            public bool Added { get; set; }

            public OnItemAddedOrRemovedPacket(
                string profileId
                , string itemId
                , string templateId
                , string itemAddressType
                , string itemAddressJson
                , bool added
                ) 
                : base(profileId, itemId, templateId, "OnItemAddedOrRemoved")
            {
                ItemAddressType = itemAddressType;
                ItemAddressJson = itemAddressJson;
                Added = added;  
            }



            
        }
    }
}
