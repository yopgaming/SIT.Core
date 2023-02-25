using EFT;
using SIT.Tarkov.Core.PlayerPatches.Health;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.PlayerPatches
{
    internal class PlayerInitPatch : ModulePatch
    {
        public static event Action<EFT.LocalPlayer> OnPlayerInit;

        protected override MethodBase GetTargetMethod()
        {
            return PatchConstants.GetMethodForType(typeof(EFT.LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static
            async
            void
            PatchPostfix(Task __result, EFT.LocalPlayer __instance, Profile profile)
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

            PatchConstants.DisplayMessageNotification($"{__instance.Profile.Nickname}[{__instance.Side}][{__instance.Profile.Info.Settings.Role}] has spawned");

        }

        //if (__instance.IsAI)
        //{
        //    BotSystemHelpers.AddActivePlayer(__instance);
        //}

    }
}
