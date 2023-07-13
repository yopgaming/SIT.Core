using EFT;
using SIT.Core.Misc;
using SIT.Core.SP.PlayerPatches.Health;
using SIT.Tarkov.Core;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Core.SP.PlayerPatches
{
    internal class Player_Init_SP_Patch : ModulePatch
    {
        public static event Action<LocalPlayer> OnPlayerInit;

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static
            async
            void
            PatchPostfix(Task __result, LocalPlayer __instance, Profile profile)
        {
            if (__instance is HideoutPlayer)
                return;

            if (OnPlayerInit != null)
                OnPlayerInit(__instance);

            await __result;

            var listener = HealthListener.Instance;
            if (profile?.Id.StartsWith("pmc") == true && __instance.IsYourPlayer)
            {
                Logger.LogInfo($"Hooking up health listener to profile: {profile.Id}");
                listener.Init(__instance.HealthController, true);
                //Logger.LogInfo($"HealthController instance: {__instance.HealthController.GetHashCode()}");
            }
            else
            {
                //Logger.LogInfo($"Skipped on HealthController instance: {__instance.HealthController.GetHashCode()} for profile id: {profile?.Id}");
            }

        }
    }
}
