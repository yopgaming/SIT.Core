//using Comfort.Common;
//using EFT;
//using EFT.Bots;
//using SIT.Coop.Core.Matchmaker;
//using SIT.Core.Configuration;
//using SIT.Core.Misc;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections;
//using System.Linq;
//using System.Reflection;
//using UnityEngine;

//namespace SIT.Coop.Core.LocalGame
//{
//    internal class LocalGameSpawnAICoroutinePatch : ModulePatch
//    {
//        private Type BaseGameType { get; set; } = typeof(EFT.BaseLocalGame<GamePlayerOwner>);
//        private Type LocalGameType { get; set; } = typeof(EFT.LocalGame);

//        protected override MethodBase GetTargetMethod()
//        {
//            var targetMethod = ReflectionHelpers.GetAllMethodsForType(LocalGameType, excludeBaseType: true)
//                .FirstOrDefault(
//                m =>
//                //!m.IsVirtual
//                //&&
//                m.GetParameters().Length >= 4
//                && m.GetParameters()[0].ParameterType == typeof(float)
//                && m.GetParameters()[0].Name == "startDelay"
//                && m.GetParameters()[1].Name == "controllerSettings"
//                );

//            Logger.LogDebug($"LocalGameSpawnAICoroutinePatch:TargetType:{LocalGameType}:TargetMethod:{targetMethod.Name}");
//            return targetMethod;
//        }

//        [PatchPrefix]
//        public static bool PatchPrefix(ref BotControllerSettings controllerSettings)
//        {
//            // Keep bots levels low for Coop, reduce performance lag
//            if (Matchmaker.MatchmakerAcceptPatches.IsClient || !PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem)
//            {
//                controllerSettings.BotAmount = EBotAmount.NoBots;
//                controllerSettings.IsEnabled = false;
//                //return false;
//            }
//            else
//            {
//                controllerSettings.BotAmount = EBotAmount.Low;
//                //return true;
//            }

//            return true;

//        }

//        [PatchPostfix]
//        public static IEnumerator PatchPostfix(
//            IEnumerator __result,
//            EFT.LocalGame __instance,
//            float startDelay,
//            BotControllerSettings controllerSettings,
//            ISpawnSystem spawnSystem, 
//            Callback runCallback,
//            WavesSpawnScenario ___wavesSpawnScenario_0,
//            NonWavesSpawnScenario ___nonWavesSpawnScenario_0
//        )
//        {
//            //if (MatchmakerAcceptPatches.IsClient)
//            //{
//            //    yield return new WaitForSeconds(startDelay);
//            //    yield return ReflectionHelpers.GetMethodForType(__instance.GetType(), "method_17") // This method just calls BaseLocalGame<>, TODO: change this
//            //        .Invoke(__instance, new object[] { startDelay, controllerSettings, spawnSystem, runCallback }); // method_17(startDelay, controllerSettings, spawnSystem, runCallback);
//            //    yield return __result;
//            //}
//            //Logger.LogInfo($"LocalGameSpawnAICoroutinePatch:PatchPostfix");



//            if (MatchmakerAcceptPatches.IsClient)
//            {
//                yield return new WaitForSeconds(1);
//                if (___nonWavesSpawnScenario_0 != null)
//                    ___nonWavesSpawnScenario_0.Stop();

//                if (___wavesSpawnScenario_0 != null)
//                    ___wavesSpawnScenario_0.Stop();
//            }

//            yield return __result;

//        }

//    }
//}
