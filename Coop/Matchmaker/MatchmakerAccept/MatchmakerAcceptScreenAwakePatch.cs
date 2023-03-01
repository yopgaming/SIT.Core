using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Matchmaker
{
    public class MatchmakerAcceptScreenAwakePatch : ModulePatch
    {
        [Serializable]
        private class ServerStatus
        {
            [JsonProperty("ip")]
            public string ip { get; set; }

            [JsonProperty("status")]
            public string status { get; set; }
        }

        static BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        public static Type GetThisType()
        {
            return Tarkov.Core.PatchConstants.EftTypes
                 .Single(x => x == typeof(EFT.UI.Matchmaker.MatchMakerAcceptScreen));
            //.Single(x => x.FullName == "EFT.UI.Matchmaker.MatchMakerAcceptScreen");
        }

        protected override MethodBase GetTargetMethod()
        {

            var methodName = "Awake";

            return GetThisType().GetMethods(privateFlags).First(x => x.Name == methodName);

        }

        [PatchPrefix]
        private static bool PatchPrefix(
            EFT.UI.Matchmaker.MatchMakerAcceptScreen __instance
            )
        {
            Logger.LogInfo("MatchmakerAcceptScreenAwakePatch.PatchPrefix");
            MatchmakerAcceptPatches.MatchMakerAcceptScreenInstance = __instance;
            return true;
        }

        public static void DoCreateAndCheck()
        {
            if (MatchmakerAcceptPatches.Profile == null)
            {
                Logger.LogError("MatchmakerAcceptScreenAwakePatch::DoCreateAndCheck::MatchmakerAcceptPatches.Profile == null");
                return;
            }
            MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.AccountId);
            MatchmakerAcceptPatches.CheckForMatch();
        }

        public static void GoToRaid()
        {
            MatchmakerAcceptPatches.CheckForMatch();

            if (MatchmakerAcceptPatches.IsSinglePlayer)
            {
                Tarkov.Core.PatchConstants.DisplayMessageNotification("Starting Singleplayer Game...");
            }
            // SendInvitePatch sets up the Host
            else if (MatchmakerAcceptPatches.IsServer)
            {
                Tarkov.Core.PatchConstants.DisplayMessageNotification("Starting Coop Game as Host");
                MatchmakerAcceptPatches.SetGroupId(PatchConstants.GetPHPSESSID());
            }
            // MatchmakerAcceptPatches.CheckForMatch sets up the Client
            else if (MatchmakerAcceptPatches.IsClient)
            {
                Tarkov.Core.PatchConstants.DisplayMessageNotification("Starting Coop Game as Client");
            }

        }
    }
}









