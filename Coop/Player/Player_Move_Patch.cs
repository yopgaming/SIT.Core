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
    internal class Player_Move_Patch : ModuleReplicationPatch
    {
        private static Dictionary<string, UnityEngine.Vector2> lastDirection = new();

        private static List<long> ProcessedCalls = new();

        public static Dictionary<string, bool> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "Move";
        //public override bool DisablePatch => true;

        public static Request RequestInstance = null;

        public Player_Move_Patch()
        {
            RequestInstance = Request.GetRequestInstance(true, Logger);
        }

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            if (__instance.IsAI || __instance.AIData != null)
                return true;

            var result = false;
            //if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            //    result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
           UnityEngine.Vector2 direction
            )
        {
            if (__instance.IsAI || __instance.AIData != null)
                return;

            var player = __instance;

            //if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            //{
            //    CallLocally.Remove(player.Profile.AccountId);
            //    return;
            //}

            if (lastDirection.ContainsKey(player.Profile.AccountId))
            {
                if (lastDirection[player.Profile.AccountId] == direction)
                    //|| Vector2.Dot(lastDirection[player.Profile.AccountId], direction) >= 0.75)
                    return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("dX", direction.x.ToString());
            dictionary.Add("dY", direction.y.ToString());
            //dictionary.Add("pX", __instance.Position.x.ToString());
            //dictionary.Add("pY", __instance.Position.y.ToString());
            //dictionary.Add("pZ", __instance.Position.z.ToString());
            dictionary.Add("m", "Move");
            ServerCommunication.PostLocalPlayerData(player, dictionary, RequestInstance);

            if (!lastDirection.ContainsKey(player.Profile.AccountId))
                lastDirection.Add(player.Profile.AccountId, direction);

            lastDirection[player.Profile.AccountId] = direction;
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

            //if (CallLocally.ContainsKey(player.Profile.AccountId))
            //    return;

            //Logger.LogDebug($"Replicated Move {player.Profile.AccountId}");
            try
            {
                UnityEngine.Vector2 direction = new UnityEngine.Vector2(float.Parse(dict["dX"].ToString()), float.Parse(dict["dY"].ToString()));
                //CallLocally.Add(player.Profile.AccountId, true);
                //player.Move(direction);
                if (player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent playerReplicatedComponent))
                {
                    playerReplicatedComponent.ReplicatedDirection = direction;
                }
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }
        }
    }
}
