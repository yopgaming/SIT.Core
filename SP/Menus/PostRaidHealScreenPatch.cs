﻿//using EFT;
//using SIT.Tarkov.Core;
//using System.Linq;
//using System.Reflection;

//namespace SIT.Core.SP.Menus
//{
//    /// <summary>
//    /// We need to alter Class1049.smethod_0().
//    /// Set the passed in ERaidMode to online, this ensures the heal screen shows.
//    /// It cannot be changed in the calling method as doing so causes the post-raid exp display to remain at 0
//    /// </summary>
//    public class PostRaidHealScreenPatch : ModulePatch
//    {
//        protected override MethodBase GetTargetMethod()
//        {
//            var desiredType = PatchConstants.EftTypes.Single(x => x.Name == "PostRaidHealthScreenClass");
//            var desiredMethod = desiredType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(IsTargetMethod);

//            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
//            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

//            return desiredMethod;
//        }

//        private static bool IsTargetMethod(MethodInfo mi)
//        {
//            var parameters = mi.GetParameters();
//            return parameters.Length == 7
//                && parameters[0].Name == "session"
//                && parameters[1].Name == "profileId"
//                && parameters[2].Name == "savageProfile"
//                && parameters[3].Name == "location"
//                && parameters[4].Name == "exitStatus"
//                && parameters[5].Name == "exitTime"
//                && parameters[6].Name == "raidMode";
//        }

//        [PatchPrefix]
//        private static bool PatchPrefix(TarkovApplication __instance, ref ERaidMode raidMode)
//        {
//            raidMode = ERaidMode.Online;

//            return true; // Perform original method
//        }
//    }
//}
