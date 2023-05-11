using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
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

        public static Dictionary<string, bool> CallLocally
            = new Dictionary<string, bool>();

        public static Dictionary<string, bool> LastPress = new Dictionary<string, bool>();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            , bool pressed
            )
        {
            return true;
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

            //var ticks = DateTime.Now.Ticks;
            //Dictionary<string, object> dictionary = new Dictionary<string, object>();
            //dictionary.Add("t", ticks);
            //dictionary.Add("pr", pressed.ToString());
            //dictionary.Add("m", "SetTriggerPressed");
            //ServerCommunication.PostLocalPlayerData(player, dictionary);

            if(player.TryGetComponent<PlayerReplicatedComponent>(out var component))
            {
                component.TriggerPressed = pressed;
            }

        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                return;

            if (!prc.IsClientDrone)
                return;

            bool pressed = bool.Parse(dict["pr"].ToString());

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    var weaponEffectsManager
                        = (WeaponEffectsManager)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(EFT.Player.FirearmController), typeof(WeaponEffectsManager)).GetValue(firearmCont);
                    if (weaponEffectsManager == null)
                        return;

                    weaponEffectsManager.PlayShotEffects(player.IsVisible, player.Distance);
                    firearmCont.WeaponSoundPlayer.FireBullet(null, player.Position, UnityEngine.Vector3.zero, 1);
                    //ReflectionHelpers.GetMethodForType(typeof(EFT.Player.FirearmController), "method_52").Invoke()
                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
        }

        public static void ReplicatePressed(EFT.Player player, bool pressed)
        {
            if (player.HandsController is EFT.Player.FirearmController firearmCont && pressed)
            {
                try
                {
                    var weaponEffectsManager
                        = (WeaponEffectsManager)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(EFT.Player.FirearmController), typeof(WeaponEffectsManager)).GetValue(firearmCont);
                    if (weaponEffectsManager == null)
                        return;

                    weaponEffectsManager.PlayShotEffects(player.IsVisible, player.Distance);
                    firearmCont.WeaponSoundPlayer.FireBullet(null, player.Position, UnityEngine.Vector3.zero, 1);
                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
        }
    }
}
