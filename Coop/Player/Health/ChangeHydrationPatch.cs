//using SIT.Core.Misc;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace SIT.Core.Coop.Player.Health
//{
//    internal class ChangeHydrationPatch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(PlayerHealthController);

//        public override string MethodName => "ChangeHydration";

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(typeof(PlayerHealthController), "ChangeHydration", findFirst: true);
//        }

//        [PatchPostfix]
//        public static void PatchPostfix(
//            PlayerHealthController __instance
//            , float value
//            )
//        {
//            var player = __instance.Player;


//        }

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//        }
//    }
//}
