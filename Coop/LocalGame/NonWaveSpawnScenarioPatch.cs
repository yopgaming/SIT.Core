using BepInEx.Configuration;
using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Coop.Core.LocalGame
{
    internal class NonWaveSpawnScenarioPatch : ModulePatch
    {
        private static ConfigFile _config;

        public NonWaveSpawnScenarioPatch(ConfigFile config)
        {
            _config = config;
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(NonWavesSpawnScenario), "Run");
        }


        [PatchPrefix]
        public static bool PatchPrefix(NonWavesSpawnScenario __instance)
        {
            var EnableAISpawnWaveSystem = _config.Bind("Server", "Enable AI Spawn Wave System", true
                        , new ConfigDescription("Whether to run the Wave Spawner System. Useful for testing.")).Value;

            var result = !Matchmaker.MatchmakerAcceptPatches.IsClient && EnableAISpawnWaveSystem;
            //PatchConstants.SetFieldOrPropertyFromInstance(__instance, "Enabled", result);
            return result;
        }
    }
}
