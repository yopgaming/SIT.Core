#pragma warning disable CS0618 // Type or member is obsolete
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Tarkov.Core;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace SIT.Coop.Core.Player
{
    internal class PlayerReplicatedComponent : NetworkBehaviour
    {
        internal const int PacketTimeoutInSeconds = 1;
        internal ConcurrentQueue<Dictionary<string, object>> QueuedPackets { get; } = new();
        internal Dictionary<string, object> LastMovementPacket { get; set; }
        internal EFT.LocalPlayer player { private get; set; }
        public float LastTiltLevel { get; private set; }
        public bool IsMyPlayer { get; internal set; }

        void Awake()
        {
            PatchConstants.Logger.LogInfo("PlayerReplicatedComponent:Awake");
        }

        void Start()
        {
            PatchConstants.Logger.LogInfo($"PlayerReplicatedComponent:Start");

            StartCoroutine(HandleQueuedPackets());
        }

        private IEnumerator HandleQueuedPackets()
        {
            var waitSeconds = new WaitForSeconds(1f);
            var waitEndOfFrame = new WaitForEndOfFrame();

            while (true)
            {
                var coopGC = CoopGameComponent.GetCoopGameComponent();
                if (coopGC == null)
                    continue;

                if (player == null)
                {
                    PatchConstants.Logger.LogInfo($"Player is NULL for Component {this}");
                    yield return waitSeconds;
                    continue;
                }

                //if (!coopGC.Players.ContainsKey(player.Profile.AccountId))
                //{
                //    coopGC.Players.TryAdd(player.Profile.AccountId, player);
                //    yield return waitSeconds;
                //    continue;
                //}

                if (!QueuedPackets.Any())
                {
                    yield return waitSeconds;
                    continue;
                }

                //PatchConstants.Logger.LogInfo($"{player.Profile.AccountId} has {QueuedPackets.Count} QueuedPackets");

                // Concurrent Queue may be breaking. CoopGameComponent is fighting with this thread and seems to win in most cases. 
                // Maybe move all logic to run directly from CoopGameComponent?
                if (QueuedPackets.TryDequeue(out Dictionary<string, object> packet))
                {
                    HandlePacket(packet);
                }

                //yield return waitSeconds;
                yield return waitEndOfFrame;
            }
        }

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

            switch (method)
            {

                case "HostDied":
                    PatchConstants.Logger.LogInfo("Host Died");
                    //LocalGameEndingPatch.EndSession(LocalGamePatches.LocalGameInstance, LocalGamePatches.MyPlayerProfile.Id, EFT.ExitStatus.Survived, "", 0);
                    break;
                case "Jump":
                    PlayerOnJumpPatch.Replicated(player, packet);
                    break;
                case "Move":
                    LastMovementPacket = packet;
                    break;
                case "Position":
                    if (!IsMyPlayer)
                    {
                        Vector3 newPos = Vector3.zero;
                        newPos.x = float.Parse(packet["x"].ToString());
                        newPos.y = float.Parse(packet["y"].ToString());
                        newPos.z = float.Parse(packet["z"].ToString());
                        //ReceivedPacketPostion = newPos;
                    }
                    break;
                case "Rotation":
                    if (!IsMyPlayer)
                    {
                        var rotationX = float.Parse(packet["rX"].ToString());
                        var rotationY = float.Parse(packet["rY"].ToString());
                        //ReceivedPacketRotation = new Vector2(rotationX, rotationY);
                    }
                    break;
                case "Tilt":
                    PlayerOnTiltPatch.TiltReplicated(player, packet);
                    break;


            }
        }

        void Update()
        {
            if (player == null)
                return;

            UpdateMovement();

        }

        private IEnumerator UpdateMovement()
        {
            var waitSeconds = new WaitForSeconds(0.01f);
            var waitEndOfFrame = new WaitForEndOfFrame();

            while (true)
            {
                if (player == null)
                    continue;

                //yield return waitEndOfFrame;

                if (player.MovementContext != null)
                {
                    if (this.LastTiltLevel != player.MovementContext.Tilt
                        && (this.LastTiltLevel - player.MovementContext.Tilt > 0.05f || this.LastTiltLevel - player.MovementContext.Tilt < -0.05f)
                        )
                    {
                        this.LastTiltLevel = player.MovementContext.Tilt;
                        ServerCommunication.PostLocalPlayerData(player, new Dictionary<string, object>
                        {
                            { "tilt", LastTiltLevel },
                            { "m", "Tilt" }
                        });
                    }


                    if (LastMovementPacket == null)
                        continue;

                    PlayerOnMovePatch.MoveReplicated(player, LastMovementPacket);

                    yield return waitEndOfFrame;

                }
            }
        }
    }
}
