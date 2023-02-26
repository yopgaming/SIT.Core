//using SIT.Coop.Core.Web;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace SIT.Core.Coop.Player.FirearmControllerPatches
//{
//    internal class ItemHandsControllerPickupPatch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player.ItemHandsController);
//        public override string MethodName => "Pickup";

//        protected override MethodBase GetTargetMethod()
//        {
//            var method = PatchConstants.GetMethodForType(InstanceType, MethodName);
//            return method;
//        }

//        public static Dictionary<string, bool> CallLocally
//            = new Dictionary<string, bool>();


//        [PatchPrefix]
//        public static bool PrePatch(EFT.Player.ItemHandsController __instance)
//        {
//            //var player = ____player;
//            var player = PatchConstants.GetFieldFromType(__instance.GetType(), "_player").GetValue(__instance) as EFT.Player;
//            if (player == null)
//                return false;

//            var result = false;
//            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
//                result = true;

//            return result;
//        }

//        [PatchPostfix]
//        public static void PostPatch(EFT.Player.ItemHandsController __instance, bool p)
//        {
//            var player = PatchConstants.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
//            if (player == null)
//                return;

//            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
//            {
//                CallLocally.Remove(player.Profile.AccountId);
//                return;
//            }

//            Dictionary<string, object> dictionary = new Dictionary<string, object>();
//            dictionary.Add("t", DateTime.Now.Ticks);
//            dictionary.Add("pick", p.ToString());
//            dictionary.Add("m", "Pickup");
//            ServerCommunication.PostLocalPlayerData(player, dictionary);
//        }

//        private static ConcurrentBag<long> ProcessedCalls = new ConcurrentBag<long>();

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            var timestamp = long.Parse(dict["t"].ToString());
//            if (!ProcessedCalls.Contains(timestamp))
//                ProcessedCalls.Add(timestamp);
//            else
//                return;

//            if (player.HandsController is EFT.Player.ItemHandsController itemhandscont)
//            {
//                CallLocally.Add(player.Profile.AccountId, true);
//                itemhandscont.Pickup(true);
//            }
//        }
//    }
//}
