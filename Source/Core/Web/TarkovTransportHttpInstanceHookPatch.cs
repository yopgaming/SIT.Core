//using SIT.Core.Misc;
//using SIT.Tarkov.Core;
//using System.Reflection;

//namespace SIT.Core.Core.Web
//{
//    internal class TarkovTransportHttpInstanceHookPatch : ModulePatch
//    {
//        public static TarkovRequestTransportHttp TarkovRequestTransportHttpInstance { get; set; } = null;

//        protected override MethodBase GetTargetMethod()
//        {
//            var t = typeof(TarkovRequestTransportHttp);

//            return ReflectionHelpers.GetMethodForType(t, "method_3");
//        }

//        [PatchPostfix]
//        public static void Postfix(
//            TarkovRequestTransportHttp __instance
//            )
//        {

//            if (TarkovRequestTransportHttpInstance == null && __instance != null)
//            {
//                TarkovRequestTransportHttpInstance = __instance;
//                Logger.LogInfo("Found TarkovRequestTransportHttpInstance");
//            }
//        }
//    }
//}
