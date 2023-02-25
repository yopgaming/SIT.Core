using EFT;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;

namespace SIT.Coop.Core.Matchmaker
{
    public enum EMatchmakerType
    {
        Single = 0,
        GroupPlayer = 1,
        GroupLeader = 2
    }

    [Serializable]
    public class ServerStatus
    {
        [JsonProperty("ip")]
        public string ip { get; set; }

        [JsonProperty("status")]
        public string status { get; set; }
    }

    public static class MatchmakerAcceptPatches
    {
        #region Fields/Properties
        public static EFT.UI.Matchmaker.MatchMakerAcceptScreen MatchMakerAcceptScreenInstance { get; set; }
        public static Profile Profile { get; set; }
        public static EMatchmakerType MatchingType { get; set; } = EMatchmakerType.Single;
        public static bool IsServer => MatchingType == EMatchmakerType.GroupLeader;
        public static bool IsClient => MatchingType == EMatchmakerType.GroupPlayer;
        public static bool IsSinglePlayer => MatchingType == EMatchmakerType.Single;
        public static int HostExpectedNumberOfPlayers { get; set; }
        private static string groupId;
        #endregion
        public static object MatchmakerScreenController
        {
            get
            {
                var screenController = PatchConstants.GetFieldOrPropertyFromInstance<object>(MatchMakerAcceptScreenInstance, "ScreenController", false);
                if (screenController != null)
                {
                    PatchConstants.Logger.LogInfo("MatchmakerAcceptPatches.Found ScreenController Instance");

                    return screenController;

                }
                return null;
            }
        }
        public static void Run()
        {
            //new MatchmakerAcceptScreenAwakePatch().Enable();
            //new MatchmakerAcceptScreenShowPatch().Enable();
            //new AcceptInvitePatch().Enable();
            //new SendInvitePatch().Enable();
        }

        public static string GetGroupId()
        {
            return groupId;
        }

        public static void SetGroupId(string newId)
        {
            groupId = newId;
        }

        public static bool CheckForMatch()
        {
            return false;
        }

        internal static void CreateMatch(string accountId)
        {
            PatchConstants.Logger.LogInfo($"CreateMatch:: Create Match for {accountId}");

            string text = Request.Instance.PostJson("/coop/server/create", JsonConvert.SerializeObject(
                new Dictionary<string, string>
            {
                { "serverId", accountId }
            }));
            if (!string.IsNullOrEmpty(text))
            {
                PatchConstants.Logger.LogInfo($"CreateMatch:: Match Created for {accountId}");
                SetGroupId(accountId);
                return;
            }

            PatchConstants.Logger.LogError("CreateMatch:: ERROR: Match NOT Created");

        }
    }
}
