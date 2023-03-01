using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.DefaultControls;
using UnityEngine.UI;
using SIT.Tarkov.Core;

namespace SIT.Core.Coop
{
    internal class CoopUI : MonoBehaviour
    {
        public static readonly GameObject CoopUIHUD = new GameObject("CoopUIHUD", new Type[] { typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster) });

        void Start()
        {

            PatchConstants.Logger.LogInfo("CoopUI.Start");

            Canvas canvs = CoopUIHUD.GetComponent<Canvas>();
            canvs.renderMode = RenderMode.ScreenSpaceOverlay;
            canvs.sortingOrder = 1;
            canvs.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;
            DontDestroyOnLoad(CoopUIHUD);

            var textGO = CreateText(new DefaultControls.Resources() { });
            textGO.RectTransform().SetParent(transform);
            Text text = textGO.GetComponent<Text>();
            text.text = "HELLO WORLD!!";
            DontDestroyOnLoad(textGO);

        }
    }
}
