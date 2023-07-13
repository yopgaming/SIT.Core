using Aki.Custom.Patches;
using BepInEx;
using BepInEx.Bootstrap;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.UI;
using SIT.Core.AkiSupport.Airdrops;
using SIT.Core.AkiSupport.Custom;
using SIT.Core.AkiSupport.Singleplayer;
using SIT.Core.AkiSupport.SITFixes;
using SIT.Core.Configuration;
using SIT.Core.Coop;
using SIT.Core.Coop.AI;
using SIT.Core.Core;
using SIT.Core.Core.FileChecker;
using SIT.Core.Core.Web;
using SIT.Core.Misc;
using SIT.Core.Other;
using SIT.Core.SP.Menus;
using SIT.Core.SP.PlayerPatches;
using SIT.Core.SP.PlayerPatches.Health;
using SIT.Core.SP.Raid;
using SIT.Core.SP.ScavMode;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SIT.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public static PluginConfigSettings Settings { get; private set; }

        private bool ShownDependancyError { get; set; }
        public static string EFTVersionMajor { get; internal set; }

        private void Awake()
        {
            Instance = this;
            Settings = new PluginConfigSettings(Logger, Config);
            LogDependancyErrors();
            // Gather the Major/Minor numbers of EFT ASAP
            new VersionLabelPatch(Config).Enable();
            StartCoroutine(VersionChecks());

            EnableCorePatches();
            EnableSPPatches();
            EnableCoopPatches();
            OtherPatches.Run(Config, this);

            Logger.LogInfo($"Stay in Tarkov is loaded!");
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private IEnumerator VersionChecks()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);

                if (!string.IsNullOrEmpty(EFTVersionMajor))
                {
                    Logger.LogInfo("Version Check: Detected:" + EFTVersionMajor);
                    if (EFTVersionMajor.Split('.').Length > 4)
                    {
                        var majorN1 = EFTVersionMajor.Split('.')[0]; // 0
                        var majorN2 = EFTVersionMajor.Split('.')[1]; // 13
                        var majorN3 = EFTVersionMajor.Split('.')[2]; // 1
                        var majorN4 = EFTVersionMajor.Split('.')[3]; // 1
                        var majorN5 = EFTVersionMajor.Split('.')[4]; // build number

                        if (majorN1 != "0" || majorN2 != "13" || majorN3 != "1" || majorN4 != "1")
                        {
                            Logger.LogError("Version Check: This version of SIT is not designed to work with this version of EFT.");
                        }
                        else
                        {
                            Logger.LogInfo("Version Check: OK.");
                        }
                    }

                    yield break;
                }
            }
        }

        private void EnableCorePatches()
        {
            // SIT Legal Game Checker
            LegalGameCheck.LegalityCheck();

            var enabled = Config.Bind<bool>("SIT Core Patches", "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all SIT Core Patches.
            {
                Logger.LogInfo("SIT Core Patches has been disabled! Ignoring Patches.");
                return;
            }

            // File Checker
            new ConsistencySinglePatch().Enable();
            new ConsistencyMultiPatch().Enable();
            new RunFilesCheckingPatch().Enable();
            // BattlEye
            new BattlEyePatch().Enable();
            new BattlEyePatchFirstPassRun().Enable();
            new BattlEyePatchFirstPassUpdate().Enable();
            // Web Requests
            new SslCertificatePatch().Enable();
            new UnityWebRequestPatch().Enable();
            new TransportPrefixPatch().Enable();
            new WebSocketPatch().Enable();
            //new TarkovTransportWSInstanceHookPatch().Enable();
            //new TarkovTransportHttpInstanceHookPatch().Enable();
            new SendCommandsPatch().Enable();
        }

        private void EnableSPPatches()
        {
            var enabled = Config.Bind<bool>("SIT.SP", "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all SIT SP Patches.
            {
                Logger.LogInfo("SIT SP Patches has been disabled! Ignoring Patches.");
                return;
            }

            //// --------- PMC Dogtags -------------------
            new UpdateDogtagPatch().Enable();

            //// --------- Player Init & Health -------------------
            EnableSPPatches_PlayerHealth(Config);

            //// --------- SCAV MODE ---------------------
            new DisableScavModePatch().Enable();

            //// --------- Airdrop -----------------------
            new AirdropPatch().Enable();

            //// --------- Screens ----------------
            EnableSPPatches_Screens(Config);

            //// --------- Progression -----------------------
            EnableSPPatches_PlayerProgression();

            //// --------------------------------------
            // Bots
            EnableSPPatches_Bots(Config);

            new QTEPatch().Enable();
            new TinnitusFixPatch().Enable();

            try
            {
                BundleManager.GetBundles();
                new EasyAssetsPatch().Enable();
                new EasyBundlePatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError("// --- ERROR -----------------------------------------------");
                Logger.LogError("Bundle System Failed!!");
                Logger.LogError(ex.ToString());
                Logger.LogError("// --- ERROR -----------------------------------------------");

            }

            new WavesSpawnScenarioInitPatch(Config).Enable();
            new WavesSpawnScenarioMethodPatch().Enable();
        }

        private static void EnableSPPatches_Screens(BepInEx.Configuration.ConfigFile config)
        {
            //new OfflineRaidMenuPatch().Enable();
            new OfflineSettingsScreenPatch().Enable();
            new InsuranceScreenPatch().Enable();
            new MatchmakerLocationScreen_DisableReadyButton_Patch().Enable();

            //try
            //{
            //    new MatchmakerLocationScreen_DisableLevelLock_Patch().Enable();
            //}
            //catch(Exception ex) { Plugin.Instance.Logger.LogError(ex.Message); }

            new LighthouseBridgePatch().Enable();
            new LighthouseTransmitterPatch().Enable();
            new PostRaidHealScreenPatch().Enable();
        }

        private static void EnableSPPatches_PlayerProgression()
        {
            new OfflineSaveProfile().Enable();
            new ExperienceGainFix().Enable();
        }

        private void EnableSPPatches_PlayerHealth(BepInEx.Configuration.ConfigFile config)
        {
            var enabled = config.Bind<bool>("SIT.SP", "EnableHealthPatches", true);
            if (!enabled.Value)
                return;

            new Player_Init_SP_Patch().Enable();
            new ChangeHealthPatch().Enable();
            new ChangeHydrationPatch().Enable();
            new ChangeEnergyPatch().Enable();
            new OnDeadPatch(Config).Enable();
            new MainMenuControllerForHealthListenerPatch().Enable();
        }

        private static void EnableSPPatches_Bots(BepInEx.Configuration.ConfigFile config)
        {
            new CoreDifficultyPatch().Enable();
            new BotDifficultyPatch().Enable();
            new GetNewBotTemplatesPatch().Enable();
            new BotSettingsRepoClassIsFollowerFixPatch().Enable();
            new IsPlayerEnemyPatch().Enable();
            new IsPlayerEnemyByRolePatch().Enable();

            var enabled = config.Bind<bool>("SIT.SP", "EnableBotPatches", true);
            if (!enabled.Value)
                return;

            new AddEnemyToAllGroupsInBotZonePatch().Enable();
            new CheckAndAddEnemyPatch().Enable();

        }

        private void EnableCoopPatches()
        {
            CoopPatches.Run(Config);
        }

        public static GameWorld gameWorld { get; private set; }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //GetPoolManager();
            GetBackendConfigurationInstance();

            if (Singleton<GameWorld>.Instantiated)
                gameWorld = Singleton<GameWorld>.Instance;
        }

        private void GetBackendConfigurationInstance()
        {
            if (
                PatchConstants.BackendStaticConfigurationType != null &&
                PatchConstants.BackendStaticConfigurationConfigInstance == null)
            {
                PatchConstants.BackendStaticConfigurationConfigInstance = ReflectionHelpers.GetPropertyFromType(PatchConstants.BackendStaticConfigurationType, "Config").GetValue(null);
                //Logger.LogInfo($"BackendStaticConfigurationConfigInstance Type:{ PatchConstants.BackendStaticConfigurationConfigInstance.GetType().Name }");
            }

            if (PatchConstants.BackendStaticConfigurationConfigInstance != null
                && PatchConstants.CharacterControllerSettings.CharacterControllerInstance == null
                )
            {
                PatchConstants.CharacterControllerSettings.CharacterControllerInstance
                    = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(PatchConstants.BackendStaticConfigurationConfigInstance, "CharacterController", false);
                //Logger.LogInfo($"PatchConstants.CharacterControllerInstance Type:{PatchConstants.CharacterControllerSettings.CharacterControllerInstance.GetType().Name}");
            }

            if (PatchConstants.CharacterControllerSettings.CharacterControllerInstance != null
                && PatchConstants.CharacterControllerSettings.ClientPlayerMode == null
                )
            {
                PatchConstants.CharacterControllerSettings.ClientPlayerMode
                    = ReflectionHelpers.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ClientPlayerMode", false);

                PatchConstants.CharacterControllerSettings.ObservedPlayerMode
                    = ReflectionHelpers.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ObservedPlayerMode", false);

                PatchConstants.CharacterControllerSettings.BotPlayerMode
                    = ReflectionHelpers.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "BotPlayerMode", false);
            }

        }


        private void LogDependancyErrors()
        {
            // Skip if we've already shown the message, or there are no errors
            if (ShownDependancyError || Chainloader.DependencyErrors.Count == 0)
            {
                return;
            }

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("Errors occurred during plugin loading");
            stringBuilder.AppendLine("-------------------------------------");
            stringBuilder.AppendLine();
            foreach (string error in Chainloader.DependencyErrors)
            {
                stringBuilder.AppendLine(error);
                stringBuilder.AppendLine();
            }
            string errorMessage = stringBuilder.ToString();

            DisplayMessageNotifications.DisplayMessageNotification($"{errorMessage}", ENotificationDurationType.Infinite, ENotificationIconType.Alert, UnityEngine.Color.red);

            // Show an error in the BepInEx console/log file
            Logger.LogError(errorMessage);

            // Show an error in the in-game console, we have to write this in reverse order because of the nature of the console output
            foreach (string line in errorMessage.Split('\n').Reverse())
            {
                if (line.Trim().Length > 0)
                {
                    ConsoleScreen.LogError(line);
                }
            }

            ShownDependancyError = true;


        }

        //private void GetPoolManager()
        //{
        //    if (PatchConstants.PoolManagerType == null)
        //    {
        //        PatchConstants.PoolManagerType = PatchConstants.EftTypes.Single(x => ReflectionHelpers.GetAllMethodsForType(x).Any(x => x.Name == "LoadBundlesAndCreatePools"));
        //        Type generic = typeof(Singleton<>);
        //        Type[] typeArgs = { PatchConstants.PoolManagerType };
        //        ConstructedBundleAndPoolManagerSingletonType = generic.MakeGenericType(typeArgs);
        //    }
        //}

        //private Type ConstructedBundleAndPoolManagerSingletonType { get; set; }
        //public static object BundleAndPoolManager { get; set; }

        //public static Type poolsCategoryType { get; set; }
        //public static Type assemblyTypeType { get; set; }

        //public static MethodInfo LoadBundlesAndCreatePoolsMethod { get; set; }

        //    public static Task LoadBundlesAndCreatePools(ResourceKey[] resources)
        //    {
        //        try
        //        {
        //            if (BundleAndPoolManager == null)
        //            {
        //                PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: BundleAndPoolManager is missing");
        //                return null;
        //            }

        //            var raidE = Enum.Parse(poolsCategoryType, "Raid");
        //            //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: raidE is " + raidE.ToString());

        //            var localE = Enum.Parse(assemblyTypeType, "Local");
        //            //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: localE is " + localE.ToString());

        //            var GenProp = ReflectionHelpers.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null);
        //            //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: GenProp is " + GenProp.ToString());


        //            return PatchConstants.InvokeAsyncStaticByReflection(
        //                LoadBundlesAndCreatePoolsMethod,
        //                BundleAndPoolManager
        //                , raidE
        //                , localE
        //                , resources
        //                , GenProp
        //                , (object o) => { PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: Progressing!"); }
        //                , default(CancellationToken)
        //                );
        //        }
        //        catch (Exception ex)
        //        {
        //            PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools -- ERROR ->>>");
        //            PatchConstants.Logger.LogInfo(ex.ToString());
        //        }
        //        return null;
        //    }

    }
}
