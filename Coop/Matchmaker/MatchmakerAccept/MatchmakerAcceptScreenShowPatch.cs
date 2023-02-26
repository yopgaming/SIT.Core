using EFT;
//using ScreenController = EFT.UI.Matchmaker.MatchMakerAcceptScreen.GClass2426;
//using Grouping = GClass2434;
using EFT.UI;
using EFT.UI.Matchmaker;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace SIT.Coop.Core.Matchmaker
{
    public class MatchmakerAcceptScreenShowPatch : ModulePatch
    {


        static BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        public static Type GetThisType()
        {
            return Tarkov.Core.PatchConstants.EftTypes
                 .Single(x => x == typeof(EFT.UI.Matchmaker.MatchMakerAcceptScreen));
        }

        protected override MethodBase GetTargetMethod()
        {

            var methodName = "Show";

            return GetThisType().GetMethods(privateFlags)
                .First(x => x.Name == methodName && x.GetParameters()[0].Name == "session");

        }


        //private static Button _updateListButton;

        [PatchPrefix]
        private static void Pre(
            ref ISession session,
            ref RaidSettings raidSettings,
            Profile ___profile_0,
            MatchMakerAcceptScreen __instance,
            DefaultUIButton ____acceptButton
            )
        {
            ____acceptButton.OnClick.AddListener(() =>
            {
                Logger.LogInfo("MatchmakerAcceptScreenShow.PatchPrefix:Clicked");
                MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.AccountId);
            });
        }


        [PatchPostfix]
        private static void Post(
            ref ISession session,
            ref RaidSettings raidSettings,
            Profile ___profile_0,
            MatchMakerAcceptScreen __instance,
            DefaultUIButton ____acceptButton
            )
        {
			Logger.LogInfo("MatchmakerAcceptScreenShow.PatchPostfix");

            // ------------------------------------------
            // Keep an instance for other patches to work
            MatchmakerAcceptPatches.MatchMakerAcceptScreenInstance = __instance;
            // ------------------------------------------
            MatchmakerAcceptPatches.Profile = ___profile_0;
            Logger.LogInfo("MatchmakerAcceptScreenShow.PatchPostfix:" + ___profile_0.AccountId);

            if (MatchmakerAcceptPatches.CheckForMatch())
            {
                ____acceptButton.SetHeaderText("Join Match");
            }
            else
            {
                ____acceptButton.SetHeaderText("Start Match");
            }

        }
    }


}
