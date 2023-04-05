using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Concurrent;
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

        private static ConcurrentDictionary<string, long> ProcessedCalls = new();

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
        public static bool PrePatch(
          EFT.Player __instance,
          UnityEngine.Vector2 direction
           )
        {
            var player = __instance;
            var accountId = player.Profile.AccountId;

            // If this player is a Client drone, do not send any data, anywhere
            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                    return false;

                if (prc.IsAI()) // AI Dude do their own logic because shitty AI logic causes LOADS of calls : TODO: Write logic 
                {
                    prc.ReplicatedDirection = direction;
                    return false;
                }

                if (!prc.IsOwnedPlayer()) // If it isn't an owned player (i.e. you are controlling them) then you shouldnt send
                    return false;
            }

            return true;

        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
           UnityEngine.Vector2 direction
            )
        {
            var player = __instance;
            var accountId = player.Profile.AccountId;

            //if (lastDirection.ContainsKey(accountId) && Vector3.Dot(direction, lastDirection[accountId]) >= 0)
            //    return;

            // If this player is a Client drone, do not send any data, anywhere
            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                    return;

                if (prc.IsAI()) // AI Dude do their own logic because shitty AI logic causes LOADS of calls : TODO: Write logic
                {
                    AIProcess(player, direction);
                    return;
                }

                if (!prc.IsOwnedPlayer()) // If it isn't an owned player (i.e. you are controlling them) then you shouldnt send
                    return;
            }

            // AI cannot use this pattern
            if (player.IsAI || !player.IsYourPlayer)
            {
                AIProcess(__instance, direction);
                return;
            }

            //if(!lastDirection.ContainsKey(accountId))
            //    lastDirection.Add(accountId, direction);

            //lastDirection[accountId] = direction;

            //Dictionary<string, object> dictionary = new Dictionary<string, object>();
            //dictionary.Add("t", DateTime.Now.Ticks);
            //dictionary.Add("dX", direction.x.ToString());
            //dictionary.Add("dY", direction.y.ToString());
            //dictionary.Add("m", "Move");
            //ServerCommunication.PostLocalPlayerData(player, dictionary, RequestInstance);



        }

        public static void AIProcess(
           EFT.Player player,
           UnityEngine.Vector2 direction
            )
        {
            //Dictionary<string, object> dictionary = new Dictionary<string, object>();
            //dictionary.Add("t", DateTime.Now.Ticks);
            //dictionary.Add("dX", direction.x.ToString());
            //dictionary.Add("dY", direction.y.ToString());
            //dictionary.Add("m", "Move");
            //ServerCommunication.PostLocalPlayerData(player, dictionary, RequestInstance);

        }

        private bool HasProcessed(EFT.Player player, Dictionary<string, object> dict)
        {
            var playerID = player.Id.ToString();
            var timestamp = long.Parse(dict["t"].ToString());
            if (!ProcessedCalls.ContainsKey(playerID))
            {
                Logger.LogDebug($"Adding {playerID},{timestamp} to {this.GetType()} Processed Calls Dictionary");
                ProcessedCalls.TryAdd(playerID, timestamp);
                return true;
            }

            if (ProcessedCalls[playerID] != timestamp)
            {
                ProcessedCalls.TryUpdate(playerID, timestamp, timestamp);
                return false;
            }

            return false;
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if(HasProcessed(player, dict))
                return;

            try
            {
                UnityEngine.Vector2 direction = new UnityEngine.Vector2(float.Parse(dict["dX"].ToString()), float.Parse(dict["dY"].ToString()));
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
