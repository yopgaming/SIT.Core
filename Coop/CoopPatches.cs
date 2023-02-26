using BepInEx.Logging;
using SIT.Coop.Core.LocalGame;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Player;
using System;
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
            //new LocalGamePlayerSpawn().Enable();

            // ------ MATCHMAKER -------------------------
            MatchmakerAcceptPatches.Run();



            // Tests
            //_ = new EFT.Player().SITToJson();


        }

        private static void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            EnableDisablePatches();
        }

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

            if (enablePatches)
            {
                new PlayerOnInitPatch(m_Config).Enable();
                //new PlayerOnMovePatch().Enable();
            }


            var moduleReplicationPatches = Assembly.GetAssembly(typeof(ModuleReplicationPatch)).GetTypes().Where(x => x.GetInterface("IModuleReplicationPatch") != null);
            //Logger.LogInfo($"{moduleReplicationPatches.Count()} Module Replication Patches found");
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
                    Logger.LogInfo($"Enabled {mrp.GetType()}");
                    mrp.Enable();
                }
                else
                    mrp.Disable();
            }
        }
    }
}
