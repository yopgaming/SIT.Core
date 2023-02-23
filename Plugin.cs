using BepInEx;
using Comfort.Common;
using EFT;
using Microsoft.Win32;
using SIT.Tarkov.Core.Hideout;
using SIT.Tarkov.Core.Menus;
using SIT.Tarkov.Core.PlayerPatches;
using SIT.Tarkov.Core.SP;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.AI;
using SIT.Tarkov.Core.Bundles;
using SIT.Tarkov.Core.PlayerPatches.Health;
using SIT.Tarkov.Core.SP.ScavMode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using SIT.Core.AkiSupport;
using SIT.Core.Misc;
using SIT.Core.AkiSupport.Custom;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.LocalGame;
using SIT.Coop.Core.Matchmaker.MatchmakerAccept.Grouping;
using SIT.Coop.Core.Matchmaker.MatchmakerAccept;
using SIT.Core.Coop;
using static GClass1643;
using SIT.Core.Menus;
using SIT.Core.AkiSupport.Airdrops;

namespace SIT.Core
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    //[BepInDependency()] // Should probably be dependant on Aki right?
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;

        private void Awake()
        {
            //PatchConstants.GetBackendUrl();

            EnableCorePatches();
            EnableSPPatches();
            EnableCoopPatches();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            Instance = this;
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

            //// --------- On Dead -----------------------
            new OnDeadPatch(Config).Enable();

            //// --------- Player Init -------------------
            new PlayerInitPatch().Enable();
            new ChangeHealthPatch().Enable();
            new ChangeHydrationPatch().Enable();
            new ChangeEnergyPatch().Enable();

            //// --------- SCAV MODE ---------------------
            new DisableScavModePatch().Enable();

            //// --------- Airdrop -----------------------
            new AirdropPatch().Enable();

            //// --------- AI -----------------------
            new BotSelfEnemyPatch().Enable();
            new IsPlayerEnemyByRolePatch().Enable();

            //// --------- Matchmaker ----------------
            new AutoSetOfflineMatch().Enable();
            new DisableReadyButtonOnFirstScreen().Enable();

            //// -------------------------------------
            //// Progression
            new OfflineSaveProfile().Enable();
            new ExperienceGainFix().Enable();

          

            //// -------------------------------------
            //// Quests
            //new ItemDroppedAtPlace_Beacon().Enable();

            //// --------------------------------------
            // Bots
            new AddSptBotSettingsPatch().Enable();
            new RemoveUsedBotProfilePatch().Enable();

            //new IsBossOrFollowerFixPatch().Enable();

            new VersionLabelPatch().Enable();

            new QTEPatch().Enable();
            new TinnitusFixPatch().Enable();


            new InsuranceScreenPatch().Enable();

            try
            {
                BundleManager.GetBundles();
                //new EasyAssetsPatch().Enable();
                //new EasyBundlePatch().Enable();
            }
            catch(Exception ex)
            {
                Logger.LogError("// --- ERROR -----------------------------------------------");
                Logger.LogError("Bundle System Failed!!");
                Logger.LogError(ex.ToString());
                Logger.LogError("// --- ERROR -----------------------------------------------");

            }

        }

        private void EnableCorePatches()
        {
            var enabled = Config.Bind<bool>("SIT Core Patches", "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all SIT Core Patches.
            {
                Logger.LogInfo("SIT Core Patches has been disabled! Ignoring Patches.");
                return;
            }

            new ConsistencySinglePatch().Enable();
            new ConsistencyMultiPatch().Enable();
            new BattlEyePatch().Enable();
            new SslCertificatePatch().Enable();
            new UnityWebRequestPatch().Enable();
            new TransportPrefixPatch().Enable();
            new WebSocketPatch().Enable();
        }

        private void EnableCoopPatches()
        {
            //new LocalGameStartingPatch(Config).Enable();
            //new LocalGameBotWaveSystemPatch().Enable();
            //new MatchmakerAcceptScreenAwakePatch().Enable();
            //new MatchmakerAcceptScreenShowPatch().Enable();
            //new SendInvitePatch().Enable();
            //new AcceptInvitePatch().Enable();
            CoopPatches.Run(Config);
        }

        //private void SceneManager_sceneUnloaded(Scene arg0)
        //{

        //}

        public static GameWorld gameWorld { get; private set; }


        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            GetPoolManager();
            GetBackendConfigurationInstance();

            gameWorld = Singleton<GameWorld>.Instance;

            //EnableCoopPatches();

        }

        //public void SetupMoreGraphicsMenuOptions()
        //{
        //    Logger.LogInfo("Adjusting sliders for Overall Visibility and LOD Quality");
        //    var TypeOfGraphicsSettingsTab = typeof(EFT.UI.Settings.GraphicsSettingsTab);

        //    var readOnlyCollection_0 = TypeOfGraphicsSettingsTab.GetField(
        //        "readOnlyCollection_0",
        //        BindingFlags.Static |
        //        BindingFlags.NonPublic
        //        );

        //    var readOnlyCollection_3 = TypeOfGraphicsSettingsTab.GetField(
        //        "readOnlyCollection_3",
        //        BindingFlags.Static |
        //        BindingFlags.NonPublic
        //        );

        //    List<float> overallVisibility = new();
        //    for (int i = 0; i <= 11; i++)
        //    {
        //        overallVisibility.Add(400 + (i * 50));
        //    }

        //    for (int i = 0; i <= 4; i++)
        //    {
        //        overallVisibility.Add(1000 + (i * 500));
        //    }


        //    List<float> lodQuality = new();
        //    for (int i = 0; i <= 9; i++)
        //    {
        //        lodQuality.Add((float)(2 + (i * 0.25)));
        //    }

        //    var Collection_0 = Array.AsReadOnly<float>(overallVisibility.ToArray());
        //    var Collection_3 = Array.AsReadOnly<float>(lodQuality.ToArray());

        //    readOnlyCollection_0.SetValue(null, Collection_0);
        //    readOnlyCollection_3.SetValue(null, Collection_3);
        //    Logger.LogInfo("Adjusted sliders for Overall Visibility and LOD Quality");
        //}

        private void GetBackendConfigurationInstance()
        {
            if (
                PatchConstants.BackendStaticConfigurationType != null &&
                PatchConstants.BackendStaticConfigurationConfigInstance == null)
            {
                PatchConstants.BackendStaticConfigurationConfigInstance = PatchConstants.GetPropertyFromType(PatchConstants.BackendStaticConfigurationType, "Config").GetValue(null);
                //Logger.LogInfo($"BackendStaticConfigurationConfigInstance Type:{ PatchConstants.BackendStaticConfigurationConfigInstance.GetType().Name }");
            }

            if (PatchConstants.BackendStaticConfigurationConfigInstance != null
                && PatchConstants.CharacterControllerSettings.CharacterControllerInstance == null
                )
            {
                PatchConstants.CharacterControllerSettings.CharacterControllerInstance
                    = PatchConstants.GetFieldOrPropertyFromInstance<object>(PatchConstants.BackendStaticConfigurationConfigInstance, "CharacterController", false);
                Logger.LogInfo($"PatchConstants.CharacterControllerInstance Type:{PatchConstants.CharacterControllerSettings.CharacterControllerInstance.GetType().Name}");
            }

            if (PatchConstants.CharacterControllerSettings.CharacterControllerInstance != null
                && PatchConstants.CharacterControllerSettings.ClientPlayerMode == null
                )
            {
                PatchConstants.CharacterControllerSettings.ClientPlayerMode
                    = PatchConstants.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ClientPlayerMode", false);

                PatchConstants.CharacterControllerSettings.ObservedPlayerMode
                    = PatchConstants.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "ObservedPlayerMode", false);

                PatchConstants.CharacterControllerSettings.BotPlayerMode
                    = PatchConstants.GetFieldOrPropertyFromInstance<CharacterControllerSpawner.Mode>(PatchConstants.CharacterControllerSettings.CharacterControllerInstance, "BotPlayerMode", false);
            }

        }



        private void GetPoolManager()
        {
            if (PatchConstants.PoolManagerType == null)
            {
                PatchConstants.PoolManagerType = PatchConstants.EftTypes.Single(x => PatchConstants.GetAllMethodsForType(x).Any(x => x.Name == "LoadBundlesAndCreatePools"));
                //Logger.LogInfo($"Loading PoolManagerType:{ PatchConstants.PoolManagerType.FullName}");

                //Logger.LogInfo($"Getting PoolManager Instance");
                Type generic = typeof(Comfort.Common.Singleton<>);
                Type[] typeArgs = { PatchConstants.PoolManagerType };
                ConstructedBundleAndPoolManagerSingletonType = generic.MakeGenericType(typeArgs);
                //Logger.LogInfo(PatchConstants.PoolManagerType.FullName);
                //Logger.LogInfo(ConstructedBundleAndPoolManagerSingletonType.FullName);

                //new LoadBotTemplatesPatch().Enable();
                //new RemoveUsedBotProfile().Enable();
                //new CreateFriendlyAIPatch().Enable();
            }
        }

        private Type ConstructedBundleAndPoolManagerSingletonType { get; set; }
        public static object BundleAndPoolManager { get; set; }

        public static Type poolsCategoryType { get; set; }
        public static Type assemblyTypeType { get; set; }

        public static MethodInfo LoadBundlesAndCreatePoolsMethod { get; set; }

        public static async void LoadBundlesAndCreatePoolsAsync(ResourceKey[] resources)
        {
            try
            {
                if (BundleAndPoolManager == null)
                {
                    PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: BundleAndPoolManager is missing");
                    return;
                }

                await Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(
                    PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, resources, JobPriority.General, null, CancellationToken.None);

            }
            catch (Exception ex)
            {
                PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools -- ERROR ->>>");
                PatchConstants.Logger.LogInfo(ex.ToString());
            }
        }

        public static Task LoadBundlesAndCreatePools(ResourceKey[] resources)
        {
            try
            {
                if (BundleAndPoolManager == null)
                {
                    PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: BundleAndPoolManager is missing");
                    return null;
                }

                var raidE = Enum.Parse(poolsCategoryType, "Raid");
                //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: raidE is " + raidE.ToString());

                var localE = Enum.Parse(assemblyTypeType, "Local");
                //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: localE is " + localE.ToString());

                var GenProp = PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null);
                //PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: GenProp is " + GenProp.ToString());


                return PatchConstants.InvokeAsyncStaticByReflection(
                    LoadBundlesAndCreatePoolsMethod,
                    BundleAndPoolManager
                    , raidE
                    , localE
                    , resources
                    , GenProp
                    , (object o) => { PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: Progressing!"); }
                    , default(CancellationToken)
                    );

                //Task task = LoadBundlesAndCreatePoolsMethod.Invoke(BundleAndPoolManager,
                //    new object[] {
                //    Enum.Parse(poolsCategoryType, "Raid")
                //    , Enum.Parse(assemblyTypeType, "Local")
                //    , resources
                //    , PatchConstants.GetPropertyFromType(PatchConstants.JobPriorityType, "General").GetValue(null, null)
                //    , null
                //    , default(CancellationToken)
                //    }
                //    ) as Task;
                ////PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: task is " + task.GetType());

                //if (task != null) // && task.GetType() == typeof(Task))
                //{
                //    task.ContinueWith(t => { PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools loaded"); });
                //    //var t = task as Task;
                //    PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools: task is " + task.GetType());
                //    return task;
                //}
            }
            catch (Exception ex)
            {
                PatchConstants.Logger.LogInfo("LoadBundlesAndCreatePools -- ERROR ->>>");
                PatchConstants.Logger.LogInfo(ex.ToString());
            }
            return null;
        }

    }
}
