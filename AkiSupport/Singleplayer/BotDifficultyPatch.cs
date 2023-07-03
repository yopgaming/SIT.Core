using EFT;
using SIT.Core.Core;
using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;

namespace SIT.Core.AkiSupport.Singleplayer
{
    public class BotDifficultyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var methodName = "LoadDifficultyStringInternal";
            var flags = BindingFlags.Public | BindingFlags.Static;

            return PatchConstants.EftTypes.Single(x => x.GetMethod(methodName, flags) != null)
                .GetMethod(methodName, flags);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref string __result, BotDifficulty botDifficulty, WildSpawnType role)
        {
            __result = AkiBackendCommunication.Instance.GetJson($"/singleplayer/settings/bot/difficulty/{role}/{botDifficulty}");
            return string.IsNullOrWhiteSpace(__result);
        }
    }
}
