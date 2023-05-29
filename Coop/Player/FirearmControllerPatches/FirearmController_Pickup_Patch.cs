//using SIT.Coop.Core.Web;
//using SIT.Core.Misc;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace SIT.Core.Coop.Player.FirearmControllerPatches
//{
//    internal class FirearmController_Pickup_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player.FirearmController);
//        public override string MethodName => "FCPickup";

//        protected override MethodBase GetTargetMethod()
//        {
//            var method = ReflectionHelpers.GetMethodForType(InstanceType, "Pickup", true);
//            return method;
//        }

//        public static Dictionary<string, bool> CallLocally
//            = new Dictionary<string, bool>();


//        [PatchPrefix]
//        public static bool PrePatch(
//            EFT.Player.FirearmController __instance
//            , EFT.Player ____player)
//        {
//            var player = ____player;
//            if (player == null)
//                return false;

//            var result = false;
//            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
//                result = true;

//            return result;
//        }

//        [PatchPostfix]
//        public static void PostPatch(EFT.Player.FirearmController __instance, bool p)
//        {
//            var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
//            if (player == null)
//                return;

//            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
//            {
//                CallLocally.Remove(player.Profile.AccountId);
//                return;
//            }

//            Dictionary<string, object> dictionary = new Dictionary<string, object>();
//            dictionary.Add("t", DateTime.Now.Ticks);
//            dictionary.Add("p", p.ToString());
//            dictionary.Add("m", "FCPickup");
//            ServerCommunication.PostLocalPlayerData(player, dictionary);
//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            var timestamp = long.Parse(dict["t"].ToString());
//            if (HasProcessed(GetType(), player, dict))
//                return;

//            if (player.HandsController is EFT.Player.FirearmController firearmCont)
//            {
//                CallLocally.Add(player.Profile.AccountId, true);
//                firearmCont.Pickup(bool.Parse(dict["p"].ToString()));
//            }
//        }
//    }
//}
