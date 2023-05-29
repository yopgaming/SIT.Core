using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_SetTriggerPressed_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "SetTriggerPressed";

        //public override bool DisablePatch => true;

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally = new();

        public static Dictionary<string, bool> LastPress = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            , bool pressed
            )
        {
            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , bool pressed
            , EFT.Player ____player
            )
        {
            var player = ____player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            var ticks = DateTime.Now.Ticks;
            Dictionary<string, object> packet = new Dictionary<string, object>();
            packet.Add("t", ticks);
            packet.Add("pr", pressed.ToString());
            packet.Add("m", "SetTriggerPressed");
            ServerCommunication.PostLocalPlayerData(player, packet);
            //Logger.LogInfo("Pressed:PostPatch");
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //Logger.LogInfo("Pressed:Replicated");

            if (HasProcessed(GetType(), player, dict))
                return;

            if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                return;

            //if (!prc.IsClientDrone)
            //    return;

            if (CallLocally.ContainsKey(player.Profile.AccountId))
                return;

            CallLocally.Add(player.Profile.AccountId, true);

            bool pressed = bool.Parse(dict["pr"].ToString());

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    firearmCont.SetTriggerPressed(pressed);
                    //var weaponEffectsManager
                    //    = (WeaponEffectsManager)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(EFT.Player.FirearmController), typeof(WeaponEffectsManager)).GetValue(firearmCont);
                    //if (weaponEffectsManager == null)
                    //    return;

                    if(pressed)
                    {

                        // weaponEffectsManager.PlayShotEffects(player.IsVisible, player.Distance);
                        //firearmCont.WeaponSoundPlayer.FireBullet(null, player.Position, UnityEngine.Vector3.zero, 1);

                        var weapon = player.TryGetItemInHands<EFT.InventoryLogic.Weapon>();
                        if (weapon != null)
                        {
                            firearmCont.WeaponSoundPlayer.FireBullet(null, player.Position, UnityEngine.Vector3.zero, 1, false, weapon.FireMode.FireMode == EFT.InventoryLogic.Weapon.EFireMode.fullauto);

                            var weaponEffectsManager
                                = (WeaponEffectsManager)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(EFT.Player.FirearmController), typeof(WeaponEffectsManager)).GetValue(firearmCont);
                            if (weaponEffectsManager == null)
                                return;
                            weaponEffectsManager.PlayShotEffects(player.IsVisible, player.Distance);
                        }


                    }
                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
        }

        //public static void ReplicatePressed(EFT.Player player, bool pressed)
        //{
        //    if (player.HandsController is EFT.Player.FirearmController firearmCont && pressed)
        //    {
        //        try
        //        {
        //            var weaponEffectsManager
        //                = (WeaponEffectsManager)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(EFT.Player.FirearmController), typeof(WeaponEffectsManager)).GetValue(firearmCont);
        //            if (weaponEffectsManager == null)
        //                return;

        //            weaponEffectsManager.PlayShotEffects(player.IsVisible, player.Distance);
        //            firearmCont.WeaponSoundPlayer.FireBullet(null, player.Position, UnityEngine.Vector3.zero, 1);
        //        }
        //        catch (Exception e)
        //        {
        //            Logger.LogInfo(e);
        //        }
        //    }
        //}
    }
}
