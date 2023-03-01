//using SIT.Coop.Core.Web;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;

//namespace SIT.Core.Coop.Player.Movement
//{
//    public class Player_MovementContext_ApplyMotion_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(MovementContext);
//        public override string MethodName => "ApplyMotion";

//        protected override MethodBase GetTargetMethod()
//        {
//            var method = PatchConstants.GetMethodForType(InstanceType, MethodName);

//            return method;
//        }

//        public static Dictionary<string, bool> CallLocally
//            = new Dictionary<string, bool>();

//        private static List<long> ProcessedCalls
//            = new List<long>();

//        //[PatchPrefix]
//        //public static bool PrePatch(
//        //    MovementContext __instance
//        //    , Vector3 motion, float deltaTime)
//        //{
//        //    //var result = false;
//        //    //if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
//        //    //    result = true;

//        //    //return result;
//        //    return true;
//        //}

//        [PatchPostfix]
//        public static void PostPatch(
//           ref MovementContext __instance,
//           ref Vector3 motion, 
//           ref float deltaTime
//            )
//        {
//            var player = PatchConstants.GetFieldOrPropertyFromInstance<EFT.Player>(__instance, "player_0", false);

//            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
//            {
//                CallLocally.Remove(player.Profile.AccountId);
//                return;
//            }

//            Dictionary<string, object> dictionary = new Dictionary<string, object>();
//            dictionary.Add("t", DateTime.Now.Ticks);
//            dictionary.Add("x", motion.x);
//            dictionary.Add("y", DateTime.Now.Ticks);
//            dictionary.Add("z", DateTime.Now.Ticks);
//            dictionary.Add("m", "ApplyMotion");
//            //ServerCommunication.PostLocalPlayerData(player, dictionary);

//        }


//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            var timestamp = long.Parse(dict["t"].ToString());

//            if (!ProcessedCalls.Contains(timestamp))
//                ProcessedCalls.Add(timestamp);
//            else
//            {
//                ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddHours(-1).Ticks);
//                return;
//            }

//            var directionX = float.Parse(dict["x"].ToString());
//            var directionY = float.Parse(dict["y"].ToString());
//            var directionZ = float.Parse(dict["z"].ToString());
//            var delta = float.Parse(dict["d"].ToString());
//            try
//            {
//                CallLocally.Add(player.Profile.AccountId, true);
//                player.MovementContext.ApplyMotion(new UnityEngine.Vector3(directionX, directionY, directionZ), delta);
//            }
//            catch (Exception e)
//            {
//                Logger.LogInfo(e);
//            }
//        }
//    }
//}

