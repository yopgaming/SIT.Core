using System.Reflection;
using EFT;
using System.Linq;
using SIT.Tarkov.Core;

namespace SIT.Core.AkiSupport.Custom
{
    public class QTEPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => typeof(HideoutPlayerOwner).GetMethod(nameof(HideoutPlayerOwner.StopWorkout));

        [PatchPostfix]
        private static void PatchPostfix(HideoutPlayerOwner __instance)
        {
            new Request().PutJson("/client/hideout/workout", new
            {
                skills = __instance.HideoutPlayer.Skills,
                effects = __instance.HideoutPlayer.HealthController.BodyPartEffects
            }
            .ToJson());
        }
    }
}
