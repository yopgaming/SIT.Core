//using SIT.Core.Misc;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace SIT.Core.SP.Menus
//{
//    public class MatchmakerLocationScreen_DisableLevelLock_Patch : ModulePatch
//    {

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(typeof(EFT.UI.Matchmaker.MatchMakerSelectionLocationScreen), "method_4", false, true);

//        }

//        [PatchPrefix]
//        public static bool Prefix(ref bool __result)
//        {
//            __result = true;
//            return false;
//        }

//    }
//}
