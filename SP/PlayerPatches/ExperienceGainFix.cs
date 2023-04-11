using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;

namespace SIT.Core.SP.PlayerPatches
{
    public class ExperienceGainFix : ModulePatch
    {

        [PatchPrefix]
        static void PrefixPatch(ref bool isOnline)
        {
            isOnline = true;
        }

        [PatchPostfix]
        static void PostfixPatch(ref bool isOnline)
        {
            //isOnline = false;
        }

        protected override MethodBase GetTargetMethod()
        {
            var returnedType = PatchConstants.EftTypes.Single(x =>
                 x.FullName.StartsWith(typeof(EFT.UI.SessionEnd.SessionResultExperienceCount).FullName)
                 && ReflectionHelpers.GetPropertyFromType(x, "KeyScreen") != null
                 );
            return returnedType.GetConstructor(new Type[] { typeof(EFT.Profile), typeof(bool), typeof(EFT.ExitStatus) });
        }
    }
}