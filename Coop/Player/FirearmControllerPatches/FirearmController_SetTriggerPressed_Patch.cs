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
            var result = false;
            if (CallLocally.TryGetValue(____player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            if (LastPress.ContainsKey(____player.Profile.AccountId) && LastPress[____player.Profile.AccountId] == pressed)
                return true;

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
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", ticks);
            dictionary.Add("pr", pressed.ToString());
            dictionary.Add("m", "SetTriggerPressed");
            ServerCommunication.PostLocalPlayerData(player, dictionary);

            if (!LastPress.ContainsKey(player.Profile.AccountId))
                LastPress.Add(player.Profile.AccountId, pressed);

            LastPress[player.Profile.AccountId] = pressed;

            var timestamp = ticks;
            // then instantly trigger? otherwise we are waiting for the return trip?
            CallLocally.Add(player.Profile.AccountId, true);
            __instance.SetTriggerPressed(pressed);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            bool pressed = bool.Parse(dict["pr"].ToString());

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    CallLocally.Add(player.Profile.AccountId, true);
                    firearmCont.SetTriggerPressed(pressed);
                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
        }
    }
}
