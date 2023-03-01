using System.Linq;
using System.Reflection;
using SIT.Tarkov.Core;

namespace SIT.Core.SP.PlayerPatches.Health
{
    internal class ChangeHealthPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = PatchConstants.EftTypes
                .First(x =>
                    PatchConstants.GetMethodForType(x, "ChangeHealth") != null
                    && PatchConstants.GetMethodForType(x, "Kill") != null
                    && PatchConstants.GetMethodForType(x, "DoPainKiller") != null
                    && PatchConstants.GetMethodForType(x, "DoScavRegeneration") != null
                    && x.GetMethod("GetOverallHealthRegenTime", BindingFlags.Public | BindingFlags.Instance) == null // We don't want this one
                    );
            Logger.LogInfo("ChangeHealth:" + t.FullName);
            var method = PatchConstants.GetMethodForType(t, "ChangeHealth");

            Logger.LogInfo("ChangeHealth:" + method.Name);
            return method;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            object __instance
            , EBodyPart bodyPart
            , float value
            , object damageInfo)
        {
            if (__instance == HealthListener.Instance.MyHealthController)
            {
                HealthListener.Instance.CurrentHealth.Health[bodyPart].ChangeHealth(value);
            }
        }
    }
}
