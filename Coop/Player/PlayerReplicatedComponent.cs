#pragma warning disable CS0618 // Type or member is obsolete
using Newtonsoft.Json;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SIT.Coop.Core.Player
{
    /// <summary>
    /// Player Replicated Component is the Player/AI direct communication to the Server
    /// </summary>
    internal class PlayerReplicatedComponent : MonoBehaviour
    {
        internal const int PacketTimeoutInSeconds = 1;
        //internal ConcurrentQueue<Dictionary<string, object>> QueuedPackets { get; } = new();
        internal Dictionary<string, object> LastMovementPacket { get; set; }
        internal EFT.LocalPlayer player { get; set; }
        public float LastTiltLevel { get; private set; }
        public bool IsMyPlayer { get; internal set; }
        public bool IsClientDrone { get; internal set; }

        void Awake()
        {
            PatchConstants.Logger.LogDebug("PlayerReplicatedComponent:Awake");
        }

        void Start()
        {
            PatchConstants.Logger.LogDebug($"PlayerReplicatedComponent:Start");

            //StartCoroutine(HandleQueuedPackets());
            if (player == null)
            {
                player = this.GetComponentInParent<EFT.LocalPlayer>();
                PatchConstants.Logger.LogDebug($"PlayerReplicatedComponent:Start:Set Player to {player}");
            }

            GCHelpers.EnableGC();

            //StartCoroutine(DoManualUpdateSequence());
        }

        private IEnumerator DoManualUpdateSequence()
        {
            var waitEndOfFrame = new WaitForEndOfFrame();
            var waitFor = new WaitForSeconds(1);
            while (true)
            {
                //yield return waitFor;

                if (player == null)
                    continue;

                //UpdateMovementSend();

                //if (player.IsAI)
                //{
                //    if (!LastPosition.HasValue)
                //        LastPosition = player.Position;

                //    if (Vector3.Distance(LastPosition.Value, player.Position) > 3)
                //    {
                //        LastPosition = player.Position;
                //    }
                //}

                yield return waitEndOfFrame;
            }
        }

        public void HandlePacket(Dictionary<string, object> packet)
        {
            var method = packet["m"].ToString();

            var patch = ModuleReplicationPatch.Patches.FirstOrDefault(x=>x.MethodName == method);
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

            }
        }


        void Update()
        {
            //if (player == null)
            //    return;

            //if (ReplicatedDirection.HasValue)
            //{
            //    player.InputDirection = ReplicatedDirection.Value;
            //    player.CurrentState.Move(player.InputDirection);
            //}

            //if (ReplicatedRotation.HasValue && ReplicatedRotationClamp.HasValue)
            //{
            //    if (!ReplicatedRotation.Value.IsAnyComponentInfinity() && !ReplicatedRotation.Value.IsAnyComponentNaN())
            //    {
            //        //player.CurrentState.Rotate(ReplicatedRotation.Value, ReplicatedRotationClamp.HasValue);
            //        player.Rotation = ReplicatedRotation.Value;
            //    }
            //}

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
                if (LastPosition != player.Position)
                {
                    if (Vector3.Distance(LastPosition, player.Position) > 0.5)
                    {
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        dict.Add("pX", LastPosition.x);
                        dict.Add("pY", LastPosition.y);
                        dict.Add("pZ", LastPosition.z);
                        dict.Add("m", "Position");
                        ServerCommunication.PostLocalPlayerData(player, dict);
                    }
                    LastPosition = player.Position;
                }

                //if (LastRotator != player.Rotation)
                //{
                //    if (Vector3.Distance(LastRotator, player.Rotation) > 0.01)
                //    {
                //        Dictionary<string, object> dict = new Dictionary<string, object>();
                //        dict.Add("rX", LastRotator.x);
                //        dict.Add("rY", LastRotator.y);
                //        dict.Add("m", "Rotation");
                //        ServerCommunication.PostLocalPlayerData(player, dict);
                //    }
                //    LastRotator = player.Rotation;
                //}

                //if (ReplicatedPosition.HasValue)
                //{
                //    player.Teleport(ReplicatedPosition.Value);
                //}
            }
        }

        private Vector2 LastDirection { get; set; } = Vector2.zero;
        private Vector2 LastRotator { get; set; } = Vector2.zero;
        private Vector3 LastPosition { get; set; } = Vector3.zero;
        public Vector2? ReplicatedDirection { get; internal set; }
        public Vector2? ReplicatedRotation { get; internal set; }
        public bool? ReplicatedRotationClamp { get; internal set; }
        public Vector3? ReplicatedPosition { get; internal set; }


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

        //private bool IsUpdatingMovementSend = false;
        //private void UpdateMovementSend()
        //{
        //    if(IsUpdatingMovementSend) 
        //        return;

        //    if (player == null)
        //        return;

        //    if (player.MovementContext == null)
        //        return;

        //    if (!player.IsAI && player.AIData != null)
        //        return;

        //    if (MatchmakerAcceptPatches.IsClient)
        //        return;

        //    IsUpdatingMovementSend = true;
        //    try
        //    {

        //        if (
        //            (LastDirection != player.InputDirection
        //            && Vector2.Dot(LastDirection, player.InputDirection) <= 0.5)
        //            || Vector2.Dot(LastRotator, player.Rotation) < 0
        //            )
        //        {
        //            PreMadeMoveDataPacket["dX"] = Math.Round(player.InputDirection.x, 2).ToString();
        //            PreMadeMoveDataPacket["dY"] = Math.Round(player.InputDirection.y, 2).ToString();
        //            PreMadeMoveDataPacket["rX"] = Math.Round(player.Rotation.x, 2).ToString();
        //            PreMadeMoveDataPacket["rY"] = Math.Round(player.Rotation.y, 2).ToString();
        //            //PreMadeMoveDataPacket["pX"] = Math.Round(player.Position.x, 2).ToString();
        //            //PreMadeMoveDataPacket["pY"] = Math.Round(player.Position.y, 2).ToString();
        //            //PreMadeMoveDataPacket["pZ"] = Math.Round(player.Position.z, 2).ToString();
        //            PreMadeMoveDataPacket["t"] = DateTime.Now.Ticks;
        //            ServerCommunication.PostLocalPlayerData(player, PreMadeMoveDataPacket);
        //            LastDirection = player.InputDirection;
        //            LastRotator = player.Rotation;
        //        }

        //        //if (this.LastTiltLevel != player.MovementContext.Tilt
        //        //    && (this.LastTiltLevel - player.MovementContext.Tilt > 0.05f || this.LastTiltLevel - player.MovementContext.Tilt < -0.05f)
        //        //    )
        //        //{
        //        //    this.LastTiltLevel = player.MovementContext.Tilt;
        //        //    PreMadeTiltDataPacket["tilt"] = LastTiltLevel;
        //        //    ServerCommunication.PostLocalPlayerData(player, PreMadeTiltDataPacket);
        //        //}
        //    }
        //    finally
        //    {
        //        IsUpdatingMovementSend = false;
        //    }
        //}
    }
}
