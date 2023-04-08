using EFT.InventoryLogic;
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class Player_TryProceed_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "TryProceed";

        public static Dictionary<string, bool> CallLocally
            = new Dictionary<string, bool>();

        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player);
            if (t == null)
                Logger.LogInfo($"PlayerOnTryProceedPatch:Type is NULL");

            var method = ReflectionHelpers.GetMethodForType(t, MethodName);

            //Logger.LogInfo($"PlayerOnTryProceedPatch:{t.Name}:{method.Name}");
            return method;
        }


        [PatchPrefix]
        public static bool PrePatch(
           EFT.Player __instance
            )
        {
            if (__instance.IsAI)
                return true;

            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance
            , Item item
            , bool scheduled)
        {
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(__instance.Profile.AccountId);
                return;
            }

            //Logger.LogInfo($"PlayerOnTryProceedPatch:Patch");
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("m", "TryProceed");
            args.Add("t", DateTime.Now.Ticks);
            args.Add("item.id", item.Id);
            args.Add("item.tpl", item.TemplateId);
            args.Add("s", scheduled.ToString());
            ServerCommunication.PostLocalPlayerData(__instance, args);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            var item = player.Profile.Inventory.GetAllItemByTemplate(dict["item.tpl"].ToString()).FirstOrDefault();
            if(item == null)
            {
                Logger.LogError($"PlayerOnTryProceedPatch:{player.Profile.AccountId}:Replicated:Item not found");
                player.SetFirstAvailableItem((result) => { 
                    Logger.LogDebug(result.ToString());
                });
                return;
            }
            
            if (item != null)
            {
                //Logger.LogDebug($"PlayerOnTryProceedPatch:{player.Profile.AccountId}:Replicated:Found Item");
                CallLocally.Add(player.Profile.AccountId, true);
                player.TryProceed(item, (IResult) =>
                {
                    Logger.LogDebug($"PlayerOnTryProceedPatch:{player.Profile.AccountId}:Replicated:Try Proceed Succeeded?:{IResult.Succeed}");
                }, bool.Parse(dict["s"].ToString()));
            }
        }
    }
}