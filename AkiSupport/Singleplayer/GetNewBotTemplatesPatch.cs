using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
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
            _getNewProfileMethod = ReflectionHelpers.GetMethodForType(typeof(LocalGameBotCreator), nameof(LocalGameBotCreator.GetNewProfile));
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(LocalGameBotCreator), nameof(LocalGameBotCreator.CreateProfile));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task<Profile> __result, BotsPresets __instance, CreationData data, bool withDelete)
        {
            withDelete = true;
            return true; // do original method
        }

        private static Profile GetFirstResult(Task<Profile[]> task)
        {
            var result = task.Result[0];
            Logger.LogInfo($"{DateTime.Now:T} Loading bot {result.Info.Nickname} profile from server. role: {result.Info.Settings.Role} side: {result.Side}");

            return result;
        }
    }
}
