using Comfort.Common;
using EFT;
using EFT.InputSystem;
using EFT.UI;
using EFT.UI.Matchmaker;
using HarmonyLib;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.UIElements.StyleVariableResolver;

namespace SIT.Core.Coop
{
    /// <summary>
    /// Overwrite and use our own CoopGame instance instead
    /// </summary>
    internal class TarkovApplication_LocalGameCreator_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetAllMethodsForType(typeof(TarkovApplication)).Single(
                x =>

                x.GetParameters().Length >= 2
                && x.GetParameters()[0].ParameterType == typeof(TimeAndWeatherSettings)
                && x.GetParameters()[1].ParameterType == typeof(MatchmakerTimeHasCome.TimeHasComeScreenController)
                );
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            Logger.LogDebug("TarkovApplication_LocalGameCreator_Patch:Prefix");

            return false;
        }

        [PatchPostfix]
        public static async Task Postfix(
            Task __result,
           TarkovApplication __instance,
           TimeAndWeatherSettings timeAndWeather,
           MatchmakerTimeHasCome.TimeHasComeScreenController timeHasComeScreenController,
            RaidSettings ____raidSettings,
            InputTree ____inputTree,
            GameDateTime ____localGameDateTime,
            float ____fixedDeltaTime,
            string ____backendUrl
            )
        {
            Logger.LogDebug("TarkovApplication_LocalGameCreator_Patch:Postfix");
            if (Singleton<NotificationManagerClass>.Instantiated)
            {
                Singleton<NotificationManagerClass>.Instance.Deactivate();
            }

            var session = __instance.GetClientBackEndSession();
            session.Profile.Inventory.Stash = null;
            session.Profile.Inventory.QuestStashItems = null;
            session.Profile.Inventory.DiscardLimits = Singleton<ItemFactory>.Instance.GetDiscardLimits();
            await session.SendRaidSettings(____raidSettings);
            timeHasComeScreenController.ChangeStatus("Creating Coop Game");
            await Task.Delay(1000);
            CoopGame localGame = CoopGame.Create(____inputTree
                , session.Profile
                , ____localGameDateTime
                , session.InsuranceCompany
                , MonoBehaviourSingleton<MenuUI>.Instance
                , MonoBehaviourSingleton<CommonUI>.Instance
                , MonoBehaviourSingleton<PreloaderUI>.Instance
                , MonoBehaviourSingleton<GameUI>.Instance
                , ____raidSettings.SelectedLocation
                , timeAndWeather
                , ____raidSettings.WavesSettings
                , ____raidSettings.SelectedDateTime
                , new Callback<ExitStatus, TimeSpan, ClientMetrics>((r) => {

                    Logger.LogInfo("Callback Metrics. Invoke method 45");
                    ReflectionHelpers.GetMethodForType(__instance.GetType(), "method_45").Invoke(__instance, new object[] {
                    session.Profile.Id, session.ProfileOfPet, ____raidSettings.SelectedLocation, r, timeHasComeScreenController
                    });
                })
                , ____fixedDeltaTime
                , EUpdateQueue.Update
                , session
                , TimeSpan.FromSeconds((double)(60 * ____raidSettings.SelectedLocation.EscapeTimeLimit)));
            Singleton<AbstractGame>.Create(localGame);
                await localGame.method_3(____raidSettings.BotSettings, ____backendUrl, null, new Callback((r) =>
                //await localGame.CreatePlayerToStartMatch(____raidSettings.BotSettings, ____backendUrl, null, new Callback((r) =>
                {

                    //using (GClass21.StartWithToken("LoadingScreen.LoadComplete"))
                    //{
                        UnityEngine.Object.DestroyImmediate(MonoBehaviourSingleton<MenuUI>.Instance.gameObject);
                        MainMenuController mmc =
                            (MainMenuController)ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(TarkovApplication), typeof(MainMenuController)).GetValue(__instance);
                        mmc.Unsubscribe();
                        Singleton<GameWorld>.Instance.OnGameStarted();
                    //}

                }));

            //__result = Task.Run(() => { });
            __result = Task.CompletedTask;
        }
    }
}
