using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Core.AkiSupport.SITFixes
{
    internal class BotSettingsRepoClassIsFollowerFixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(BotSettingsRepoClass), "IsFollower");
        }

        [PatchPrefix]
        public static bool Prefix(ref bool __result, EFT.WildSpawnType role)
        {
            __result = false;
            return false;
        }

        [PatchPostfix]
        public static void Postfix(ref bool __result, EFT.WildSpawnType role)
        {
            __result = false;
        }
    }
}
