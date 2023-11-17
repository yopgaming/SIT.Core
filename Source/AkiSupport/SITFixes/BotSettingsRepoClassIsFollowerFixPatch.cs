using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;

namespace SIT.Core.AkiSupport.SITFixes
{
    internal class BotSettingsRepoClassIsFollowerFixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = PatchConstants.EftTypes.First(x => x.GetMethods().Any(y => y.Name == "IsFollower" && y.GetParameters().Length == 1 && y.GetParameters()[0].ParameterType == typeof(WildSpawnType)));
            return ReflectionHelpers.GetMethodForType(t, "IsFollower");
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
