using EFT;
using EFT.UI;
using SIT.Coop.Core.Matchmaker;
using SIT.Core.Core;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace SIT.Core.Coop.Components
{
    internal class SITMatchmakerGUIComponent : MonoBehaviour
    {
        public Rect windowRect = new Rect(20, 20, 120, 50);
        public Rect windowInnerRect = new Rect(20, 20, 120, 50);

        public RaidSettings RaidSettings { get; internal set; }
        public DefaultUIButton OriginalBackButton { get; internal set; }
        public DefaultUIButton OriginalAcceptButton { get; internal set; }

        private Task GetMatchesTask { get; set; }

        private Dictionary<string, object>[] m_Matches { get; set; }

        void Start()
        {
            GetMatches();
            StartCoroutine(ResolveMatches());
        }

        void GetMatches()
        {
            GetMatchesTask = Task.Run(async () =>
            {
                while (true)
                {
                    //var result = AkiBackendCommunication.Instance.PostJsonAsync<Dictionary<string, object>[]>("/coop/server/getAllForLocation", RaidSettings.ToJson()).Result;
                    var result = await AkiBackendCommunication.Instance.PostJsonAsync<Dictionary<string, object>[]>("/coop/server/getAllForLocation", RaidSettings.ToJson(), timeout: 4000, debug: false);
                    if (result != null)
                    {
                        m_Matches = result;
                    }
                    await Task.Delay(7000);
                }
            });
        }

        IEnumerator ResolveMatches()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
            }
        }

        void OnGUI()
        {
            var w = 0.5f; // proportional width (0..1)
            var h = 0.8f; // proportional height (0..1)
            windowRect.x = (float)(Screen.width * (1 - w)) / 2;
            windowRect.y = (float)(Screen.height * (1 - h)) / 2;
            windowRect.width = Screen.width * w;
            windowRect.height = Screen.height * h;

            windowInnerRect = GUI.Window(0, windowRect, DrawWindow, "SIT Match Browser");
        }

        void DrawWindow(int windowID)
        {
            if (GUI.Button(new Rect(10, 20, (windowInnerRect.width / 2) - 20, 20), "Host Match"))
            {

                MatchmakerAcceptPatches.CreateMatch(MatchmakerAcceptPatches.Profile.AccountId, RaidSettings);
                OriginalAcceptButton.OnClick.Invoke();
                DestroyThis();

            }

            if (GUI.Button(new Rect((windowInnerRect.width / 2) + 10, 20, (windowInnerRect.width / 2) - 20, 20), "Play Single Player"))
            {
                OriginalAcceptButton.OnClick.Invoke();
                MatchmakerAcceptPatches.MatchingType = EMatchmakerType.Single;
                DestroyThis();

            }

            if(m_Matches != null)
            {
                foreach (var match in m_Matches)
                {
                    PatchConstants.Logger.LogInfo(match);
                }
            }

            // Back button
            if (GUI.Button(new Rect((windowInnerRect.width / 2) + 10, windowInnerRect.height - 40, (windowInnerRect.width / 2) - 20, 20), "Back"))
            {
                OriginalBackButton.OnClick.Invoke();
                DestroyThis();
            }
        }

        void OnDestroy()
        {
            GetMatchesTask.Dispose();
            GetMatchesTask = null;
        }

        void DestroyThis()
        {
            GameObject.DestroyImmediate(this.gameObject);
            GameObject.DestroyImmediate(this);
        }

        
    }
}
