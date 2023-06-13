using Comfort.Common;
using EFT;
using EFT.Bots;
using SIT.Core.Configuration;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.LocalGame
{
    internal class LocalGameSpawnAICoroutinePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetMethod = ReflectionHelpers.GetAllMethodsForType(typeof(EFT.LocalGame))
                .LastOrDefault(
                m =>
                !m.IsVirtual
                && m.GetParameters().Length >= 4
                && m.GetParameters()[0].ParameterType == typeof(float)
                && m.GetParameters()[0].Name == "startDelay"
                && m.GetParameters()[1].Name == "controllerSettings"
                );

            return targetMethod;
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotControllerSettings controllerSettings)
        {
            // Keep bots levels low for Coop, reduce performance lag
            if (Matchmaker.MatchmakerAcceptPatches.IsClient || !PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem)
            {
                controllerSettings.BotAmount = EBotAmount.NoBots;
                controllerSettings.IsEnabled = false;
            }
            else
                controllerSettings.BotAmount = EBotAmount.Low;

            return true;
        }

        [PatchPostfix]
        public static IEnumerator PatchPostfix(
            IEnumerator __result,
            EFT.LocalGame __instance,
            ISpawnSystem spawnSystem,
            Callback runCallback,
            WavesSpawnScenario ___wavesSpawnScenario_0,
            NonWavesSpawnScenario ___nonWavesSpawnScenario_0
        )
        {
            //Logger.LogInfo($"LocalGameSpawnAICoroutinePatch:PatchPostfix");


            yield return __result;
        }

    }
}
