using EFT;
using SIT.Core.Misc;
using SIT.Core.SP.PlayerPatches.Health;
using SIT.Tarkov.Core;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Core.SP.PlayerPatches
{
    internal class PlayerInitPatch : ModulePatch
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
            if (OnPlayerInit != null)
                OnPlayerInit(__instance);

            await __result;

            if (profile?.Id.StartsWith("pmc") == true)
            {
                Logger.LogInfo($"Hooking up health listener to profile: {profile.Id}");
                var listener = HealthListener.Instance;
                listener.Init(__instance.HealthController, true);
                Logger.LogInfo($"HealthController instance: {__instance.HealthController.GetHashCode()}");
            }
            else
            {
                //Logger.LogInfo($"Skipped on HealthController instance: {__instance.HealthController.GetHashCode()} for profile id: {profile?.Id}");
            }

            DisplayMessageNotifications.DisplayMessageNotification($"{__instance.Profile.Nickname}[{__instance.Side}][{__instance.Profile.Info.Settings.Role}] has spawned");

        }
    }
}
