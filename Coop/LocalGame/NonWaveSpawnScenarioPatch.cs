using BepInEx.Configuration;
using EFT;
using SIT.Core.Configuration;
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
            var result = !Matchmaker.MatchmakerAcceptPatches.IsClient && PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem;
            ReflectionHelpers.SetFieldOrPropertyFromInstance(__instance, "Enabled", result);
            return result;
        }
    }
}
