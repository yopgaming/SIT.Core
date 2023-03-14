using EFT;
using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.Player
{
    internal class Player_Rotate_Patch : ModuleReplicationPatch
    {
        private static Dictionary<string, UnityEngine.Vector2> lastDirection = new();

        private static Dictionary<string, UnityEngine.Vector2> lastDelta = new();

        private static List<long> ProcessedCalls = new();

        public static List<string> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "Rotate";

        public override bool DisablePatch => true;

        public static Request RequestInstance = null;

        public Player_Rotate_Patch()
        {
            RequestInstance = Request.GetRequestInstance(true, Logger);
        }

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

            return method;
        }

        [PatchPrefix]
        public static bool Prefix(
          EFT.Player __instance,
          Vector2 deltaRotation, bool ignoreClamp = false
           )
        {
            if (!lastDelta.ContainsKey(__instance.Profile.AccountId))
            {
                lastDelta.Add(__instance.Profile.AccountId, deltaRotation);
                return true;
            }

            if (lastDirection[__instance.Profile.AccountId] == deltaRotation)
                return false;


            lastDelta[__instance.Profile.AccountId] = deltaRotation;
            return true;
        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
           Vector2 deltaRotation, bool ignoreClamp = false
            )
        {
            var player = __instance;

            if (lastDirection.ContainsKey(player.Profile.AccountId))
            {
                if (lastDirection[player.Profile.AccountId] == __instance.Rotation
                    )
                    return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("rX", __instance.Rotation.x.ToString());
            dictionary.Add("rY", __instance.Rotation.y.ToString());
            dictionary.Add("m", "Rotate");
            ServerCommunication.PostLocalPlayerData(player, dictionary, RequestInstance);

            if (!lastDirection.ContainsKey(player.Profile.AccountId))
                lastDirection.Add(player.Profile.AccountId, __instance.Rotation);

            lastDirection[player.Profile.AccountId] = __instance.Rotation;
        }

       
        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());

            if (!ProcessedCalls.Contains(timestamp))
                ProcessedCalls.Add(timestamp);
            else
            {
                ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddHours(-1).Ticks);
                return;
            }

            Logger.LogDebug($"Replicated Rotate {player.Profile.AccountId}");
            try
            {
                UnityEngine.Vector2 rotation = new UnityEngine.Vector2(float.Parse(dict["rX"].ToString()), float.Parse(dict["rY"].ToString()));
                if (player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent playerReplicatedComponent))
                {
                    playerReplicatedComponent.ReplicatedRotation = rotation;
                }
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }
        }
    }
}
