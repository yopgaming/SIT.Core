using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Core.SP.PlayerPatches.Health
{
    public class MainMenuControllerForHealthListenerPatch : ModulePatch
    {
        static MainMenuControllerForHealthListenerPatch()
        {
            _ = nameof(IHealthController.HydrationChangedEvent);
            _ = nameof(MainMenuController.HealthController);
        }

        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MainMenuController);
            var desiredMethod = ReflectionHelpers.GetMethodForType(desiredType, "ShowScreen");

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPostfix]
        private static void PatchPostfix(MainMenuController __instance)
        {
            var healthController = __instance.HealthController;
            var listener = HealthListener.Instance;

            Logger.LogInfo("MainMenuControllerForHealthListenerPatch:PatchPostfix");
            if (healthController == null)
            {
                Logger.LogInfo("MainMenuControllerPatch() - healthController is null");
            }

            if (listener == null)
            {
                Logger.LogInfo("MainMenuControllerPatch() - listener is null");
            }

            if (healthController != null && listener != null)
            {
                listener.Init(healthController, false);
            }


            if (HealthListener.Instance != null)
                AkiBackendCommunication.Instance.PostJson("/player/health/sync", HealthListener.Instance.CurrentHealth.ToJson());


        }
    }
}
