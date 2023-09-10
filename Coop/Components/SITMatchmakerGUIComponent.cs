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
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;
using FontStyle = UnityEngine.FontStyle;

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
        
        private bool showPasswordField = false;
 
        private string passwordInput = "";

        private const float verticalSpacing = 10f;



        private ManualLogSource Logger { get; set; }
        public MatchMakerPlayerPreview MatchMakerPlayerPreview { get; internal set; }

        public Canvas Canvas { get; set; }
        public Profile Profile { get; internal set; }
        public bool showHostGameWindow { get; private set; }
        public Rect hostGameWindowInnerRect { get; private set; }

        public bool showServerBrowserWindow { get; private set; } = true;

        private bool showErrorMessageWindow { get; set; } = false;

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
            // Define the proportions for the main window and the host game window (same size)
            var windowWidthFraction = 0.33f;
            var windowHeightFraction = 0.33f;

            // Calculate the position and size of the main window
            var windowWidth = Screen.width * windowWidthFraction;
            var windowHeight = Screen.height * windowHeightFraction;
            var windowX = (Screen.width - windowWidth) / 2;
            var windowY = (Screen.height - windowHeight) / 2;

            // Create the main window rectangle
            windowRect = new Rect(windowX, windowY, windowWidth, windowHeight);

            if (showServerBrowserWindow)
            {
                windowInnerRect = GUI.Window(0, windowRect, DrawWindow, "Server Browser");

                // Calculate the position for the "Host Game" and "Play Single Player" buttons
                var buttonWidth = 250;
                var buttonHeight = 50;
                var buttonX = (Screen.width - buttonWidth * 2 - 10) / 2;
                var buttonY = Screen.height * 0.75f - buttonHeight;

                // Define a GUIStyle for Host Game and Play single player
                GUIStyle gamemodeButtonStyle = new GUIStyle(GUI.skin.button);
                gamemodeButtonStyle.fontSize = 24;
                gamemodeButtonStyle.fontStyle = FontStyle.Bold;

                // Create "Host Game" button
                if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "Host Game", gamemodeButtonStyle))
                {
                    showServerBrowserWindow = false;
                    showHostGameWindow = true;
                }

                // Create "Play Single Player" button next to the "Host Game" button
                buttonX += buttonWidth + 10;
                if (GUI.Button(new Rect(buttonX, buttonY, buttonWidth, buttonHeight), "Play Single Player", gamemodeButtonStyle))
                {
                    MatchmakerAcceptPatches.MatchingType = EMatchmakerType.Single;
                    OriginalAcceptButton.OnClick.Invoke();
                    DestroyThis();
                }
            }
            else if (showHostGameWindow)
            {
                windowInnerRect = GUI.Window(0, windowRect, DrawHostGameWindow, "Host Game");
            }

            // Handle the "Back" Button
            if (showServerBrowserWindow || showHostGameWindow)
            {
                // Calculate the vertical position
                var backButtonX = (Screen.width - 200) / 2;
                var backButtonY = Screen.height * 0.95f - 40;

                // Define a GUIStyle for the "Back" button with larger and bold text
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = 24;
                buttonStyle.fontStyle = FontStyle.Bold;

                if (GUI.Button(new Rect(backButtonX, backButtonY, 200, 40), "Back", buttonStyle))
                {
                    // Handle the "Back" button click
                    if (showServerBrowserWindow)
                    {
                        OriginalBackButton.OnClick.Invoke();
                        DestroyThis();
                        AkiBackendCommunication.Instance.WebSocketClose();

                    }
                    else if (showHostGameWindow)
                    {
                        // Add logic to go back to the main menu or previous screen
                        showServerBrowserWindow = true;
                        showHostGameWindow = false;
                    }
                }
            }

            if (showErrorMessageWindow)
            {
                windowInnerRect = GUI.Window(0, windowRect, DrawWindowErrorMessage, "Error Message");
            }
        }

        /// <summary>
        /// TODO: Finish this on Error Window
        /// </summary>
        /// <param name="windowID"></param>
        void DrawWindowErrorMessage(int windowID)
        {
            if (!showErrorMessageWindow)
                return;



        }

            void DrawWindow(int windowID)
            {
                // Define column labels
                string[] columnLabels = { "Server", "Players", "Location" };

                // Define the button style
                GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.fontSize = 14;
                buttonStyle.padding = new RectOffset(6, 6, 6, 6);

                // Define the label style
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.MiddleCenter;
                labelStyle.fontSize = 14;
                labelStyle.normal.textColor = Color.white;

                // Calculate the number of rows and columns
                int numRows = 7;
                int numColumns = 3;

                // Calculate cell width and height
                float cellWidth = windowInnerRect.width / (numColumns + 1);
                float cellHeight = (windowInnerRect.height - 40) / numRows;

                // Calculate the vertical positions for lines and labels
                float topSeparatorY = 20;
                float middleSeparatorY = topSeparatorY + cellHeight - 7;
                float bottomSeparatorY = topSeparatorY + numRows * cellHeight;

                // Calculate the width of the separator
                float separatorWidth = 2;

                // Draw the first horizontal line at the top
                GUI.DrawTexture(new Rect(10, topSeparatorY, windowInnerRect.width - 20, separatorWidth), Texture2D.grayTexture);

                // Draw the second horizontal line under Server, Players, and Location
                GUI.DrawTexture(new Rect(10, middleSeparatorY, windowInnerRect.width - 20, separatorWidth), Texture2D.grayTexture);

                // Draw the third horizontal line at the bottom
                GUI.DrawTexture(new Rect(10, bottomSeparatorY, windowInnerRect.width - 20, separatorWidth), Texture2D.grayTexture);

                // Draw vertical separator lines
                for (int col = 1; col < numColumns + 1; col++)
                {
                    float separatorX = col * cellWidth - separatorWidth / 2;
                    GUI.DrawTexture(new Rect(separatorX - 2, topSeparatorY, separatorWidth, bottomSeparatorY - topSeparatorY), Texture2D.grayTexture);
                }

                // Draw column labels at the top
                for (int col = 0; col < 3; col++)
                {
                    float cellX = col * cellWidth + separatorWidth / 2;
                    GUI.Label(new Rect(cellX, topSeparatorY + 5, cellWidth - separatorWidth, 25), columnLabels[col], labelStyle);
                }

                // Reset the GUI.backgroundColor to its original state
                GUI.backgroundColor = Color.white;

                       if (m_Matches != null)
                        {
                            var index = 0;
                            var yPosOffset = 60;

                            foreach (var match in m_Matches)
                            {
                                var yPos = yPosOffset + index * (cellHeight + 5);

                                // Display Host Name with "Raid" label
                                GUI.Label(new Rect(10, yPos, cellWidth - separatorWidth, cellHeight), $"{match["HostName"].ToString()} Raid", labelStyle);

                                // Display Player Count
                                GUI.Label(new Rect(cellWidth, yPos, cellWidth - separatorWidth, cellHeight), match["PlayerCount"].ToString(), labelStyle);

                                // Display Location
                                GUI.Label(new Rect(cellWidth * 2, yPos, cellWidth - separatorWidth, cellHeight), match["Location"].ToString(), labelStyle);

                                // Calculate the width of the combined server information (Host Name, Player Count, Location)
                                var serverInfoWidth = cellWidth * 3 - separatorWidth * 2;

                            // Create "Join" button for each match on the next column
                            if (GUI.Button(new Rect(cellWidth * 3 + separatorWidth / 2 + 15, yPos + (cellHeight * 0.3f) - 5, cellWidth * 0.8f, cellHeight * 0.6f), "Join", buttonStyle))
                            {
                                // Perform actions when the "Join" button is clicked
                                if (MatchmakerAcceptPatches.CheckForMatch(RaidSettings, out string returnedJson))
                                    {
                                        Logger.LogDebug(returnedJson);
                                        JObject result = JObject.Parse(returnedJson);
                                        var groupId = result["ServerId"].ToString();
                                        MatchmakerAcceptPatches.SetGroupId(groupId);
                                        MatchmakerAcceptPatches.MatchingType = EMatchmakerType.GroupPlayer;
                            MatchmakerAcceptPatches.HostExpectedNumberOfPlayers = int.Parse(result["expectedNumberOfPlayers"].ToString());
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
        }

        void HandleJoinButtonClick()
            {
                // Perform actions when the "Join" button is clicked
                if (MatchmakerAcceptPatches.CheckForMatch(RaidSettings, out string returnedJson))
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

        void DrawHostGameWindow(int windowID)
        {
            var rows = 3;
            var halfWindowWidth = windowInnerRect.width / 2;

            // Define a style for the title label
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = 18;
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontStyle = FontStyle.Bold;

            // Define a style for buttons
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 30;
            buttonStyle.fontStyle = FontStyle.Bold;

            for (var iRow = 0; iRow < rows; iRow++)
            {
                var y = 40 + (iRow * 50);

                switch (iRow)
                {
                    case 0:
                        // Title label for the number of players
                        GUI.Label(new Rect(10, y, windowInnerRect.width - 20, 30), "Number of Players to Wait For (Including You)", labelStyle);
                        break;

                    case 1:
                        // Decrease button
                        if (GUI.Button(new Rect(halfWindowWidth - 50, y, 30, 30), "-", buttonStyle))
                        {
                            if (MatchmakerAcceptPatches.HostExpectedNumberOfPlayers > 1)
                            {
                                MatchmakerAcceptPatches.HostExpectedNumberOfPlayers -= 1;
                            }
                        }

                        // Player count label
                        GUI.Label(new Rect(halfWindowWidth - 15, y, 30, 30), MatchmakerAcceptPatches.HostExpectedNumberOfPlayers.ToString(), labelStyle);

                        // Increase button
                        if (GUI.Button(new Rect(halfWindowWidth + 20, y, 30, 30), "+", buttonStyle))
                        {
                            if (MatchmakerAcceptPatches.HostExpectedNumberOfPlayers < 10)
                            {
                                MatchmakerAcceptPatches.HostExpectedNumberOfPlayers += 1;
                            }
                        }
                        break;

                    case 2:
                        // Calculate the width of the "Require Password" text
                        var requirePasswordTextWidth = GUI.skin.label.CalcSize(new GUIContent("Require Password")).x;

                        // Calculate the position for the checkbox and text to center-align them
                        var horizontalSpacing = 10;
                        var checkboxX = halfWindowWidth - requirePasswordTextWidth / 2 - horizontalSpacing;
                        var textX = checkboxX + 20;

                        // Disable the checkbox to prevent interaction
                        GUI.enabled = false;

                        // Checkbox to toggle the password field visibility
                        showPasswordField = GUI.Toggle(new Rect(checkboxX, y, 200, 30), showPasswordField, "");

                        // "Require Password" text
                        GUI.Label(new Rect(textX, y, requirePasswordTextWidth, 30), "Require Password");

                        // Feature is currently unavailable
                        var featureUnavailableLabelStyle = new GUIStyle(GUI.skin.label)
                        {
                            fontStyle = FontStyle.Italic,
                            normal = { textColor = Color.gray },
                            fontSize = 10
                        };
                        GUI.Label(new Rect(textX, y + 20, requirePasswordTextWidth, 30), "soonTM", featureUnavailableLabelStyle);

                        // Reset GUI.enabled to enable other elements
                        GUI.enabled = true;

                        // Password field (visible only when the checkbox is checked)
                        var passwordFieldWidth = 200;
                        var passwordFieldX = halfWindowWidth - passwordFieldWidth / 2;

                        if (showPasswordField)
                        {
                            passwordInput = GUI.PasswordField(new Rect(220, y, 200, 30), passwordInput, '*', 25);
                            y += 30;
                        }

                        break;


                }
            }

            // Style for back and start button
            GUIStyle smallButtonStyle = new GUIStyle(GUI.skin.button);
            smallButtonStyle.fontSize = 18;
            smallButtonStyle.alignment = TextAnchor.MiddleCenter;

            // Back button
            if (GUI.Button(new Rect(10, windowInnerRect.height - 60, halfWindowWidth - 20, 30), "Back", smallButtonStyle))
            {
                showHostGameWindow = false;
                showServerBrowserWindow = true;
            }

            // Start button
            if (GUI.Button(new Rect(halfWindowWidth + 10, windowInnerRect.height - 60, halfWindowWidth - 20, 30), "Start", smallButtonStyle))
            {
                MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.ProfileId, RaidSettings);
                OriginalAcceptButton.OnClick.Invoke();
                DestroyThis();
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