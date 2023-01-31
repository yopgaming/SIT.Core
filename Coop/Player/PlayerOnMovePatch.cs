using SIT.Coop.Core.Matchmaker;
using SIT.Tarkov.Core;
using SIT.Coop.Core.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using EFT;
using SIT.Coop.Core.LocalGame;

namespace SIT.Coop.Core.Player
{
    internal class PlayerOnMovePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = SIT.Tarkov.Core.PatchConstants.EftTypes.FirstOrDefault(x => x.FullName == "EFT.Player");
            if (t == null)
                Logger.LogInfo($"PlayerOnMovePatch:Type is NULL");

            var method = PatchConstants.GetAllMethodsForType(t)
                .FirstOrDefault(x =>
                x.GetParameters().Length == 1
                && x.GetParameters()[0].Name.Contains("direction")
                && x.Name == "Move"
                );

            Logger.LogInfo($"PlayerOnMovePatch:{t.Name}:{method.Name}");
            return method;
        }

        public static Dictionary<string, ulong> Sequence { get; } = new Dictionary<string, ulong>();
        public static Dictionary<string, DateTime> LastPacketSent { get; } = new Dictionary<string, DateTime>();
        public static Dictionary<string, long> LastPacketReceived { get; } = new Dictionary<string, long>();
        public static Dictionary<string, Vector2?> LastDirection { get; } = new Dictionary<string, Vector2?>();

        public static Dictionary<string, bool> ClientIsMoving { get; } = new Dictionary<string, bool>();
        
        public static System.Random RandomizerForAI = new System.Random();

        public static bool IsMyPlayer(EFT.Player player) { return player == (LocalGamePatches.MyPlayer as EFT.Player); }

        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player __instance,
            Vector2 direction)
        {
            //direction.Normalize();
            //direction.x = (float)Math.Round(direction.x, 2);
            //direction.y = (float)Math.Round(direction.x, 2);
        //    return false;// Matchmaker.MatchmakerAcceptPatches.IsSinglePlayer;
        //}


        //[PatchPostfix]
        //public static void PatchPostfix(
        //    EFT.Player __instance,
        //    Vector2 direction)
        //{
            var accountId = __instance.Profile.AccountId;
            var nickname = __instance.Profile.Nickname;

            direction.Normalize();
            direction.x = (float)Math.Round(direction.x, 2);
            direction.y = (float)Math.Round(direction.y, 2);

            var prc = __instance.GetComponent<PlayerReplicatedComponent>();
            if (prc == null)
            {
                Logger.LogInfo("PRC is NULL on " + nickname);
                return true;
            }

            if (!LastDirection.ContainsKey(accountId))
                LastDirection.Add(accountId, null);

            if (LastDirection[accountId] != direction)
            {
                LastDirection[accountId] = direction;

                //    var timeToWait = __instance.IsAI ? -(450 + RandomizerForAI.Next(-100, 100))  : -5;

                //    if (!LastPacketSent.ContainsKey(accountId) || LastPacketSent[accountId] < DateTime.Now.AddMilliseconds(timeToWait))
                //    {
                //        if (!Sequence.ContainsKey(accountId))
                //            Sequence.Add(accountId, 0);

                //        Sequence[accountId]++;

                //        //Logger.LogInfo(__instance.Profile.AccountId + " " + direction);
                //        __instance.CurrentState.Move(direction);
                //        __instance.InputDirection = direction;

                //        if (!LastPacketSent.ContainsKey(accountId))
                //            LastPacketSent.Add(accountId, DateTime.Now);

                //        LastPacketSent[accountId] = DateTime.Now;

                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                //dictionary.Add("seq", Sequence[accountId]);
                dictionary.Add("dX", Math.Round(direction.x, 2).ToString());
                dictionary.Add("dY", Math.Round(direction.y, 2).ToString());
                //dictionary.Add("pX", __instance.Position.x);
                //dictionary.Add("pY", __instance.Position.y);
                //dictionary.Add("pZ", __instance.Position.z);
                dictionary.Add("rX", Math.Round(__instance.Rotation.x, 2).ToString());
                dictionary.Add("rY", Math.Round(__instance.Rotation.y, 2).ToString());
                dictionary.Add("m", "Move");

                //        //Logger.LogInfo("Sending Move for " + nickname);
                ServerCommunication.PostLocalPlayerData(__instance, dictionary, out string returnedData, out var genData);
                //        // setup prediction of outcome
                //        prc.DequeueAllMovementPackets();
                //        prc.LastMovementPacket = dictionary;
                //    }
            }

            return false;
        }

        public static void MoveReplicated(EFT.Player player, Dictionary<string, object> dict)
        {


            var accountId = player.Profile.AccountId;

            var thisPacket = dict;

            Vector2 direction = new Vector2(float.Parse(dict["dX"].ToString()), float.Parse(dict["dY"].ToString()));

            //Logger.LogInfo($"{accountId} MoveReplicated!");
            //var newPos = Vector3.zero;
            //newPos.x = float.Parse(dict["pX"].ToString());
            //newPos.y = float.Parse(dict["pY"].ToString());
            //newPos.z = float.Parse(dict["pZ"].ToString());
            Vector2? newRot = null;// = Vector2.zero;
            if (dict.ContainsKey("rX"))
            {
                float rx = float.Parse(dict["rX"].ToString());
                float ry = float.Parse(dict["rY"].ToString());
                newRot = new Vector2(rx, ry);
                //newRot.x = float.Parse(dict["rX"].ToString());
                //newRot.y = float.Parse(dict["rY"].ToString());
            }

            var packetTime = long.Parse(dict["t"].ToString());

            // Is first packet OR is after the last packet received. This copes with unordered received packets
            //if ((!LastPacketReceived.ContainsKey(accountId) || LastPacketReceived[accountId] <= packetTime))
            //{
                player.CurrentState.Move(direction);
                player.InputDirection = direction;

                if (newRot.HasValue)
                {
                    //player.CurrentState.Rotate(newRot.Value);
                }
                //Logger.LogInfo(accountId + ": move replicated");

                if (!LastPacketReceived.ContainsKey(accountId))
                    LastPacketReceived.Add(accountId, packetTime);

                LastPacketReceived[accountId] = packetTime;

            //}




        }


    }
}
