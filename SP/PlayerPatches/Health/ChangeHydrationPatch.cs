using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;

namespace SIT.Core.SP.PlayerPatches.Health
{
    internal class ChangeHydrationPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.EftTypes
                .LastOrDefault(x => x.GetMethod("ChangeHydration", BindingFlags.Public | BindingFlags.Instance) != null)
                .GetMethod("ChangeHydration", BindingFlags.Public | BindingFlags.Instance);
        }

        [PatchPostfix]
        public static void PatchPostfix(
            object __instance
            , float value
            )
        {
            if (__instance == HealthListener.Instance.MyHealthController)
            {
                if (HealthListener.Instance.CurrentHealth.Hydration + value > 0)
                    HealthListener.Instance.CurrentHealth.Hydration += value;
            }
        }
    }
}
