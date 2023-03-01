#pragma warning disable CS0618 // Type or member is obsolete
using Newtonsoft.Json;
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
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
    internal class PlayerReplicatedComponent : NetworkBehaviour
    {
        internal const int PacketTimeoutInSeconds = 1;
        //internal ConcurrentQueue<Dictionary<string, object>> QueuedPackets { get; } = new();
        internal Dictionary<string, object> LastMovementPacket { get; set; }
        internal EFT.LocalPlayer player { get; set; }
        public float LastTiltLevel { get; private set; }
        public bool IsMyPlayer { get; internal set; }

        void Awake()
        {
            PatchConstants.Logger.LogInfo("PlayerReplicatedComponent:Awake");
        }

        void Start()
        {
            PatchConstants.Logger.LogInfo($"PlayerReplicatedComponent:Start");

            //StartCoroutine(HandleQueuedPackets());
            if (player == null)
            {
                player = this.GetComponentInParent<EFT.LocalPlayer>();
                PatchConstants.Logger.LogInfo($"PlayerReplicatedComponent:Start:Set Player to {player}");
            }

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

        //private IEnumerator HandleQueuedPackets()
        //{
        //    var waitSeconds = new WaitForSeconds(1f);
        //    var waitEndOfFrame = new WaitForEndOfFrame();

        //    while (true)
        //    {
        //        var coopGC = CoopGameComponent.GetCoopGameComponent();
        //        if (coopGC == null)
        //            continue;

        //        if (player == null)
        //        {
        //            PatchConstants.Logger.LogInfo($"Player is NULL for Component {this}");
        //            yield return waitSeconds;
        //            continue;
        //        }

        //        //if (!coopGC.Players.ContainsKey(player.Profile.AccountId))
        //        //{
        //        //    coopGC.Players.TryAdd(player.Profile.AccountId, player);
        //        //    yield return waitSeconds;
        //        //    continue;
        //        //}

        //        if (!QueuedPackets.Any())
        //        {
        //            yield return waitSeconds;
        //            continue;
        //        }

        //        //PatchConstants.Logger.LogInfo($"{player.Profile.AccountId} has {QueuedPackets.Count} QueuedPackets");

        //        // Concurrent Queue may be breaking. CoopGameComponent is fighting with this thread and seems to win in most cases. 
        //        // Maybe move all logic to run directly from CoopGameComponent?
        //        if (QueuedPackets.TryDequeue(out Dictionary<string, object> packet))
        //        {
        //            HandlePacket(packet);
        //        }

        //        //yield return waitSeconds;
        //        yield return waitEndOfFrame;
        //    }
        //}

        public void HandlePacket(Dictionary<string, object> packet)
        {
            var method = packet["m"].ToString();

            foreach (var patch in ModuleReplicationPatch.Patches)
            {
                if (patch.MethodName == method)
                {
                    patch.Replicated(player, packet);
                    break;
                }
            }

            //switch (method)
            //{

            //    case "HostDied":
            //        PatchConstants.Logger.LogInfo("Host Died");
            //        //LocalGameEndingPatch.EndSession(LocalGamePatches.LocalGameInstance, LocalGamePatches.MyPlayerProfile.Id, EFT.ExitStatus.Survived, "", 0);
            //        break;
            //    case "Jump":
            //        PlayerOnJumpPatch.Replicated(player, packet);
            //        break;
            //    case "Move":
            //        LastMovementPacket = packet;
            //        break;
            //    case "Position":
            //        if (!IsMyPlayer)
            //        {
            //            Vector3 newPos = Vector3.zero;
            //            newPos.x = float.Parse(packet["x"].ToString());
            //            newPos.y = float.Parse(packet["y"].ToString());
            //            newPos.z = float.Parse(packet["z"].ToString());
            //            //ReceivedPacketPostion = newPos;
            //        }
            //        break;
            //    case "Rotation":
            //        if (!IsMyPlayer)
            //        {
            //            var rotationX = float.Parse(packet["rX"].ToString());
            //            var rotationY = float.Parse(packet["rY"].ToString());
            //            //ReceivedPacketRotation = new Vector2(rotationX, rotationY);
            //        }
            //        break;
            //    case "Tilt":
            //        PlayerOnTiltPatch.TiltReplicated(player, packet);
            //        break;


            //}
        }


        void Update()
        {
            if (player == null)
                return;

            _ = UpdateMovementSend();
            //    UpdateMovement();

            //    if (player.IsAI)
            //    {
            //        if (!LastPosition.HasValue)
            //            LastPosition = player.Position;

            //        if (Vector3.Distance(LastPosition.Value, player.Position) > 3)
            //        {
            //            LastPosition = player.Position;
            //        }
            //    }
        }

        //        public static Dictionary<string, object> PreMadeDataPacket;

        private Vector2 LastDirection { get; set; } = Vector2.zero;
        private Vector2 LastRotator { get; set; } = Vector2.zero;
        private Vector3 LastPosition { get; set; } = Vector3.zero;
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

        private bool IsUpdatingMovementSend = false;
        private async Task UpdateMovementSend()
        {
            if(IsUpdatingMovementSend) 
                return;

            if (player == null)
                return;

            if (player.MovementContext == null)
                return;

            IsUpdatingMovementSend = true;

            if (
                (LastDirection != player.InputDirection
                && Vector2.Dot(LastDirection, player.InputDirection) < 0.51)
                || Vector2.Dot(LastRotator, player.Rotation) < 0
                )
            {
                PreMadeMoveDataPacket["dX"] = Math.Round(player.InputDirection.x, 2).ToString();
                PreMadeMoveDataPacket["dY"] = Math.Round(player.InputDirection.y, 2).ToString();
                PreMadeMoveDataPacket["rX"] = Math.Round(player.Rotation.x, 2).ToString();
                PreMadeMoveDataPacket["rY"] = Math.Round(player.Rotation.y, 2).ToString();
                ServerCommunication.PostLocalPlayerData(player, PreMadeMoveDataPacket);
                LastDirection = player.InputDirection;
                LastRotator = player.Rotation;
            }

            if (this.LastTiltLevel != player.MovementContext.Tilt
                && (this.LastTiltLevel - player.MovementContext.Tilt > 0.05f || this.LastTiltLevel - player.MovementContext.Tilt < -0.05f)
                )
            {
                this.LastTiltLevel = player.MovementContext.Tilt;
                PreMadeTiltDataPacket["tilt"] = LastTiltLevel;
                ServerCommunication.PostLocalPlayerData(player, PreMadeTiltDataPacket);
            }

            IsUpdatingMovementSend = false;

        }
    }
}
