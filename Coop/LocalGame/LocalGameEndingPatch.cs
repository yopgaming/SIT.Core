using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.EnvironmentEffect;
using EFT.UI;
using EFT.UI.Screens;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Coop.Core.LocalGame
{
    internal class LocalGameEndingPatch : ModulePatch
    {
        private static ConfigFile _config;


        public LocalGameEndingPatch(ConfigFile config)
        {
            _config = config;
        }

        protected override MethodBase GetTargetMethod()
        {
            var t = PatchConstants.EftTypes.FirstOrDefault(x => x.FullName.StartsWith("EFT.LocalGame"));
            if (t == null)
                Logger.LogInfo($"LocalGameEndingPatch:Type is NULL");

            //var method = ReflectionHelpers.GetAllMethodsForType(t)
            //    .FirstOrDefault(x => x.GetParameters().Length >= 4
            //    && x.GetParameters().Any(x => x.Name.Contains("profileId"))
            //    && x.GetParameters().Any(x => x.Name.Contains("exitStatus"))
            //    && x.GetParameters().Any(x => x.Name.Contains("exitName"))
            //    && x.GetParameters().Any(x => x.Name.Contains("delay"))
            //    );

            var method = ReflectionHelpers.GetMethodForType(t, "Stop", false, true);

            Logger.LogInfo($"LocalGameEndingPatch:{t.Name}:{method.Name}");
            return method;
        }

        [PatchPrefix]
        public static bool PatchPrefix(
            EFT.LocalGame __instance
            )
        {
            // If Host. Run as normal.
            return !Matchmaker.MatchmakerAcceptPatches.IsClient;
        }

        [PatchPostfix]
        public static void Postfix(
            EFT.LocalGame __instance
            , string profileId, ExitStatus exitStatus, string exitName, float delay
            )
        {
            if (Matchmaker.MatchmakerAcceptPatches.IsClient)
            {
                EndSession(__instance, profileId, exitStatus, exitName, delay);
            }
        }

        /// <summary>
        /// Ends the session for the provided ProfileId in the Instanced Game
        /// </summary>
        /// <param name="game"></param>
        /// <param name="profileId"></param>
        /// <param name="exitStatus"></param>
        /// <param name="exitName"></param>
        /// <param name="delay"></param>
        public static void EndSession(
            EFT.LocalGame game
            , string profileId
            , ExitStatus exitStatus
            , string exitName
            , float delay)
        {
            var gameStatus = ReflectionHelpers.GetFieldOrPropertyFromInstance
                                   <GameStatus>(game, "Status", false);
            if (game.Status == GameStatus.Starting || game.Status == GameStatus.Started)
            {
                //__instance.endByTimerScenario_0.GameStatus_0 = GameStatus.SoftStopping;
                var endByTimerScenario_0 = ReflectionHelpers.GetFieldOrPropertyFromInstance
                                <EndByTimerScenario>(game, "endByTimerScenario_0", false);
                if (endByTimerScenario_0 != null)
                {
                    var endByTimerScenarioGameStatus = ReflectionHelpers.GetFieldOrPropertyFromInstance
                                    <GameStatus>(endByTimerScenario_0, "GameStatus_0", false);
                    endByTimerScenarioGameStatus = GameStatus.SoftStopping;
                }

            }
            gameStatus = GameStatus.Stopping;
            game.GameTimer.TryStop();
            if (EnvironmentManager.Instance != null)
            {
                EnvironmentManager.Instance.Stop();
            }
            MonoBehaviourSingleton<PreloaderUI>.Instance.StartBlackScreenShow(1f, 1f, delegate
            {
                // Get Coop Game Component to find profileid player
                var coopGC = Singleton<GameWorld>.Instance.GetComponent<CoopGameComponent>();
                if(coopGC == null)
                {
                    Logger.LogError("Couldn't find Coop Game Component");
                    return;
                }

                // ----------------------------------------------------------------------------------------------------------
                // TODO: Hard coded to only work with Client Owner Player. Probably should change this!
                EFT.LocalPlayer myPlayer = null;
                if (coopGC.Players.Any(x => x.Value.IsYourPlayer))
                    myPlayer = coopGC.Players.First(x => x.Value.IsYourPlayer).Value as EFT.LocalPlayer;
                else if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x=>x.IsYourPlayer))
                    myPlayer = Singleton<GameWorld>.Instance.RegisteredPlayers.First(x => x.IsYourPlayer) as EFT.LocalPlayer;
                //
                // ----------------------------------------------------------------------------------------------------------

                if (myPlayer == null)
                {
                    Logger.LogError("Couldn't find Own Player");
                    return;
                }
                myPlayer.OnGameSessionEnd(exitStatus, game.PastTime, game.LocationObjectId, exitName);

                game.CleanUp();
                gameStatus = GameStatus.Stopped;
                ReflectionHelpers.SetFieldOrPropertyFromInstance(game, "GameStatus", gameStatus);
                PatchConstants.BackEndSession.OfflineRaidEnded(exitStatus, exitName, 0);
                StaticManager.Instance.WaitSeconds(delay, delegate
                {
                    var callback_0 = ReflectionHelpers.GetFieldOrPropertyFromInstance
                                <Callback<ExitStatus, TimeSpan, Metrics>>(game, "callback_0", false);
                    if (callback_0 != null)
                    {
                        callback_0(new Result<ExitStatus, TimeSpan, Metrics>(exitStatus, new TimeSpan(), new Metrics()));
                    }
                    UIEventSystem.Instance.Enable();
                });
            });
        }
    }
}
