using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static EFT.UI.CharacterSelectionStartScreen;

namespace SIT.Core.Coop.Player
{

    /// <summary>
    /// Move does not work in a traditional MRP as Bots call this function every frame. Only Players can use this MRP.
    /// </summary>
    internal class Player_Move_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "Move";

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
          ref UnityEngine.Vector2 direction
           )
        {
            // FIX: Error that occurs when leaving a raid
            if (__instance == null)
                return false;

            var player = __instance;
            var accountId = player.Profile.AccountId;

            // If this player is a Client drone, do not send any data, anywhere
            var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
            if (prc.IsClientDrone)
                return false;

            //direction.x = (float)Math.Round(direction.x, 3);
            //direction.y = (float)Math.Round(direction.y, 3);

            return true;

        }

        public static Dictionary<string, Vector2> LastDirections { get; } = new();

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
           ref UnityEngine.Vector2 direction
            )
        {
            var player = __instance;
            var accountId = player.Profile.AccountId;

            if (!player.IsYourPlayer)
            {
                if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                    prc.ReplicatedDirection = direction;
                return;
            }

            //direction.x = (float)Math.Round(direction.x, 3);
            //direction.y = (float)Math.Round(direction.y, 3);

            if (!LastDirections.ContainsKey(accountId))
                LastDirections.Add(accountId, direction);
            else if (LastDirections[accountId] == direction && direction == Vector2.zero)
                return;

            //Dictionary<string, object> dictionary = new Dictionary<string, object>();
            //dictionary.Add("t", DateTime.Now.Ticks);
            //dictionary.Add("dX", direction.x.ToString());
            //dictionary.Add("dY", direction.y.ToString());
            //dictionary.Add("spd", player.MovementContext.CharacterMovementSpeed);
            //dictionary.Add("m", "Move");
            //ServerCommunication.PostLocalPlayerData(player, dictionary);

            PlayerMovePacket playerMovePacket = new PlayerMovePacket();
            playerMovePacket.AccountId = accountId;
            playerMovePacket.dX = direction.x;
            playerMovePacket.dY = direction.y;
            playerMovePacket.spd = player.MovementContext.CharacterMovementSpeed;
            playerMovePacket.spr = player.MovementContext.IsSprintEnabled;
            Request.Instance.SendDataToPool(playerMovePacket.ToJson());
            LastDirections[accountId] = direction;
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent playerReplicatedComponent))
            {
                if (playerReplicatedComponent.IsClientDrone)
                {
                    UnityEngine.Vector2 direction = new UnityEngine.Vector2(float.Parse(dict["dX"].ToString()), float.Parse(dict["dY"].ToString()));
                    float spd = float.Parse(dict["spd"].ToString());
                    bool spr = bool.Parse(dict["spr"].ToString());
                    playerReplicatedComponent.ReplicatedDirection = null;
                    playerReplicatedComponent.ReplicatedPosition = null;
                    if (player.IsSprintEnabled) 
                    {
                        if (!spr)
                        {
                            player.MovementContext.EnableSprint(false);
                        }
                    } else
                    {
                        if (spr)
                        {
                            player.MovementContext.EnableSprint(true);
                        }
                    }
                    player.CurrentState.ChangeSpeed(spd);
                    player.CurrentState.Move(direction);
                    player.InputDirection = direction;
                }
            }
        }

        public class PlayerMovePacket : BasePlayerPacket
        {
            public float dX { get; set; }
            public float dY { get; set; }
            public float spd { get; set; }
            public bool spr { get; set; }

            public PlayerMovePacket() : base() 
            {
                Method = "Move";
            }

        }
    }
}
