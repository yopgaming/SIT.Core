//using CoopTarkovGameServer;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using SIT.Coop.Core.Matchmaker;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Coop.Core.LocalGame
{
    /// <summary>
    /// Target that smethod_3 like
    /// </summary>
    public class LocalGameStartingPatch : ModulePatch
    {
        //public static EchoGameServer gameServer;
        private static ConfigFile _config;

        //private static LocalGameSpawnAICoroutinePatch gameSpawnAICoroutinePatch;

        public LocalGameStartingPatch(ConfigFile config)
        {
            _config = config;
            //gameSpawnAICoroutinePatch = new SIT.Coop.Core.LocalGame.LocalGameSpawnAICoroutinePatch(_config);
        }

        public static TimeAndWeatherSettings TimeAndWeather { get; internal set; }



        protected override MethodBase GetTargetMethod()
        {
            //foreach(var ty in SIT.Tarkov.Core.PatchConstants.EftTypes.Where(x => x.Name.StartsWith("BaseLocalGame")))
            //{
            //    Logger.LogInfo($"LocalGameStartingPatch:{ty}");
            //}
            _ = typeof(EFT.BaseLocalGame<GamePlayerOwner>);

            //var t = SIT.Tarkov.Core.PatchConstants.EftTypes.FirstOrDefault(x => x.FullName.StartsWith("EFT.LocalGame"));
            var t = typeof(EFT.LocalGame);
            //var t = typeof(EFT.BaseLocalGame<GamePlayerOwner>);
            if (t == null)
                Logger.LogInfo($"LocalGameStartingPatch:Type is NULL");

            var method = ReflectionHelpers.GetAllMethodsForType(t, false)
                .FirstOrDefault(x => x.GetParameters().Length >= 3
                && x.GetParameters().Any(x => x.Name.Contains("botsSettings"))
                && x.GetParameters().Any(x => x.Name.Contains("backendUrl"))
                && x.GetParameters().Any(x => x.Name.Contains("runCallback"))
                );

            Logger.LogInfo($"LocalGameStartingPatch:{t.Name}:{method.Name}");
            return method;
        }

        [PatchPostfix]
        public static async void PatchPostfix(
            BaseLocalGame<GamePlayerOwner> __instance
            , Task __result
            )
        {
            await __result;

            //LocalGamePatches.LocalGameInstance = __instance;
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                Logger.LogError("GameWorld is NULL");
                return;
            }
            var coopGameComponent = CoopGameComponent.GetCoopGameComponent();
            if (coopGameComponent != null)
            {
                GameObject.Destroy(coopGameComponent);
            }

            // Hideout is SinglePlayer only. Do not create CoopGameComponent
            if (__instance.GetType().Name.Contains("HideoutGame"))
                return;

            if (CoopPatches.CoopGameComponentParent == null)
                CoopPatches.CoopGameComponentParent = new GameObject("CoopGameComponentParent");

            coopGameComponent = CoopPatches.CoopGameComponentParent.GetOrAddComponent<CoopGameComponent>();
            coopGameComponent.LocalGameInstance = __instance;   

            //coopGameComponent = gameWorld.GetOrAddComponent<CoopGameComponent>();
            if (!string.IsNullOrEmpty(MatchmakerAcceptPatches.GetGroupId()))
                coopGameComponent.ServerId = MatchmakerAcceptPatches.GetGroupId();
            else
            {
                GameObject.Destroy(coopGameComponent);
                coopGameComponent = null;
                Logger.LogError("========== ERROR = COOP ========================");
                Logger.LogError("No Server Id found, Deleting Coop Game Component");
                Logger.LogError("================================================");
            }

            if (!MatchmakerAcceptPatches.IsClient)
            {
                Dictionary<string, object> packet = new Dictionary<string, object>
                {
                    { "m", "timeAndWeather" },
                    { "t", DateTime.Now.Ticks },
                    { "ct", TimeAndWeather.CloudinessType },
                    { "ft", TimeAndWeather.FogType },
                    { "hod", TimeAndWeather.HourOfDay },
                    { "rt", TimeAndWeather.RainType },
                    { "tft", TimeAndWeather.TimeFlowType },
                    { "wt", TimeAndWeather.WindType },
                    { "serverId", CoopGameComponent.GetServerId() }
                };
                Request.Instance.PostJson("/coop/server/update", packet.ToJson(), debug: true);
            }

            CoopPatches.EnableDisablePatches();

        }

        private static void EchoGameServer_OnLog(string text)
        {
            Logger.LogInfo(text);
        }

    }
}
