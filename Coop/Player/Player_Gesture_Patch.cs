using SIT.Coop.Core.Web;
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
    internal class Player_Gesture_Patch : ModuleReplicationPatch
    {
        private static Dictionary<string, EGesture> LastGesture = new();
        public static List<string> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "Gesture";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, "vmethod_4");
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.Contains(__instance.Profile.AccountId))
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
            EGesture gesture
            )
        {
            var player = __instance;

            if (LastGesture.ContainsKey(player.Profile.AccountId))
            {
                if (LastGesture[player.Profile.AccountId] == gesture)
                    return;
            }

            if (CallLocally.Contains(player.Profile.AccountId))
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("g", gesture.ToString());
            dictionary.Add("m", "Gesture");
            ServerCommunication.PostLocalPlayerData(player, dictionary);

            if (!LastGesture.ContainsKey(player.Profile.AccountId))
                LastGesture.Add(player.Profile.AccountId, gesture);

            LastGesture[player.Profile.AccountId] = gesture;
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());

            if (HasProcessed(GetType(), player, dict))
                return;

            if (CallLocally.Contains(player.Profile.AccountId))
                return;

            try
            {
                CallLocally.Add(player.Profile.AccountId);
                if (!player.HandsController.IsInInteractionStrictCheck() && Enum.TryParse<EGesture>(dict["g"].ToString(), out var g))
                {
                    player.HandsController.ShowGesture(g);
                }
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }

        }
    }
}
