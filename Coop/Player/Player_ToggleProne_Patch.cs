//using SIT.Coop.Core.Web;
//using SIT.Core.Misc;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Generic;
//using System.Reflection;

//namespace SIT.Core.Coop.Player
//{
//    public class Player_ToggleProne_Patch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(EFT.Player);
//        public override string MethodName => "ToggleProne";

//        protected override MethodBase GetTargetMethod()
//        {
//            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);

//            return method;
//        }

//        public static Dictionary<string, bool> CallLocally
//            = new Dictionary<string, bool>();

//        [PatchPrefix]
//        public static bool PrePatch(EFT.Player __instance)
//        {
//            var result = false;
//            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
//                result = true;

//            return result;
//        }

//        [PatchPostfix]
//        public static void PostPatch(
//           EFT.Player __instance
//            )
//        {
//            var player = __instance;

//            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
//            {
//                CallLocally.Remove(player.Profile.AccountId);
//                return;
//            }

//            Dictionary<string, object> dictionary = new Dictionary<string, object>();
//            dictionary.Add("t", DateTime.Now.Ticks);
//            dictionary.Add("m", "ToggleProne");
//            ServerCommunication.PostLocalPlayerData(player, dictionary);

//        }


//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            if (HasProcessed(GetType(), player, dict))
//                return;

//            try
//            {
//                CallLocally.Add(player.Profile.AccountId, true);
//                player.ToggleProne();
//            }
//            catch (Exception e)
//            {
//                Logger.LogError(e);
//            }
//        }
//    }
//}

