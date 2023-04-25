using EFT;
//using ScreenController = EFT.UI.Matchmaker.MatchMakerAcceptScreen.GClass2426;
//using Grouping = GClass2434;
using EFT.UI;
using EFT.UI.Matchmaker;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        private static DateTime LastClickedTime { get; set; } = DateTime.MinValue;

        [PatchPrefix]
        private static void Pre(
            ref ISession session,
            ref RaidSettings raidSettings,
            Profile ___profile_0,
            MatchMakerAcceptScreen __instance,
            DefaultUIButton ____acceptButton
            )
        {
            Logger.LogInfo("MatchmakerAcceptScreenShow.PatchPrefix");

            var rs = raidSettings;
            ____acceptButton.OnClick.AddListener(() =>
            {
                if (LastClickedTime < DateTime.Now.AddSeconds(-10))
                {
                    LastClickedTime = DateTime.Now;

                    Logger.LogInfo("MatchmakerAcceptScreenShow.PatchPrefix:Clicked");
                    if (MatchmakerAcceptPatches.CheckForMatch(rs, out string returnedJson))
                    {
                        Logger.LogDebug(returnedJson);
                        JObject result = JObject.Parse(returnedJson);
                        var groupId = result["ServerId"].ToString();
                        Matchmaker.MatchmakerAcceptPatches.SetGroupId(groupId);
                        MatchmakerAcceptPatches.MatchingType = EMatchmakerType.GroupPlayer;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }
                    else
                    {
                        MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.AccountId, rs);
                    }
                }
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
            //Logger.LogInfo("MatchmakerAcceptScreenShow.PatchPostfix");

            // ------------------------------------------
            // Keep an instance for other patches to work
            MatchmakerAcceptPatches.MatchMakerAcceptScreenInstance = __instance;
            // ------------------------------------------
            MatchmakerAcceptPatches.Profile = ___profile_0;
            //Logger.LogInfo("MatchmakerAcceptScreenShow.PatchPostfix:" + ___profile_0.AccountId);

            if (MatchmakerAcceptPatches.CheckForMatch(raidSettings, out string returnedJson))
            {
                ____acceptButton.SetHeaderText("Join Match");
                raidSettings.BotSettings.BotAmount = EFT.Bots.EBotAmount.NoBots;
            }
            else
            {
                ____acceptButton.SetHeaderText("Start Match");
            }

        }
    }


}
