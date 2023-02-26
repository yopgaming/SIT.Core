using EFT;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using UnityEngine;
using SIT.Core.Coop.Matchmaker;

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
            PatchConstants.Logger.LogInfo("CheckForMatch");

            if (MatchmakerAcceptPatches.MatchMakerAcceptScreenInstance != null)
            {
                string json = new SIT.Tarkov.Core.Request().GetJson("/coop/server/exist");
                PatchConstants.Logger.LogInfo(json);

                if (!string.IsNullOrEmpty(json))
                {
                    bool serverExists = false;
                    if (json.Equals("null", StringComparison.OrdinalIgnoreCase))
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
                //    string json = new SIT.Tarkov.Core.Request().GetJson("/coop/get-invites");
                //    if (!string.IsNullOrEmpty(json))
                //    {
                //        var gClass = Activator.CreateInstance(MatchmakerAcceptPatches.InviteType);
                //        gClass = JsonConvert.DeserializeObject(json, MatchmakerAcceptPatches.InviteType);
                //        var from = Tarkov.Core.PatchConstants.GetFieldOrPropertyFromInstance<string>(gClass, "From");
                //        if (gClass != null && !string.IsNullOrEmpty(from))
                //        {
                //            PatchConstants.Logger.LogInfo($"Invite Popup! {gClass} {from}");

                    //            PatchConstants.Logger.LogInfo("GetMatchStatus");
                    //            string text = new SIT.Tarkov.Core.Request().PostJson("/coop/server/read", JsonConvert.SerializeObject(from));
                    //            if (!string.IsNullOrEmpty(text))
                    //            {
                    //                //PatchConstants.Logger.LogInfo("GetMatchStatus[1] ::" + text.Length);
                    //                ServerStatus serverStatus = JsonConvert.DeserializeObject<ServerStatus>(text);
                    //                PatchConstants.Logger.LogInfo("GetMatchStatus[2] ::" + serverStatus.status);
                    //                if (serverStatus.status == "LOADING" || serverStatus.status == "INGAME")
                    //                {
                    //                    PatchConstants.Logger.LogInfo("GetMatchStatus[3] :: Starting up");
                    //                    //MatchmakerAcceptPatches.SetGroupId(from);
                    //                    MatchmakerAcceptPatches.MatchingType = EMatchmakerType.GroupPlayer;
                    //                    MatchmakerAcceptPatches.SetGroupId(from);
                    //                    PatchConstants.DisplayMessageNotification("Server is running and waiting for you to join...");
                    //                    return true;
                    //                    //MatchmakerAcceptPatches.MatchmakerScreenController.GetType().GetMethod("ShowNextScreen", BindingFlags.Public | BindingFlags.Instance).Invoke(MatchmakerAcceptPatches.MatchmakerScreenController, new object[] { from, EFT.UI.Matchmaker.EMatchingType.GroupPlayer });

                    //                }
                    //            }
                    //        }
                    //        else
                    //        {
                    //            //MatchmakerAcceptPatches.MatchingType = EMatchmakerType.Single;
                    //        }
                    //    }
            }
            return false;
        }

        //public static string GroupingPropertyName { get { return "gclass2434_0"; } }
        public static EMatchmakerType MatchingType { get; set; } = EMatchmakerType.Single;
        public static bool ForcedMatchingType { get; set; }

        public static bool IsServer => MatchingType == EMatchmakerType.GroupLeader;

        public static bool IsClient => MatchingType == EMatchmakerType.GroupPlayer;

        public static bool IsSinglePlayer => MatchingType == EMatchmakerType.Single;
        public static int HostExpectedNumberOfPlayers { get; set; }
        public static GameObject EnvironmentUIRoot { get; internal set; }

        //public static Type InviteType { get; } = PatchConstants.EftTypes.Single(x =>
        //    (PatchConstants.GetPropertyFromType(x, "Id") != null
        //    || PatchConstants.GetFieldFromType(x, "Id") != null)
        //    && (PatchConstants.GetPropertyFromType(x, "From") != null
        //    || PatchConstants.GetFieldFromType(x, "From") != null)
        //    && (PatchConstants.GetPropertyFromType(x, "To") != null
        //    || PatchConstants.GetFieldFromType(x, "To") != null)
        //    && (PatchConstants.GetPropertyFromType(x, "GroupId") != null
        //    || PatchConstants.GetFieldFromType(x, "GroupId") != null)
        //    && (PatchConstants.GetPropertyFromType(x, "FromProfile") != null
        //    || PatchConstants.GetFieldFromType(x, "FromProfile") != null)
        //);

        //public static MethodInfo InvitePopupMethod { get; } = PatchConstants.GetAllMethodsForType(typeof(EFT.UI.Matchmaker.MatchMakerAcceptScreen))
        //    .First(x => x.GetParameters().Length >= 1 && x.GetParameters()[0].ParameterType == InviteType && x.GetParameters()[0].Name == "invite");

        public static void Run()
        {
            new EnvironmentUIRootPatch().Enable();
            new MatchmakerAcceptScreenAwakePatch().Enable();
            new MatchmakerAcceptScreenShowPatch().Enable();
            //new AcceptInvitePatch().Enable();
            //new SendInvitePatch().Enable();
        }

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
