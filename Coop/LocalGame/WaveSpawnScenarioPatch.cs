using BepInEx.Configuration;
using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Coop.Core.LocalGame
{
    internal class WaveSpawnScenarioPatch : ModulePatch
    {
        private static ConfigFile _config;

        public WaveSpawnScenarioPatch(ConfigFile config)
        {
            _config = config;
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(WavesSpawnScenario), "Run");
        }


        [PatchPrefix]
        public static bool PatchPrefix(WavesSpawnScenario __instance)
        {
            var EnableAISpawnWaveSystem = _config.Bind("Coop", "EnableAISpawnWaveSystem", true
                        , new ConfigDescription("Whether to run the Wave Spawner System. Useful for testing.")).Value;

            var result = !Matchmaker.MatchmakerAcceptPatches.IsClient && EnableAISpawnWaveSystem;
            ReflectionHelpers.SetFieldOrPropertyFromInstance(__instance, "Enabled", result);
            return result;
        }
    }
}
