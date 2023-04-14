using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Sounds
{
    internal class WeaponSoundPlayer_FireSonicSound_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(WeaponSoundPlayer), "FireSonicSound");
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            return false;
        }
    }
}
