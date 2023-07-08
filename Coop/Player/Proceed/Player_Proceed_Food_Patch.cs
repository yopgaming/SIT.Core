using EFT.InventoryLogic;
using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.Player.Proceed
{
    internal class Player_Proceed_Food_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedFood";

        public static Dictionary<string, bool> CallLocally = new();

        public static MethodInfo method1 = null;

        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player);
            if (t == null)
                Logger.LogInfo($"Player_Proceed_Food_Patch:Type is NULL");

            method1 = ReflectionHelpers.GetAllMethodsForType(t).FirstOrDefault(x => x.Name == "Proceed" && x.GetParameters()[0].Name == "foodDrink");

            //Logger.LogInfo($"PlayerOnTryProceedPatch:{t.Name}:{method.Name}");
            return method1;
        }


        [PatchPrefix]
        public static bool PrePatch(
           EFT.Player __instance
            )
        {
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            {
                return true;
            }

            return false;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance
            , FoodDrink foodDrink
            , float amount
            , int animationVariant
            , bool scheduled)
        {
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(__instance.Profile.AccountId);
                return;
            }

            // Stop Client Drone sending a Proceed back to the player
            if (__instance.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                    return;
            }

            //Logger.LogInfo($"PlayerOnTryProceedPatch:Patch");
            Dictionary<string, object> args = new();
            ItemAddressHelpers.ConvertItemAddressToDescriptor(foodDrink.CurrentAddress, ref args);

            args.Add("m", "ProceedFood");
            args.Add("amt", amount);
            args.Add("item.id", foodDrink.Id);
            args.Add("item.tpl", foodDrink.TemplateId);
            args.Add("variant", animationVariant);
            args.Add("s", scheduled.ToString());
            AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(__instance, args);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC == null)
                return;

            if (!ItemFinder.TryFindItemOnPlayer(player, dict["item.tpl"].ToString(), dict["item.id"].ToString(), out Item item))
                ItemFinder.TryFindItemInWorld(dict["item.id"].ToString(), out item);

            if (item != null)
            {
                CallLocally.Add(player.Profile.AccountId, true);
                player.Proceed((FoodDrink)item, float.Parse(dict["amt"].ToString()), (IResult) => { }, 1, true);
            }
        }


    }
}