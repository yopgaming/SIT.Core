//using SIT.Core.Misc;
//using SIT.Tarkov.Core;
//using System.Reflection;

//namespace SIT.Core.Core.Web
//{
//    internal class TarkovTransportWSInstanceHookPatch : ModulePatch
//    {
//        public static TarkovRequestTransportWS TarkovRequestTransportWSInstance { get; set; } = null;

//        protected override MethodBase GetTargetMethod()
//        {
//            var t = typeof(TarkovRequestTransportWS);

//            return ReflectionHelpers.GetMethodForType(t, "EstablishConnectionToUrl");
//        }

//        [PatchPostfix]
//        public static void Postfix(
//            TarkovRequestTransportWS __instance
//            )
//        {
//            if (TarkovRequestTransportWSInstance == null && __instance != null)
//            {
//                TarkovRequestTransportWSInstance = __instance;
//                Logger.LogInfo("Found TarkovRequestTransportWSInstance");
//            }
//        }
//    }
//}
