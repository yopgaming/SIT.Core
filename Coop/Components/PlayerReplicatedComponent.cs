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
        public float LastTiltLevel { get; private set; }
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
                case "Position":
                    if (IsClientDrone)
                    {
                        Vector3 packetPosition = new Vector3(
                            float.Parse(packet["pX"].ToString())
                            , float.Parse(packet["pY"].ToString())
                            , float.Parse(packet["pZ"].ToString())
                            );
                        player.Teleport(packetPosition, true);
                    }
                    break;
                case "Rotation":
                    if (IsClientDrone)
                    {
                        Vector2 packetRotation = new Vector2(
                        float.Parse(packet["rX"].ToString())
                        , float.Parse(packet["rY"].ToString())
                        );
                        player.Rotation = packetRotation;
                    }
                    break;
                case "Pose":
                    if (IsClientDrone)
                    {
                        float poseLevel = float.Parse(packet["pose"].ToString());
                        player.ChangePose(poseLevel);
                    }
                    break;
                case "Speed":
                    if (IsClientDrone)
                    {
                        float speed = float.Parse(packet["spd"].ToString());
                        player.ChangeSpeed(speed);
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
            if (ReplicatedDirection.HasValue)
            {
                player.InputDirection = ReplicatedDirection.Value;
                player.CurrentState.Move(player.InputDirection);
            }

            if (IsClientDrone)
                return;


            if (player.IsAI)
            {
                // if the character is really far away from last position, send immediately
                var repositionTimeout = Math.Max(0, -3 - Vector3.Distance(LastPosition, player.Position));
                if (Vector3.Distance(LastPosition, player.Position) > 0.5 && LastPositionSent < DateTime.Now.AddSeconds(repositionTimeout))
                {
                    if (Vector3.Distance(LastPosition, player.Position) > 0.5)
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("pX", LastPosition.x);
                        dict.Add("pY", LastPosition.y);
                        dict.Add("pZ", LastPosition.z);
                        dict.Add("m", "Position");
                        ServerCommunication.PostLocalPlayerData(player, dict);

                        LastPositionSent = DateTime.Now;
                        LastPosition = player.Position;
                    }
                }

                if (ReplicatedDirection.HasValue)
                {
                    if (Vector2.Dot(LastDirection, ReplicatedDirection.Value) < 1 && LastDirectionSent < DateTime.Now.AddSeconds(-0.5))
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("dX", ReplicatedDirection.Value.x);
                        dict.Add("dY", ReplicatedDirection.Value.y);
                        dict.Add("m", "Move");
                        ServerCommunication.PostLocalPlayerData(player, dict);

                        LastDirectionSent = DateTime.Now;
                        LastDirection = player.Position;
                    }
                }

                if (Vector2.Dot(LastRotation, player.Rotation) < 1 && LastRotationSent < DateTime.Now.AddSeconds(-0.5))
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("rX", player.Rotation.x);
                    dict.Add("rY", player.Rotation.y);
                    dict.Add("m", "Rotation");
                    ServerCommunication.PostLocalPlayerData(player, dict);

                    LastRotationSent = DateTime.Now;
                    LastRotation = player.Rotation;
                }

                if (LastPose != player.PoseLevel && LastPoseSent < DateTime.Now.AddSeconds(-1))
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("pose", player.PoseLevel);
                    dict.Add("m", "Pose");
                    ServerCommunication.PostLocalPlayerData(player, dict);

                    LastPoseSent = DateTime.Now;
                    LastPose = player.PoseLevel;
                }

                if (LastSpeed != player.Speed && LastSpeedSent < DateTime.Now.AddSeconds(-1))
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    dict.Add("spd", player.Speed);
                    dict.Add("m", "Speed");
                    ServerCommunication.PostLocalPlayerData(player, dict);

                    LastSpeedSent = DateTime.Now;
                    LastSpeed = player.Speed;
                }
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
