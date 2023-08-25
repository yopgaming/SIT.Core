using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Reflection;

namespace Aki.Custom.Patches
{
    /// <summary>
    /// SPT PMC enum value is high enough in wildspawntype it means the first aid class that gets init doesnt have an implementation
    /// On heal event, remove all negative effects from limbs e.g. light/heavy bleeds
    /// </summary>
    public class PmcFirstAidPatch : ModulePatch
    {
        private static readonly string methodName = "FirstAidApplied";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(FirstAid), methodName);
        }

        [PatchPrefix]
        private static bool PatchPrefix(BotOwner ___botOwner_0)
        {
            if (___botOwner_0.IsRole((WildSpawnType)0x26) || ___botOwner_0.IsRole((WildSpawnType)0x27))
            {
                var healthController = ___botOwner_0.GetPlayer.ActiveHealthController;

                healthController.RemoveNegativeEffects(EBodyPart.Head);
                healthController.RemoveNegativeEffects(EBodyPart.Chest);
                healthController.RemoveNegativeEffects(EBodyPart.Stomach);
                healthController.RemoveNegativeEffects(EBodyPart.LeftLeg);
                healthController.RemoveNegativeEffects(EBodyPart.RightLeg);
                healthController.RemoveNegativeEffects(EBodyPart.LeftArm);
                healthController.RemoveNegativeEffects(EBodyPart.RightArm);
            }

            return false; // skip original
        }
    }
}