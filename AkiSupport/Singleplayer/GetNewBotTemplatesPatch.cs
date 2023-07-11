using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Core.AkiSupport.Singleplayer
{
    public class GetNewBotTemplatesPatch : ModulePatch
    {
        private static MethodInfo _getNewProfileMethod;

        public GetNewBotTemplatesPatch()
        {
            _getNewProfileMethod = ReflectionHelpers.GetMethodForType(typeof(BotsPresets), nameof(BotsPresets.GetNewProfile));
            GetLogger(typeof(GetNewBotTemplatesPatch)).LogInfo($"{DateTime.Now:T}");

        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BotsPresets), nameof(BotsPresets.CreateProfile));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task<Profile> __result, BotsPresets __instance, CreationData data, bool withDelete)
        {
            withDelete = true;
            GetLogger(typeof(GetNewBotTemplatesPatch)).LogInfo($"{DateTime.Now:T} GetNewBotTemplatesPatch");
            return true; // do original method
        }

    }

    public class FillCreationDataWithProfilesPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(LocalGameBotCreator), nameof(LocalGameBotCreator.FillCreationDataWithProfiles));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task __result, LocalGameBotCreator __instance, CreationData data)
        {
       
            GetLogger(typeof(FillCreationDataWithProfilesPatch)).LogInfo($"{DateTime.Now:T} FillCreationDataWithProfilesPatch");
            return true; // do original method
        }
    }

    public class BotCreatorOptimizePatch : ModulePatch
    {
        public BotCreatorOptimizePatch() { 
        
            GetLogger(typeof(BotCreatorOptimizePatch)).LogInfo($"{DateTime.Now:T} BotCreatorOptimizePatch()");

        }
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(LocalGameBotCreator), "method_1");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref List<WaveInfo> __result, List<WaveInfo> wavesProfiles, List<WaveInfo> delayed)
        {
            __result = wavesProfiles;
            GetLogger(typeof(BotCreatorOptimizePatch)).LogInfo($"{DateTime.Now:T} method_1");
            return false; // do original method
        }
    }
}
