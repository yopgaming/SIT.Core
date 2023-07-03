using EFT;
using Newtonsoft.Json;
using SIT.Core.Coop.Matchmaker;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        #region Static Fields

        public static object MatchmakerScreenController
        {
            get
            {
                var screenController = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(MatchMakerAcceptScreenInstance, "ScreenController", false);
                if (screenController != null)
                {
                    PatchConstants.Logger.LogInfo("MatchmakerAcceptPatches.Found ScreenController Instance");

                    return screenController;

                }
                return null;
            }
        }

        public static GameObject EnvironmentUIRoot { get; internal set; }
        #endregion

        public static void Run()
        {
            new EnvironmentUIRootPatch().Enable();
            new MatchmakerAcceptScreenAwakePatch().Enable();
            new MatchmakerAcceptScreenShowPatch().Enable();
        }

        public static string GetGroupId()
        {
            return groupId;
        }

        public static void SetGroupId(string newId)
        {
            groupId = newId;
        }

        public static bool CheckForMatch(RaidSettings settings, out string outJson)
        {
            PatchConstants.Logger.LogInfo("CheckForMatch");
            outJson = string.Empty;

            if (MatchmakerAcceptPatches.MatchMakerAcceptScreenInstance != null)
            {
                outJson = AkiBackendCommunication.Instance.PostJson("/coop/server/exist", JsonConvert.SerializeObject(settings));
                //PatchConstants.Logger.LogInfo(outJson);

                if (!string.IsNullOrEmpty(outJson))
                {
                    bool serverExists = false;
                    if (outJson.Equals("null", StringComparison.OrdinalIgnoreCase))
                    {
                        serverExists = false;
                    }
                    else
                    {
                        serverExists = true;
                    }
                    PatchConstants.Logger.LogInfo($"CheckForMatch:Server Exists?:{serverExists}");

                    return serverExists;
                }
            }
            return false;
        }

        public static void CreateMatch(string accountId, RaidSettings rs)
        {

            string text = AkiBackendCommunication.Instance.PostJson("/coop/server/create", JsonConvert.SerializeObject(
                new Dictionary<string, object>
            {
                { "serverId", accountId }
                , { "settings", rs }
            }));
            if (!string.IsNullOrEmpty(text))
            {
                PatchConstants.Logger.LogInfo($"CreateMatch:: Match Created for {accountId}");
                SetGroupId(accountId);
                MatchmakerAcceptPatches.MatchingType = EMatchmakerType.GroupLeader;
                return;
            }

            PatchConstants.Logger.LogError("CreateMatch:: ERROR: Match NOT Created");

        }
    }
}
