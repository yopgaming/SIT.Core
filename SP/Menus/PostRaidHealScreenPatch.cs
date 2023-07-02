using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Core.SP.Menus
{
    public class PostRaidHealScreenPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(ProfileChangeHandler);
            return ReflectionHelpers.GetMethodForType(desiredType, "smethod_0");
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref ERaidMode raidMode)
        {
            raidMode = ERaidMode.Online;

            return true;
        }
    }
}
