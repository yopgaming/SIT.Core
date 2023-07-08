using EFT.InventoryLogic;
using SIT.Coop.Core.Player;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SIT.Core.Coop.Player.Player_Move_Patch;

namespace SIT.Core.Coop.Player
{

    /// <summary>
    /// Move does not work in a traditional MRP as Bots call this function every frame. Only Players can use this MRP.
    /// </summary>
    internal class Player_Move_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "Move";

        //public static Request RequestInstance = null;

        //public Player_Move_Patch()
        //{
        //    RequestInstance = Request.GetRequestInstance(true, Logger);
        //}

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
            // don't run this if we dont have a "player"
            if (__instance == null)
                return false;

            var player = __instance;

            // If this player is a Client drone, then don't run this method
            var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
            if (prc.IsClientDrone)
                return false;

            // If this is an AI or other player, then don't run this method
            // For some reason, this breaks the AI so they can't move. AI are becoming a pain to deal with.
            //if (!player.IsYourPlayer)
            //{
            //    return false;
            //}

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
            // don't run this if we dont have a "player"
            if (__instance == null)
                return;

            var accountId = player.Profile.AccountId;
            if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                Logger.LogError($"Unable to find PRC on {player.ProfileId}");
                return;
            }

            // If this is an AI or other player, then don't run this method
            //if (!player.IsYourPlayer)
            {
                prc.ReplicatedDirection = direction;
            }

            if (prc.IsClientDrone)
                return;
           
            if (!LastDirections.ContainsKey(accountId))
                LastDirections.Add(accountId, direction);
            else if (LastDirections[accountId] == direction && direction == Vector2.zero)
                return;

            PlayerMovePacket playerMovePacket = new PlayerMovePacket();
            playerMovePacket.AccountId = accountId;
            playerMovePacket.dX = direction.x;
            playerMovePacket.dY = direction.y;
            playerMovePacket.spd = player.MovementContext.CharacterMovementSpeed;
            playerMovePacket.spr = player.MovementContext.IsSprintEnabled;
            AkiBackendCommunication.Instance.SendDataToPool(playerMovePacket.ToJson());
            LastDirections[accountId] = direction;
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(this.GetType(), player, dict))
                return;

            var pmp = new Player_Move_Patch.PlayerMovePacket()
            {
                dX = float.Parse(dict["dX"].ToString()),
                dY = float.Parse(dict["dY"].ToString()),
                spd = float.Parse(dict["spd"].ToString()),
                spr = bool.Parse(dict["spr"].ToString()),
            };
            ReplicatedMove(player, pmp);
        }

        public void ReplicatedMove(EFT.Player player, PlayerMovePacket playerMovePacket)
        {
            if (player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent playerReplicatedComponent))
            {
                if (playerReplicatedComponent.IsClientDrone)
                {
                    UnityEngine.Vector2 direction = new UnityEngine.Vector2(playerMovePacket.dX, playerMovePacket.dY);
                    float spd = playerMovePacket.spd;
                    bool spr = playerMovePacket.spr;

                    player.InputDirection = direction;
                    if (!spr)
                    {
                        player.CurrentManagedState.ChangeSpeed(spd);
                    }

                    if (!player.IsSprintEnabled && spr)
                        player.CurrentManagedState.EnableSprint(spr, true);
                    else if (!spr && player.IsSprintEnabled)
                        player.CurrentManagedState.EnableSprint(spr, true);

                    player.CurrentManagedState.Move(direction);

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
