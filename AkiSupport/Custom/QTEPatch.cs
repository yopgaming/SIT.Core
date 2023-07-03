using EFT;
using SIT.Core.Core;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Core.AkiSupport.Custom
{
    public class QTEPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(HideoutPlayerOwner).GetMethod(nameof(HideoutPlayerOwner.StopWorkout));

        [PatchPostfix]
        private static void PatchPostfix(HideoutPlayerOwner __instance)
        {
            AkiBackendCommunication.Instance.PutJson("/client/hideout/workout", new
            {
                skills = __instance.HideoutPlayer.Skills,
                effects = __instance.HideoutPlayer.HealthController.BodyPartEffects
            }
            .ToJson());
        }
    }
}
