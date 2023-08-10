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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.Components
{
    internal class SITMatchmakerGUIComponent : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 120, 50);
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
            Texture2D texture2D = new Texture2D(128, 128);
            texture2D.Fill(Color.white);
            //styleStateBrowserWindowNormal.background = texture2D;
            //styleStateBrowserWindowNormal.textColor = Color.white;

            // Create Skin for Window
            GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
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
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                DestroyThis();
            }

            RemovePlayerCharacter();
            RemoveOldPanels();
        }

        void OnGUI()
        {
            //var w = 0.1f; // proportional width (0..1)
            var h = 0.9f; // proportional height (0..1)
            //windowRect.x = (float)(Screen.width * (1 - w)) / 2;
            //windowRect.y = (float)(Screen.height * (1 - h)) / 2;
            //windowRect.width = Screen.width * w;
            //windowRect.height = Screen.height * h;

            windowRect.x = Screen.width * 0.01f;// (float)(Screen.width * (1 - w)) / 2;
            windowRect.y = (float)(Screen.height * (1 - h)) / 2;
            windowRect.width = Screen.width * 0.3f;
            windowRect.height = Screen.height * h;

            windowInnerRect = GUI.Window(0, windowRect, DrawWindow, "", styleBrowserWindow);
        }

        void DrawWindow(int windowID)
        {
            if (GUI.Button(new Rect(10, 20, (windowInnerRect.width / 2) - 20, 20), "Host Match", styleBrowserBigButtons))
            {

                MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.AccountId, RaidSettings);
                OriginalAcceptButton.OnClick.Invoke();
                DestroyThis();

            }

            if (GUI.Button(new Rect((windowInnerRect.width / 2) + 10, 20, (windowInnerRect.width / 2) - 20, 20), "Play Single Player", styleBrowserBigButtons))
            {
                OriginalAcceptButton.OnClick.Invoke();
                MatchmakerAcceptPatches.MatchingType = EMatchmakerType.Single;
                DestroyThis();

            }

            GUI.Label(new Rect(10, 45, (windowInnerRect.width / 4), 25), "SERVER");
            GUI.Label(new Rect(10 + (windowInnerRect.width * 0.55f), 45, (windowInnerRect.width / 4), 25), "PLAYERS");
            GUI.Label(new Rect(10 + (windowInnerRect.width * 0.7f), 45, (windowInnerRect.width / 4), 25), "LOCATION");
            //GUI.Label(new Rect(10 + (windowInnerRect.width * 0.9f), 45, (windowInnerRect.width / 4), 25), "PING");

            if (m_Matches != null)
            {
                var index = 0;
                foreach (var match in m_Matches)
                {
                    var yPos = 60 + (index + 25);
                    GUI.Label(new Rect(10, yPos, (windowInnerRect.width / 4), 25), $"{match["HostName"].ToString()}'s Raid");
                    GUI.Label(new Rect(10 + (windowInnerRect.width * 0.55f), yPos, (windowInnerRect.width / 4), 25), match["PlayerCount"].ToString());
                    GUI.Label(new Rect(10 + (windowInnerRect.width * 0.7f), yPos, (windowInnerRect.width / 4), 25), match["Location"].ToString());
                    //GUI.Label(new Rect(10 + (windowInnerRect.width * 0.9f), yPos, (windowInnerRect.width / 4), 25), "-");
                    Logger.LogInfo(match.ToJson());
                    if (GUI.Button(new Rect(10 + (windowInnerRect.width * 0.9f), yPos, (windowInnerRect.width * 0.1f) - 20, 20)
                        , $"Join"
                        ))
                    {
                        if(MatchmakerAcceptPatches.CheckForMatch(RaidSettings, out string returnedJson))
                        {
                            Logger.LogDebug(returnedJson);
                            JObject result = JObject.Parse(returnedJson);
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
            if (GUI.Button(new Rect((windowInnerRect.width / 2) + 10, windowInnerRect.height - 40, (windowInnerRect.width / 2) - 20, 20), "Back (Esc)", styleBrowserBigButtons))
            {
                OriginalBackButton.OnClick.Invoke();
                DestroyThis();
            }
        }

        void OnDestroy()
        {
            if(m_cancellationTokenSource != null)   
                m_cancellationTokenSource.Cancel();
        }

        void DestroyThis()
        {
            GameObject.DestroyImmediate(this.gameObject);
            GameObject.DestroyImmediate(this);

        }

        
    }
}
