using EFT;
using EFT.UI;
using EFT.UI.Matchmaker;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SIT.Core.SP.Menus
{
    public class AutoSetOfflineMatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MatchmakerOfflineRaidScreen);
            var desiredMethod = ReflectionHelpers.GetAllMethodsForType(desiredType).Single(x => x.Name == "Show" && x.GetParameters().Length == 2);

            Logger.LogInfo($"{GetType().Name} Type: {desiredType?.Name}");
            Logger.LogInfo($"{GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        public static bool Prefix(
            MatchmakerOfflineRaidScreen __instance
            , ProfileInfo profileInfo
            , RaidSettings raidSettings
            , UpdatableToggle ____offlineModeToggle
            , DefaultUIButton ____changeSettingsButton
            , UiElementBlocker ____onlineBlocker
            , DefaultUIButton ____readyButton
           )
        {
            //Logger.LogInfo(JsonConvert.SerializeObject(raidSettings));



            raidSettings.RaidMode = ERaidMode.Local;
            RemoveBlockers(__instance
              , profileInfo
              , raidSettings
              , ____offlineModeToggle
              , ____changeSettingsButton
              , ____onlineBlocker
              , ____readyButton
              );
            //return false;
            return true;
        }

        [PatchPostfix]
        public static void PatchPostfix(
           MatchmakerOfflineRaidScreen __instance
            , ProfileInfo profileInfo
            , RaidSettings raidSettings
            , UpdatableToggle ____offlineModeToggle
            , DefaultUIButton ____changeSettingsButton
            , UiElementBlocker ____onlineBlocker
            , DefaultUIButton ____readyButton
            )
        {
            var warningPanel = GameObject.Find("Warning Panel");
            Object.Destroy(warningPanel);
            RemoveBlockers(__instance
             , profileInfo
             , raidSettings
             , ____offlineModeToggle
             , ____changeSettingsButton
             , ____onlineBlocker
             , ____readyButton
             );

            ____changeSettingsButton.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(null) { });

            //Logger.LogInfo("AutoSetOfflineMatch2.Postfix");

        }

        public static void RemoveBlockers(
            MatchmakerOfflineRaidScreen __instance
            , ProfileInfo profileInfo
            , RaidSettings raidSettings
            , UpdatableToggle ____offlineModeToggle
            , DefaultUIButton ____changeSettingsButton
            , UiElementBlocker ____onlineBlocker
            , DefaultUIButton ____readyButton
            )
        {
            raidSettings.RaidMode = ERaidMode.Local;
            ____onlineBlocker.RemoveBlock();
            ____onlineBlocker.enabled = false;
            ____offlineModeToggle.isOn = true;

            raidSettings.RaidMode = ERaidMode.Local;
            raidSettings.BotSettings.IsEnabled = true;
            raidSettings.Side = ESideType.Pmc;
            ____changeSettingsButton.Interactable = true;
            ____changeSettingsButton.enabled = true;
            ____readyButton.Interactable = false;
            ____readyButton.enabled = false;
            //Logger.LogInfo("AutoSetOfflineMatch2.RemoveBlockers");
        }
    }

}