using Aki.Custom.Patches;
using BepInEx;
using Comfort.Common;
using EFT;
using SIT.Core.AkiSupport.Airdrops;
using SIT.Core.AkiSupport.Custom;
using SIT.Core.AkiSupport.Singleplayer;
using SIT.Core.AkiSupport.SITFixes;
using SIT.Core.Coop;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Core.Other;
using SIT.Core.SP.Menus;
using SIT.Core.SP.PlayerPatches;
using SIT.Core.SP.PlayerPatches.Health;
using SIT.Core.SP.Raid;
using SIT.Core.SP.ScavMode;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace SIT.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    //[BepInDependency("com.spt-aki.core")] // Should probably be dependant on Aki right?
    [BepInProcess("EscapeFromTarkov.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;

        private void Awake()
        {
            Instance = this;

            EnableCorePatches();
            EnableSPPatches();
            EnableCoopPatches();
            OtherPatches.Run(Config, this);

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void EnableCorePatches()
        {
            var enabled = Config.Bind<bool>("SIT Core Patches", "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all SIT Core Patches.
            {
                Logger.LogInfo("SIT Core Patches has been disabled! Ignoring Patches.");
                return;
            }

            LegalGameCheck.LegalityCheck();
            new ConsistencySinglePatch().Enable();
            new ConsistencyMultiPatch().Enable();
            new BattlEyePatch().Enable();
            new SslCertificatePatch().Enable();
            new UnityWebRequestPatch().Enable();
            new TransportPrefixPatch().Enable();
            new WebSocketPatch().Enable();
        }

        private void EnableSPPatches()
        {
            var enabled = Config.Bind<bool>("SIT SP Patches", "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all SIT SP Patches.
            {
                Logger.LogInfo("SIT SP Patches has been disabled! Ignoring Patches.");
                return;
            }

            //// --------- PMC Dogtags -------------------
            new UpdateDogtagPatch().Enable();

            //// --------- Player Init & Health -------------------
            EnableSPPatches_PlayerHealth();

            //// --------- SCAV MODE ---------------------
            new DisableScavModePatch().Enable();

            //// --------- Airdrop -----------------------
            new AirdropPatch().Enable();

            //// --------- Screens ----------------
            new OfflineRaidMenuPatch().Enable();
            new AutoSetOfflineMatch2().Enable();
            new InsuranceScreenPatch().Enable();
            new VersionLabelPatch().Enable();

            //// --------- Progression -----------------------
            EnableSPPatches_PlayerProgression();

            //// --------------------------------------
            // Bots
            EnableSPPatches_Bots();

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

            new WavesSpawnScenarioInitPatch().Enable();
            new WavesSpawnScenarioMethodPatch().Enable();
        }

        private static void EnableSPPatches_PlayerProgression()
        {
            new OfflineSaveProfile().Enable();
            new ExperienceGainFix().Enable();
        }

        private void EnableSPPatches_PlayerHealth()
        {
            new PlayerInitPatch().Enable();
            new ChangeHealthPatch().Enable();
            new ChangeHydrationPatch().Enable();
            new ChangeEnergyPatch().Enable();
            new OnDeadPatch(Config).Enable();
        }

        private static void EnableSPPatches_Bots()
        {
            new BotDifficultyPatch().Enable();
            new GetNewBotTemplatesPatch().Enable();
            new BotSettingsRepoClassIsFollowerFixPatch().Enable();
            //new BotEnemyTargetPatch().Enable();
            new BotSelfEnemyPatch().Enable();
            new AkiSupport.Singleplayer.RemoveUsedBotProfilePatch().Enable();
            new AkiSupport.Custom.AddEnemyToAllGroupsInBotZonePatch().Enable();
        }

        private void EnableCoopPatches()
        {
            Logger.LogInfo("Enabling Coop Patches");
            CoopPatches.Run(Config);
        }

        public static GameWorld gameWorld { get; private set; }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            //GetPoolManager();
            GetBackendConfigurationInstance();

            if(Singleton<GameWorld>.Instantiated)
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
                Logger.LogInfo($"PatchConstants.CharacterControllerInstance Type:{PatchConstants.CharacterControllerSettings.CharacterControllerInstance.GetType().Name}");
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
