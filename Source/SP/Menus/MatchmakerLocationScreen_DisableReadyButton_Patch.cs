using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Core.SP.Menus
{
    /// <summary>
    /// This Patch Disables the Ready button after you select the location so you will not be jumping to online match by default
    /// Game needs to initialize the offline match variables first unfortunatly
    /// </summary>
    internal class MatchmakerLocationScreen_DisableReadyButton_Patch : ModulePatch
    {
        public MatchmakerLocationScreen_DisableReadyButton_Patch()
        {
        }

        [PatchPrefix]
        public static void Prefix(
           ref EFT.UI.Matchmaker.MatchMakerSelectionLocationScreen __instance
          )
        {

        }

        [PatchPostfix]
        public static void PatchPostfix(
            ref EFT.UI.Matchmaker.MatchMakerSelectionLocationScreen __instance
            , ref EFT.UI.DefaultUIButton ____readyButton
            , ref EFT.UI.DefaultUIButton ____acceptButton
            , ref RaidSettings ___raidSettings_0)
        {


            ____readyButton.gameObject.SetActive(false);
            ____acceptButton.gameObject.SetActive(true);

            //Logger.LogInfo("DEBUG::" + ___raidSettings_0.ToJson());
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(EFT.UI.Matchmaker.MatchMakerSelectionLocationScreen), "Show", false, true);
        }
    }
}
