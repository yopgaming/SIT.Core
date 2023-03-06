using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SIT.Coop.Core.Player
{
    internal class PlayerOnMovePatch// : ModulePatch
    {
        ////        public static Dictionary<string, object> PreMadeDataPacket;

        ////        public PlayerOnMovePatch()
        ////        {
        ////            PreMadeDataPacket = new()
        ////            {
        ////                { "dX", "0" },
        ////                { "dY", "0" },
        ////                { "rX", "0" },
        ////                { "rY", "0" },
        ////                { "m", "Move" }
        ////            };
        ////        }

        ////        protected override MethodBase GetTargetMethod()
        ////        {
        ////            var t = SIT.Tarkov.Core.PatchConstants.EftTypes.FirstOrDefault(x => x.FullName == "EFT.Player");
        ////            if (t == null)
        ////                Logger.LogInfo($"PlayerOnMovePatch:Type is NULL");

        ////            var method = PatchConstants.GetAllMethodsForType(t)
        ////                .FirstOrDefault(x =>
        ////                x.GetParameters().Length == 1
        ////                && x.GetParameters()[0].Name.Contains("direction")
        ////                && x.Name == "Move"
        ////                );

        ////            Logger.LogInfo($"PlayerOnMovePatch:{t.Name}:{method.Name}");
        ////            return method;
        ////        }

        ////        public static Dictionary<string, long> LastPacketReceived { get; } = new Dictionary<string, long>();
        ////        public static Dictionary<string, Vector2?> LastDirection { get; } = new Dictionary<string, Vector2?>();

        ////        //public static bool IsMyPlayer(EFT.Player player) { return player == (LocalGamePatches.MyPlayer as EFT.Player); }

        ////        [PatchPrefix]
        ////        public static bool PrePatch(
        ////            EFT.Player __instance,
        ////            Vector2 direction)
        ////        {
        ////            if (__instance == null)
        ////                return false;

        ////            // This wont work for AI. There are FAR too many calls to this from the AI.
        ////            if (__instance.IsAI)
        ////                return true;

        ////            var accountId = __instance.Profile.AccountId;
        ////            var nickname = __instance.Profile.Nickname;

        ////            direction.Normalize();
        ////            direction.x = (float)Math.Round(direction.x, 2);
        ////            direction.y = (float)Math.Round(direction.y, 2);

        ////            if (!__instance.TryGetComponent<PlayerReplicatedComponent>(out var prc))
        ////            {
        ////                Logger.LogError($"Player {__instance.Id} is trying to move without a PlayerReplicatedComponent");
        ////                return true;
        ////            }

        ////            var coopGC = CoopGameComponent.GetCoopGameComponent();
        ////            if (coopGC == null) 
        ////            {
        ////                Logger.LogError($"Player {__instance.Id} is trying to move without a CoopGameComponent");
        ////                return true;
        ////            }

        ////            if (!coopGC.Players.ContainsKey(accountId))
        ////            {
        ////                coopGC.Players.TryAdd(accountId, (EFT.LocalPlayer)__instance);
        ////            }

        ////            if (!LastDirection.ContainsKey(accountId))
        ////                LastDirection.Add(accountId, null);

        ////            if (LastDirection[accountId] != direction)
        ////            {
        ////                LastDirection[accountId] = direction;

        ////                Dictionary<string, object> dictionary = PreMadeDataPacket.ToJson().ParseJsonTo<Dictionary<string, object>>();
        ////                dictionary["dX"] = Math.Round(direction.x, 2).ToString();
        ////                dictionary["dY"] = Math.Round(direction.y, 2).ToString();
        ////                dictionary["rX"] = Math.Round(__instance.Rotation.x, 2).ToString();
        ////                dictionary["rY"] = Math.Round(__instance.Rotation.y, 2).ToString();

        ////                ServerCommunication.PostLocalPlayerData(__instance, dictionary, out _, out _);
        ////            }

        ////            return true;
        ////            //return false;
        ////        }

        public static Dictionary<string, float> LastPacketReceived { get; } = new Dictionary<string, float>();

        public static void MoveReplicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var accountId = player.Profile.AccountId;

            var thisPacket = dict;

            Vector2 direction = new Vector2(float.Parse(dict["dX"].ToString()), float.Parse(dict["dY"].ToString()));

            Vector2? newRot = null;// = Vector2.zero;
            if (dict.ContainsKey("rX"))
            {
                float rx = float.Parse(dict["rX"].ToString());
                float ry = float.Parse(dict["rY"].ToString());
                newRot = new Vector2(rx, ry);
            }

            var packetTime = long.Parse(dict["t"].ToString());
            var packetDateTime = new DateTime(packetTime);
            if (packetDateTime < DateTime.Now.AddSeconds(-3))
                return;

            if (LastPacketReceived.ContainsKey(accountId) && LastPacketReceived[accountId] == packetTime)
            {
                //PatchConstants.Logger.LogDebug($"MoveReplicated:[{accountId}]:Ignored. Packet {packetDateTime} already processed.");
                return;
            }

            if (!LastPacketReceived.ContainsKey(accountId))
                LastPacketReceived.Add(accountId, packetTime);

            LastPacketReceived[accountId] = packetTime;

            player.CurrentState.Move(direction);
            player.InputDirection = direction;

            if (newRot.HasValue)
            {
                player.CurrentState.Rotate(newRot.Value);
            }

            if (dict.ContainsKey("pX"))
            {
                float px = float.Parse(dict["pX"].ToString());
                float py = float.Parse(dict["pY"].ToString());
                float pz = float.Parse(dict["pZ"].ToString());
                var newP = new Vector3(px, py, pz);
                if (Vector3.Distance(player.Position, newP) > 1)
                    player.Teleport(newP);
            }

            PatchConstants.Logger.LogDebug(accountId + ": move replicated");



        }




    }


}

//using EFT.UI;
//using HarmonyLib;
//using SIT.Tarkov.Core;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Reflection;
//using System.Reflection.Emit;

//namespace SIT.Coop.Core.Player
//{
//    internal class PlayerOnMovePatch : ModulePatch
//    {
//        public static Dictionary<string, object> PreMadeDataPacket;

//        public PlayerOnMovePatch()
//        {
//            PreMadeDataPacket = new()
//            {
//                { "dX", "0" },
//                { "dY", "0" },
//                { "rX", "0" },
//                { "rY", "0" },
//                { "m", "Move" }
//            };
//        }

//        protected override MethodBase GetTargetMethod()
//        {
//            var t = typeof(EFT.Player);
//            if (t == null)
//                Logger.LogInfo($"PlayerOnMovePatch:Type is NULL");

//            var method = PatchConstants.GetAllMethodsForType(t)
//                .FirstOrDefault(x =>
//                x.GetParameters().Length == 1
//                && x.GetParameters()[0].Name.Contains("direction")
//                && x.Name == "Move"
//                );

//            //Logger.LogInfo($"PlayerOnMovePatch:{t.Name}:{method.Name}");
//            return method;
//        }

//        //[PatchPrefix]
//        //public static bool MovePre(ref Vector2 direction)
//        //{
//        //    return false;
//        //}

//        [PatchPostfix]
//        public static void MovePost(EFT.Player __instance, ref Vector2 direction)
//        {
//            __instance.dir
//        }
//        //static void SendMoveToServer(Vector2 direction)
//        //{

//        //}

//        //static MethodInfo m_MyExtraMethod = SymbolExtensions.GetMethodInfo((Vector2 direction) => SendMoveToServer(direction));

//        //[PatchTranspiler]
//        //static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//        //{
//        //    var cInstruction = new CodeInstruction(OpCodes.Call, m_MyExtraMethod);
//        //    instructions.AddItem(cInstruction);
//        //    // do something
//        //    foreach (var i in instructions)
//        //    {
//        //        yield return i;
//        //    }
//        //}

//    }
//}