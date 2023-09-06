using BepInEx.Logging;
using EFT;
using EFT.UI;
using EFT.UI.Matchmaker;
using Newtonsoft.Json.Linq;
using SIT.Coop.Core.Matchmaker;
using SIT.Core.Core;
using SIT.Core.Misc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.Components
{
    internal class SITMatchmakerGUIComponent : MonoBehaviour
    {
        private Rect windowRect = new(20, 20, 120, 50);
        private Rect windowInnerRect { get; set; } = new Rect(20, 20, 120, 50);
        private GUIStyle styleBrowserRaidLabel { get; } = new GUIStyle();
        private GUIStyle styleBrowserRaidRow { get; } = new GUIStyle() { };
        private GUIStyle styleBrowserRaidLink { get; } = new GUIStyle();

        //private GUIStyleState styleStateBrowserWindowNormal { get; } = new GUIStyleState()
        //{
        //    textColor = Color.white
        //};
        private GUIStyle styleBrowserWindow { get; set; }

        private GUIStyleState styleStateBrowserBigButtonsNormal { get; } = new GUIStyleState()
        {
            //background = 
            textColor = Color.white
        };
        private GUIStyle styleBrowserBigButtons { get; set; }

        public RaidSettings RaidSettings { get; internal set; }
        public DefaultUIButton OriginalBackButton { get; internal set; }
        public DefaultUIButton OriginalAcceptButton { get; internal set; }

        private Task GetMatchesTask { get; set; }

        private Dictionary<string, object>[] m_Matches { get; set; }

        private CancellationTokenSource m_cancellationTokenSource;
        private bool StopAllTasks = false;
        private ManualLogSource Logger { get; set; }
        public MatchMakerPlayerPreview MatchMakerPlayerPreview { get; internal set; }

        public Canvas Canvas { get; set; }
        public Profile Profile { get; internal set; }
        public bool showHostGameWindow { get; private set; }
        public Rect hostGameWindowInnerRect { get; private set; }
        public bool showServerBrowserWindow { get; private set; } = true;

        void Start()
        {
            // Setup Logger
            Logger = BepInEx.Logging.Logger.CreateLogSource("SIT Matchmaker GUI");
            Logger.LogInfo("Start");
            // Get Canvas
            Canvas = GameObject.FindObjectOfType<Canvas>();
            if (Canvas != null)
            {
                Logger.LogInfo("Canvas found");
                foreach (Transform b in Canvas.GetComponents<Transform>())
                {
                    Logger.LogInfo(b);
                }
                //Canvas.GetComponent<UnityEngine.GUIText>();
            }

            // Create background Texture
            Texture2D texture2D = new(128, 128);
            texture2D.Fill(Color.black);
            styleStateBrowserBigButtonsNormal.background = texture2D;
            styleStateBrowserBigButtonsNormal.textColor = Color.black;
            //styleStateBrowserWindowNormal.background = texture2D;
            //styleStateBrowserWindowNormal.textColor = Color.white;

            // Create Skin for Window
            //GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
            //skin.window = new GUIStyle();
            //skin.window.alignment = TextAnchor.MiddleLeft;
            //skin.window.normal = styleStateBrowserWindowNormal;

            m_cancellationTokenSource = new CancellationTokenSource();
            styleBrowserBigButtons = new GUIStyle()
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = styleStateBrowserBigButtonsNormal,
                active = styleStateBrowserBigButtonsNormal,
                hover = styleStateBrowserBigButtonsNormal,
            };

            styleBrowserWindow = new GUIStyle();
            //styleBrowserWindow.normal = styleStateBrowserWindowNormal;
            //styleBrowserWindow.onNormal = styleStateBrowserWindowNormal;
            styleBrowserWindow.active = styleBrowserWindow.normal;
            styleBrowserWindow.onActive = styleBrowserWindow.onNormal;
            //styleBrowserWindow.hover = styleStateBrowserWindowNormal;

            GetMatches();
            StartCoroutine(ResolveMatches());
            DisableBSGButtons();
            RemovePlayerCharacter();
            RemoveOldPanels();
        }

        private void RemoveOldPanels()
        {
            var playerNamePanel = ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(MatchMakerPlayerPreview), typeof(PlayerNamePanel)).GetValue(MatchMakerPlayerPreview) as PlayerNamePanel;
            if (playerNamePanel == null)
            {
                Logger.LogError("Unable to retrieve PlayerNamePanel");
                return;
            }

            //var playerLevelPanel = MatchMakerPlayerPreview.GetComponent<PlayerLevelPanel>();
            var playerLevelPanel = ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(MatchMakerPlayerPreview), typeof(PlayerLevelPanel)).GetValue(MatchMakerPlayerPreview) as PlayerLevelPanel;
            if (playerLevelPanel == null)
            {
                Logger.LogError("Unable to retrieve PlayerLevelPanel");
                return;
            }

            playerNamePanel.gameObject.SetActive(false);
            playerLevelPanel.gameObject.SetActive(false);

            //RectTransform tempRectTransform = playerLevelPanel.GetComponent<RectTransform>();
            //tempRectTransform.anchoredPosition = new Vector2(-1000, 0);
            //tempRectTransform.offsetMax = new Vector2(-1000, 0);
            //tempRectTransform.offsetMin = new Vector2(-1000, 0);
            //tempRectTransform.anchoredPosition3D = new Vector3(-1000, 0, 0);
        }

        private void DisableBSGButtons()
        {
            OriginalAcceptButton.gameObject.SetActive(false);
            OriginalAcceptButton.enabled = false;
            OriginalAcceptButton.Interactable = false;
            OriginalBackButton.gameObject.SetActive(false);
            OriginalBackButton.enabled = false;
            OriginalBackButton.Interactable = false;
        }

        private void RemovePlayerCharacter()
        {
            var pmv = ReflectionHelpers.GetFieldFromTypeByFieldType(MatchMakerPlayerPreview.GetType(), typeof(PlayerModelView)).GetValue(MatchMakerPlayerPreview) as PlayerModelView;
            if (pmv == null)
            {
                Logger.LogError("Unable to retrieve PlayerModelView");
                return;
            }

            var position = (Vector3)ReflectionHelpers.GetFieldFromType(typeof(PlayerModelView), "_position").GetValue(pmv);
            if (position == null)
            {
                Logger.LogError("Unable to retrieve _position");
                return;
            }

            position.x = 7000f;
            pmv.enabled = false;

            if (pmv.PlayerBody == null)
                return;

            pmv.PlayerBody.enabled = false;

        }

        void GetMatches()
        {
            CancellationToken ct = m_cancellationTokenSource.Token;
            GetMatchesTask = Task.Run(async () =>
            {
                while (!StopAllTasks)
                {
                    //AkiBackendCommunication.Instance.CreateWebSocket(Profile);

                    //var result = AkiBackendCommunication.Instance.PostJsonAsync<Dictionary<string, object>[]>("/coop/server/getAllForLocation", RaidSettings.ToJson()).Result;
                    var result = await AkiBackendCommunication.Instance.PostJsonAsync<Dictionary<string, object>[]>("/coop/server/getAllForLocation", RaidSettings.ToJson(), timeout: 4000, debug: false);
                    if (result != null)
                    {
                        m_Matches = result;
                    }

                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }

                    await Task.Delay(7000);

                    if (ct.IsCancellationRequested)
                    {
                        ct.ThrowIfCancellationRequested();
                    }
                }
            }, ct);
        }

        IEnumerator ResolveMatches()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DestroyThis();
            }

            RemovePlayerCharacter();
            RemoveOldPanels();
        }

        void OnGUI()
        {

            //var w = 0.33f; // proportional width (0..1)
            var h = 0.9f; // proportional height (0..1)
            //windowRect.x = (float)(Screen.width * (1 - w)) / 2;
            //windowRect.y = (float)(Screen.height * (1 - h)) / 2;
            //windowRect.width = Screen.width * w;
            //windowRect.height = Screen.height * h;

            windowRect.x = Screen.width * 0.01f;// (float)(Screen.width * (1 - w)) / 2;
            windowRect.y = (float)(Screen.height * (1 - h)) / 2;
            windowRect.width = Screen.width * 0.3f;
            windowRect.height = Screen.height * h;

            if (showServerBrowserWindow)
            {
                windowInnerRect = GUI.Window(0, windowRect, DrawWindow, "");
            }
            if(showHostGameWindow)
            {
                var hostGameWindowRect = new Rect();
                hostGameWindowRect.x = (float)(Screen.width * 0.33f);
                hostGameWindowRect.y = Screen.height * 0.2f;
                hostGameWindowRect.width = Screen.width * 0.33f;
                hostGameWindowRect.height = Screen.height * 0.33f;
                hostGameWindowInnerRect = GUI.Window(1, hostGameWindowRect, DrawHostGameWindow, "");
            }
        }

        void DrawWindow(int windowID)
        {
            if (GUI.Button(new Rect(10, 20, (windowInnerRect.width / 2) - 20, 20), Plugin.LanguageDictionary["HOST_RAID"], styleBrowserBigButtons))
            {
                //MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.ProfileId, RaidSettings);
                //OriginalAcceptButton.OnClick.Invoke();
                //DestroyThis();
                showServerBrowserWindow = false;
                showHostGameWindow = true;
            }

            if (GUI.Button(new Rect((windowInnerRect.width / 2) + 10, 20, (windowInnerRect.width / 2) - 20, 20), Plugin.LanguageDictionary["PLAY_SINGLE_PLAYER"], styleBrowserBigButtons))
            {
                MatchmakerAcceptPatches.MatchingType = EMatchmakerType.Single;
                OriginalAcceptButton.OnClick.Invoke();
                DestroyThis();

            }

            GUI.Label(new Rect(10, 45, (windowInnerRect.width / 4), 25), Plugin.LanguageDictionary["SERVER"]);
            GUI.Label(new Rect(10 + (windowInnerRect.width * 0.5f), 45, (windowInnerRect.width / 4), 25), Plugin.LanguageDictionary["PLAYERS"]);
            GUI.Label(new Rect(10 + (windowInnerRect.width * 0.65f), 45, (windowInnerRect.width / 4), 25), Plugin.LanguageDictionary["LOCATION"]);
            //GUI.Label(new Rect(10 + (windowInnerRect.width * 0.9f), 45, (windowInnerRect.width / 4), 25), "PING");

            if (m_Matches != null)
            {
                var index = 0;
                foreach (var match in m_Matches)
                {
                    var yPos = 60 + (index + 25);
                    GUI.Label(new Rect(10, yPos, (windowInnerRect.width / 4), 25), $"{match["HostName"].ToString()} {Plugin.LanguageDictionary["RAID"]}");
                    GUI.Label(new Rect(10 + (windowInnerRect.width * 0.5f), yPos, (windowInnerRect.width / 4), 25), match["PlayerCount"].ToString());
                    GUI.Label(new Rect(10 + (windowInnerRect.width * 0.65f), yPos, (windowInnerRect.width / 4), 25), match["Location"].ToString());
                    //GUI.Label(new Rect(10 + (windowInnerRect.width * 0.9f), yPos, (windowInnerRect.width / 4), 25), "-");
                    //Logger.LogInfo(match.ToJson());
                    if (GUI.Button(new Rect(10 + (windowInnerRect.width * 0.85f), yPos, (windowInnerRect.width * 0.15f) - 20, 20)
                        , $"Join"
                        , styleBrowserBigButtons
                        ))
                    {
                        if (MatchmakerAcceptPatches.CheckForMatch(RaidSettings, out string returnedJson))
                        {
                            Logger.LogDebug(returnedJson);
                            JObject result = JObject.Parse(returnedJson);

                            if (!result.ContainsKey("ServerId"))
                            {
                                throw new ArgumentNullException("ServerId");
                            }

                            var groupId = result["ServerId"].ToString();
                            MatchmakerAcceptPatches.SetGroupId(groupId);
                            MatchmakerAcceptPatches.MatchingType = EMatchmakerType.GroupPlayer;
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            GC.Collect();
                            DestroyThis();
                            OriginalAcceptButton.OnClick.Invoke();
                        }
                    }
                    index++;
                }
            }

            // Back button
            if (GUI.Button(new Rect((windowInnerRect.width / 2) + 10, windowInnerRect.height - 40, (windowInnerRect.width / 2) - 20, 20), Plugin.LanguageDictionary["BACK"], styleBrowserBigButtons))
            {
                OriginalBackButton.OnClick.Invoke();
                DestroyThis();
                AkiBackendCommunication.Instance.WebSocketClose();
            }
        }

        void DrawHostGameWindow(int windowID)
        {
            var rows = 2;
            var halfWindowWidth = hostGameWindowInnerRect.width / 2;

            for (var iRow = 0; iRow < rows; iRow++)
            {
                var y = 20 + (iRow * 25);
                switch (iRow)
                {
                    case 0:
                        GUI.Label(new Rect(10, y, halfWindowWidth, 20), "Number of players to wait for (including you)", new GUIStyle() {  fontSize = 14, normal = new GUIStyleState() { textColor = Color.white } });
                        break;
                    case 1:
                        if (GUI.Button(new Rect(10, y, 100, 20), "-", styleBrowserBigButtons))
                        {
                            if (MatchmakerAcceptPatches.HostExpectedNumberOfPlayers - 1 > 0)
                            {
                                MatchmakerAcceptPatches.HostExpectedNumberOfPlayers -= 1;
                            }

                            //MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.ProfileId, RaidSettings);
                            //OriginalAcceptButton.OnClick.Invoke();
                            //DestroyThis();
                        }

                        GUI.Label(new Rect(halfWindowWidth, y, 100, 20), MatchmakerAcceptPatches.HostExpectedNumberOfPlayers.ToString());

                        if (GUI.Button(new Rect((hostGameWindowInnerRect.width - 100) - 20, y, 100, 20), "+", styleBrowserBigButtons))
                        {
                            //MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.ProfileId, RaidSettings);
                            //OriginalAcceptButton.OnClick.Invoke();
                            //DestroyThis();
                            if (MatchmakerAcceptPatches.HostExpectedNumberOfPlayers + 1 < 11)
                            {
                                MatchmakerAcceptPatches.HostExpectedNumberOfPlayers += 1;
                            }
                        }
                        break;
                }

            }


            // Start button
            if (GUI.Button(new Rect(10, hostGameWindowInnerRect.height - 40, (hostGameWindowInnerRect.width / 2) - 20, 20), "Start", styleBrowserBigButtons))
            {
                MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.ProfileId, RaidSettings);
                OriginalAcceptButton.OnClick.Invoke();
                DestroyThis();
            }

            // Close button
            if (GUI.Button(new Rect((hostGameWindowInnerRect.width / 2) + 10, hostGameWindowInnerRect.height - 40, (hostGameWindowInnerRect.width / 2) - 20, 20), "Close", styleBrowserBigButtons))
            {
                showHostGameWindow = false;
                showServerBrowserWindow = true;
            }
        }


        void OnDestroy()
        {
            if (m_cancellationTokenSource != null)
                m_cancellationTokenSource.Cancel();


            StopAllTasks = true;
        }

        void DestroyThis()
        {
            StopAllTasks = true;

            GameObject.DestroyImmediate(this.gameObject);
            GameObject.DestroyImmediate(this);
        }


    }
}
