using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Core.SP.Menus
{

    /// <summary>
    /// Extremely useful tool for debugging behavior of location files. Fixed the problem with 0.13.0.4.23122
    /// </summary>
    internal class MatchmakerLocationScreen_LocationSelectDebug_Patch : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(EFT.UI.Matchmaker.MatchMakerSelectionLocationScreen), "method_5", false, true);

        }

        [PatchPrefix]
        public static bool Prefix(ref LocationSettings.Location location)
        {
            Logger.LogInfo("DEBUG:" + location.SITToJson());
            if (location != null)
            {
                location.RequiredPlayerLevelMin = 1;
                location.RequiredPlayerLevelMax = 99;
                location.MinPlayerLvlAccessKeys = 1;
                location.AveragePlayerLevel = 1;
            }

            return true;
        }

    }
}
