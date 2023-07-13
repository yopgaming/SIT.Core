using Comfort.Common;
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
    internal class Player_Proceed_Weapon_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "ProceedWeapon";

        public static Dictionary<string, bool> CallLocally = new();

        public static MethodInfo method1 = null;

        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player);
            if (t == null)
                Logger.LogInfo($"Player_Proceed_Weapon_Patch:Type is NULL");

            method1 = ReflectionHelpers.GetAllMethodsForType(t).FirstOrDefault(x => x.Name == "Proceed"
                && x.GetParameters().Length == 3
                && x.GetParameters()[0].Name == "weapon"
                && x.GetParameters()[0].ParameterType == typeof(EFT.InventoryLogic.Weapon)
                && x.GetParameters()[1].Name == "callback"
                && x.GetParameters()[1].ParameterType == typeof(Callback<IShootController>)
                && x.GetParameters()[2].Name == "scheduled"


                );

            return method1;
        }


        [PatchPrefix]
        public static bool PrePatch(
           EFT.Player __instance
            )
        {
            //if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            //{
            //    return true;
            //}

            // AI require this to ALWAYS run otherwise the AI won't start =(
            return true;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance
            , EFT.InventoryLogic.Weapon weapon
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
            ItemAddressHelpers.ConvertItemAddressToDescriptor(weapon.CurrentAddress, ref args);

            args.Add("m", "ProceedWeapon");
            args.Add("t", DateTime.Now.Ticks);
            args.Add("item.id", weapon.Id);
            args.Add("item.tpl", weapon.TemplateId);
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

            var allItemsOfTemplate = player.Profile.Inventory.GetAllItemByTemplate(dict["item.tpl"].ToString());

            if (!allItemsOfTemplate.Any())
                return;

            var item = allItemsOfTemplate.FirstOrDefault(x => x.Id == dict["item.id"].ToString());

            if (item != null && item is EFT.InventoryLogic.Weapon weapon)
            {
                CallLocally.Add(player.Profile.AccountId, true);

                var callback = new Callback<IShootController>((IResult) => { });
                method1.Invoke(player, new object[] { weapon, callback, true });
            }
        }
    }
}
