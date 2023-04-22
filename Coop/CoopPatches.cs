using BepInEx.Logging;
using Comfort.Common;
using EFT;
using SIT.Coop.Core.LocalGame;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Player;
using SIT.Core.Coop.Sounds;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace SIT.Core.Coop
{
    internal class CoopPatches
    {
        public static ManualLogSource Logger { get; private set; }

        private static BepInEx.Configuration.ConfigFile m_Config;

        public static void Run(BepInEx.Configuration.ConfigFile config)
        {
            m_Config = config;

            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("Coop");

            var enabled = config.Bind<bool>("Coop", "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all Coop stuff.
            {
                Logger.LogInfo("Coop has been disabled! Ignoring Patches.");
                return;
            }

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            new LocalGameStartingPatch(m_Config).Enable();
            new LocalGameEndingPatch(m_Config).Enable();
            new LocalGameSpawnAICoroutinePatch().Enable();
            new NonWaveSpawnScenarioPatch(m_Config).Enable();
            new WaveSpawnScenarioPatch(m_Config).Enable();

            // ------ MATCHMAKER -------------------------
            MatchmakerAcceptPatches.Run();

        }

        private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (Singleton<GameWorld>.Instantiated)
                EnableDisablePatches();
        }

        private static List<ModulePatch> NoMRPPatches = new List<ModulePatch>();

        public static void EnableDisablePatches()
        {
            var enablePatches = true;

            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC == null)
            {
                Logger.LogDebug($"CoopPatches:CoopGameComponent is null, Patches wont be Applied");
                enablePatches = false;
            }

            if (coopGC != null && !coopGC.enabled)
            {
                Logger.LogDebug($"CoopPatches:CoopGameComponent is not enabled, Patches wont be Applied");
                enablePatches = false;
            }

            if (string.IsNullOrEmpty(CoopGameComponent.GetServerId()))
            {
                Logger.LogDebug($"CoopPatches:CoopGameComponent ServerId is not set, Patches wont be Applied");
                enablePatches = false;
            }

            // ------ PLAYER -------------------------
            if (!NoMRPPatches.Any())
            {
                NoMRPPatches.Add(new PlayerOnInitPatch(m_Config));
                NoMRPPatches.Add(new WeaponSoundPlayer_FireSonicSound_Patch());
            }

            //Logger.LogInfo($"{NoMRPPatches.Count()} Non-MR Patches found");
            foreach (var patch in NoMRPPatches)
            {
                if (enablePatches)
                    patch.Enable();
                else
                    patch.Disable();
            }

            var moduleReplicationPatches = Assembly.GetAssembly(typeof(ModuleReplicationPatch)).GetTypes().Where(x => x.GetInterface("IModuleReplicationPatch") != null);
            ////Logger.LogInfo($"{moduleReplicationPatches.Count()} Module Replication Patches found");
            foreach (var module in moduleReplicationPatches)
            {
                if (module.IsAbstract
                    || module == typeof(ModuleReplicationPatch)
                    || module.Name.Contains(typeof(ModuleReplicationPatch).Name)
                    )
                    continue;

                ModuleReplicationPatch mrp = null;
                if (!ModuleReplicationPatch.Patches.Any(x => x.GetType() == module))
                    mrp = (ModuleReplicationPatch)Activator.CreateInstance(module);
                else
                    mrp = ModuleReplicationPatch.Patches.SingleOrDefault(x => x.GetType() == module);

                if (mrp == null)
                    continue;

                if (!mrp.DisablePatch && enablePatches)
                {
                    //Logger.LogInfo($"Enabled {mrp.GetType()}");
                    mrp.Enable();
                }
                else
                    mrp.Disable();
            }
        }
    }
}
