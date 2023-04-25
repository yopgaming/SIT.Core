#pragma warning disable CS0618 // Type or member is obsolete
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Core.Coop.Components;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SIT.Coop.Core.Player
{
    /// <summary>
    /// Player Replicated Component is the Player/AI direct communication to the Server
    /// </summary>
    internal class PlayerReplicatedComponent : MonoBehaviour, IPlayerPacketHandlerComponent
    {
        internal const int PacketTimeoutInSeconds = 1;
        //internal ConcurrentQueue<Dictionary<string, object>> QueuedPackets { get; } = new();
        internal Dictionary<string, object> LastMovementPacket { get; set; }
        internal EFT.LocalPlayer player { get; set; }
        public bool IsMyPlayer { get { return player != null && player.IsYourPlayer; } }
        public bool IsClientDrone { get; internal set; }

        void Awake()
        {
            PatchConstants.Logger.LogDebug("PlayerReplicatedComponent:Awake");
        }

        void Start()
        {
            PatchConstants.Logger.LogDebug($"PlayerReplicatedComponent:Start");

            if (player == null)
            {
                player = this.GetComponentInParent<EFT.LocalPlayer>();
                PatchConstants.Logger.LogDebug($"PlayerReplicatedComponent:Start:Set Player to {player}");
            }

            GCHelpers.EnableGC();
        }

        public void HandlePacket(Dictionary<string, object> packet)
        {
            var method = packet["m"].ToString();

            var patch = ModuleReplicationPatch.Patches.FirstOrDefault(x => x.MethodName == method);
            if (patch != null)
            {
                patch.Replicated(player, packet);
                return;
            }

            switch (method)
            {
                case "PlayerState":
                    if (IsClientDrone)
                    {
                        // Pose
                        float poseLevel = float.Parse(packet["pose"].ToString());
                        player.MovementContext.SetPoseLevel(poseLevel, true);
                        // Speed
                        float speed = float.Parse(packet["spd"].ToString());
                        //player.ChangeSpeed(speed);
                        player.MovementContext.CharacterMovementSpeed = speed;
                        // Rotation
                        Vector2 packetRotation = new Vector2(
                        float.Parse(packet["rX"].ToString())
                        , float.Parse(packet["rY"].ToString())
                        );
                        //player.Rotation = packetRotation;
                        ReplicatedRotation = packetRotation;
                        // Position
                        Vector3 packetPosition = new Vector3(
                            float.Parse(packet["pX"].ToString())
                            , float.Parse(packet["pY"].ToString())
                            , float.Parse(packet["pZ"].ToString())
                            );
                        //if (Vector3.Distance(packetPosition, player.Position) > 1)
                        //    player.Teleport(packetPosition, true);
                        ReplicatedPosition = packetPosition;
                        // Move / Direction
                        if (packet.ContainsKey("dX"))
                        {
                            Vector2 packetDirection = new Vector2(
                            float.Parse(packet["dX"].ToString())
                            , float.Parse(packet["dY"].ToString())
                            );
                            //player.Move(packetDirection);
                            player.CurrentState.Move(packetDirection);
                            player.InputDirection = packetDirection;
                            ReplicatedDirection = packetDirection;
                        }
                    }
                    break;

            }

            var packetHandlerComponents = this.GetComponents<IPlayerPacketHandlerComponent>();
            if (packetHandlerComponents != null)
            {
                packetHandlerComponents = packetHandlerComponents.Where(x => x.GetType() != typeof(PlayerReplicatedComponent)).ToArray();
                foreach (var packetHandlerComponent in packetHandlerComponents)
                {
                    packetHandlerComponent.HandlePacket(packet);
                }
            }
        }

        void LateUpdate()
        {
            if (IsClientDrone)
            {
                // Replicate Position.
                // If a short distance -> Smooth Lerp to the Desired Position
                // If the other side of a wall -> Teleport to the correct side (TODO)
                // If far away -> Teleport
                if (ReplicatedPosition.HasValue)
                {
                    var replicationDistance = Vector3.Distance(ReplicatedPosition.Value, player.Position);
                    var replicatedPositionDirection = ReplicatedPosition.Value - player.Position;
                    if (replicationDistance >= 3)
                    {
                        player.Teleport(ReplicatedPosition.Value, true);
                    }
                    else
                    {
                        player.Position = Vector3.Lerp(player.Position, ReplicatedPosition.Value, Time.deltaTime);
                    }
                }

                // Replicate Rotation.
                // Smooth Lerp to the Desired Rotation
                if (ReplicatedRotation.HasValue)
                {
                    player.Rotation = Vector3.Lerp(player.Rotation, ReplicatedRotation.Value, Time.deltaTime * 8);
                }

                if (ReplicatedDirection.HasValue)
                {
                    player.CurrentState.Move(ReplicatedDirection.Value);
                    player.InputDirection = ReplicatedDirection.Value;
                }
            }

            if (IsClientDrone)
                return;

            if (LastPlayerStateSent < DateTime.Now.AddSeconds(-1))
            {

                Dictionary<string, object> dictPlayerState = new Dictionary<string, object>();
                if (ReplicatedDirection.HasValue)
                {
                    dictPlayerState.Add("dX", ReplicatedDirection.Value.x);
                    dictPlayerState.Add("dY", ReplicatedDirection.Value.y);
                }
                dictPlayerState.Add("pX", player.Position.x);
                dictPlayerState.Add("pY", player.Position.y);
                dictPlayerState.Add("pZ", player.Position.z);
                dictPlayerState.Add("rX", player.Rotation.x);
                dictPlayerState.Add("rY", player.Rotation.y);
                dictPlayerState.Add("pose", player.MovementContext.PoseLevel);
                dictPlayerState.Add("spd", player.MovementContext.CharacterMovementSpeed);
                dictPlayerState.Add("spr", player.MovementContext.IsSprintEnabled);
                dictPlayerState.Add("m", "PlayerState");
                ServerCommunication.PostLocalPlayerData(player, dictPlayerState);

                LastPlayerStateSent = DateTime.Now;
            }
        }

        private Vector2 LastDirection { get; set; } = Vector2.zero;
        private DateTime LastDirectionSent { get; set; } = DateTime.Now;
        private Vector2 LastRotation { get; set; } = Vector2.zero;
        private DateTime LastRotationSent { get; set; } = DateTime.Now;
        private Vector3 LastPosition { get; set; } = Vector3.zero;
        private DateTime LastPositionSent { get; set; } = DateTime.Now;
        public Vector2? ReplicatedDirection { get; internal set; }
        public Vector2? ReplicatedRotation { get; internal set; }
        public bool? ReplicatedRotationClamp { get; internal set; }
        public Vector3? ReplicatedPosition { get; internal set; }
        public DateTime LastPoseSent { get; private set; }
        public float LastPose { get; private set; }
        public DateTime LastSpeedSent { get; private set; }
        public float LastSpeed { get; private set; }
        public DateTime LastPlayerStateSent { get; private set; } = DateTime.Now;

        public Dictionary<string, object> PreMadeMoveDataPacket = new()
        {
            { "dX", "0" },
            { "dY", "0" },
            { "rX", "0" },
            { "rY", "0" },
            { "m", "Move" }
        };
        public Dictionary<string, object> PreMadeTiltDataPacket = new()
        {
            { "tilt", "0" },
            { "m", "Tilt" }
        };

        public bool IsAI()
        {
            return player.IsAI && !player.Profile.Id.StartsWith("pmc");
        }

        public bool IsOwnedPlayer()
        {
            return player.Profile.Id.StartsWith("pmc") && !IsClientDrone;
        }
    }
}
