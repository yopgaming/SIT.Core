using SIT.Coop.Core.Player;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
            //{
            //    prc.ReplicatedDirection = direction;
            //    return;
            //}

            if (prc.IsClientDrone)
                return;

            //if (!LastDirections.ContainsKey(accountId))
            //    LastDirections.Add(accountId, direction);
            //else if (LastDirections[accountId] == direction)
            //    return;

            PlayerMovePacket playerMovePacket = new();
            playerMovePacket.AccountId = accountId;
            playerMovePacket.pX = player.Position.x;
            playerMovePacket.pY = player.Position.y;
            playerMovePacket.pZ = player.Position.z;

            playerMovePacket.dX = direction.x;
            playerMovePacket.dY = direction.y;

            playerMovePacket.spd = player.MovementContext.CharacterMovementSpeed;
            //playerMovePacket.spr = player.MovementContext.IsSprintEnabled;
            var serialized = playerMovePacket.Serialize();
            //AkiBackendCommunication.Instance.SendDataToPool(playerMovePacket.ToJson());
            //AkiBackendCommunication.Instance.PostDownWebSocketImmediately(serialized);
            AkiBackendCommunication.Instance.SendDataToPool(serialized);
            //LastDirections[accountId] = direction;
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            // Player Moves happen too often for this check. This would be a large mem leak!
            //if (HasProcessed(this.GetType(), player, dict))
            //    return;

            PlayerMovePacket pmp = new();
            if (dict.ContainsKey("data"))
            {
                pmp = new PlayerMovePacket();
                pmp.DeserializePacketSIT(dict["data"].ToString());
            }
            else
            {
                pmp = new Player_Move_Patch.PlayerMovePacket()
                {
                    dX = float.Parse(dict["dX"].ToString()),
                    dY = float.Parse(dict["dY"].ToString()),
                    spd = float.Parse(dict["spd"].ToString()),
                    //spr = bool.Parse(dict["spr"].ToString()),
                };
            }
            ReplicatedMove(player, pmp);

            pmp = null;
            dict = null;
        }

        public void ReplicatedMove(EFT.Player player, PlayerMovePacket playerMovePacket)
        {
            if (player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent playerReplicatedComponent))
            {
                if (playerReplicatedComponent.IsClientDrone)
                {
                    if (playerMovePacket.pX != 0 && playerMovePacket.pY != 0 && playerMovePacket.pZ != 0)
                    {
                        //player.Teleport(new Vector3(playerMovePacket.pX, playerMovePacket.pY, playerMovePacket.pZ));
                        var ReplicatedPosition = new Vector3(playerMovePacket.pX, playerMovePacket.pY, playerMovePacket.pZ);
                        var replicationDistance = Vector3.Distance(ReplicatedPosition, player.Position);
                        if (replicationDistance >= 3)
                        {
                            player.Teleport(ReplicatedPosition, true);
                        }
                        else
                        {
                            player.Position = Vector3.Lerp(player.Position, ReplicatedPosition, Time.deltaTime * 7);
                        }
                    }

                    UnityEngine.Vector2 direction = new(playerMovePacket.dX, playerMovePacket.dY);
                    float spd = playerMovePacket.spd;
                    //bool spr = playerMovePacket.spr;
                    //playerReplicatedComponent.ShouldSprint = spr;
                    playerReplicatedComponent.ReplicatedMovementSpeed = spd;
                    playerReplicatedComponent.ReplicatedDirection = null;

                    player.InputDirection = direction;
                    player.MovementContext.MovementDirection = direction;

                    //if (!spr)
                    //{
                    //player.CurrentManagedState.ChangeSpeed(spd);
                    player.MovementContext.CharacterMovementSpeed = spd;
                    //}

                    //if (spr)
                    //{
                    //    //Logger.LogInfo(player.CurrentManagedState.GetType().Name);
                    //    //Logger.LogInfo("Enabling Sprint");
                    //    //player.CurrentManagedState.EnableSprint(spr, true);
                    //    //player.Physical.Sprint(spr);
                    //    //player.MovementContext.PlayerAnimatorEnableSprint(true);
                    //}
                    //else if (!spr)
                    //{
                    //    //Logger.LogInfo("Disabling Sprint");
                    //    //player.Physical.Sprint(spr);
                    //    //player.CurrentManagedState.EnableSprint(spr, true);
                    //    //player.MovementContext.PlayerAnimatorEnableSprint(false);

                    //}


                    player.CurrentManagedState.Move(direction);

                    playerReplicatedComponent.ReplicatedDirection = direction;

                }
            }
        }

        public class PlayerMovePacket : BasePlayerPacket
        {
            public float pX { get; set; }
            public float pY { get; set; }
            public float pZ { get; set; }

            public float dX { get; set; }
            public float dY { get; set; }
            public float spd { get; set; }
            //public bool spr { get; set; }

            public PlayerMovePacket() : base()
            {
                Method = "Move";
            }

        }
    }
}
