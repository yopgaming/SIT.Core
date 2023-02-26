using EFT;
using EFT.Bots;
using EFT.UI;
using EFT.UI.Matchmaker;
using GPUInstancer;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;

namespace SIT.Tarkov.Core.Menus
{
    public class AutoSetOfflineMatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MatchmakerOfflineRaidScreen);
            var desiredMethod = PatchConstants.GetAllMethodsForType(desiredType).Single(x => x.Name == "Show" && x.GetParameters().Length == 2);

            Logger.LogInfo($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogInfo($"{this.GetType().Name} Method: {desiredMethod?.Name}");

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
           )
        {
            Logger.LogInfo(JsonConvert.SerializeObject(raidSettings));
            raidSettings.RaidMode = ERaidMode.Local;
            RemoveBlockers(__instance
              , profileInfo
              , raidSettings
              , ____offlineModeToggle
              , ____changeSettingsButton
              , ____onlineBlocker);
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
            )
        {
            var warningPanel = GameObject.Find("Warning Panel");
            UnityEngine.Object.Destroy(warningPanel);
            RemoveBlockers(__instance
             , profileInfo
             , raidSettings
             , ____offlineModeToggle
             , ____changeSettingsButton
             , ____onlineBlocker);

            //Logger.LogInfo("AutoSetOfflineMatch2.Postfix");

        }

        public static void RemoveBlockers(
            MatchmakerOfflineRaidScreen __instance
            , ProfileInfo profileInfo
            , RaidSettings raidSettings
            , UpdatableToggle ____offlineModeToggle
            , DefaultUIButton ____changeSettingsButton
            , UiElementBlocker ____onlineBlocker)
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
            //Logger.LogInfo("AutoSetOfflineMatch2.RemoveBlockers");
        }
    }

}