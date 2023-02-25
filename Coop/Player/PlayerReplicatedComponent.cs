#pragma warning disable CS0618 // Type or member is obsolete
using EFT.Interactive;
using SIT.Coop.Core.LocalGame;
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Core.Coop.Player.FirearmControllerPatches;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

namespace SIT.Coop.Core.Player
{
    internal class PlayerReplicatedComponent : NetworkBehaviour
    {
        internal const int PacketTimeoutInSeconds = 1;
        internal ConcurrentQueue<Dictionary<string, object>> QueuedPackets { get; }
            = new ConcurrentQueue<Dictionary<string, object>>();

        internal Dictionary<string, object> LastMovementPacket { get; set; }
        internal Dictionary<string, object> LastRotationPacket { get; set; }
        internal DateTime? LastRotationPacketPostTime { get; set; }
        internal EFT.LocalPlayer player { private get; set; }

        internal List<Vector2> ClientListRotationsToSend { get; } = new List<Vector2>();
        internal ConcurrentQueue<Vector2> ReceivedRotationPackets { get; } = new ConcurrentQueue<Vector2>();
        public float LastTiltLevel { get; private set; }
        public Quaternion? LastRotation { get; private set; }
        public Vector2 LastMovementDirection { get; private set; } = Vector2.zero;
        public bool IsMyPlayer { get; internal set; }

        //private NetworkConnection _connection { get; } = new NetworkConnection();
        //private System.Random RandomConnectionIds { get; } = new System.Random();

        //private NetworkClient _client { get; set; }

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


        //private Vector3 ReceivedPacketPostion = Vector3.zero;
        //private Vector2 ReceivedPacketRotation = Vector2.zero;


        //void FixedUpdate()
        //{

        //}

        ////bool handlingPackets = false;

        void Update()
        {
            if (player == null)
                return;

            UpdateMovement();

        }


        public static bool ShouldReplicate(EFT.Player player, bool isMyPlayer)
        {
            return (Matchmaker.MatchmakerAcceptPatches.IsClient && isMyPlayer)
               || (Matchmaker.MatchmakerAcceptPatches.IsServer && player.IsAI)
               || (Matchmaker.MatchmakerAcceptPatches.IsServer && isMyPlayer);
        }

        //private Vector3? LastSentPosition;

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
                        Dictionary<string, object> dictionary = new Dictionary<string, object>();
                        dictionary.Add("tilt", LastTiltLevel);
                        dictionary.Add("m", "Tilt");
                        ServerCommunication.PostLocalPlayerData(player, dictionary);
                    }

                }

                //if (!IsMyPlayer && ReceivedPacketRotation != Vector2.zero)
                //{
                //    player.MovementContext.Rotation = Vector2.Lerp(player.MovementContext.Rotation, ReceivedPacketRotation, 2f * Time.deltaTime);
                //}

                if (LastMovementPacket == null)
                    continue;

                PlayerOnMovePatch.MoveReplicated(player, LastMovementPacket);

                yield return waitEndOfFrame;

            }
        }
    }
}
