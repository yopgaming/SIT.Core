using EFT;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.PlayerPatches.Health;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Tarkov.SP
{
    public class ReplaceInPlayer : ModulePatch
    {
        private static string _playerAccountId;

        protected override MethodBase GetTargetMethod()
        {
            //return typeof(Player).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return PatchConstants.GetMethodForType(typeof(LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static async void PatchPostfix(
            object __instance
            , Task __result
            , object ____healthController
            , object healthController)
        {

            var instanceProfile = __instance.GetType().GetProperty("Profile"
                , BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).GetValue(__instance);
            if (instanceProfile == null)
            {
                Logger.LogInfo("ReplaceInPlayer:PatchPostfix: Couldn't find Profile");
                return;
            }
                
            var instanceAccountProp = instanceProfile.GetType().GetField("AccountId"
                , BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
              
            if (instanceAccountProp == null)
            {
                Logger.LogInfo($"ReplaceInPlayer:PatchPostfix: instanceAccountProp not found");
                return;
            }
            var instanceAccountId = instanceAccountProp.GetValue(instanceProfile).ToString();

            // If is Bot Guid, then ignore it
            if (Guid.TryParse(instanceAccountId, out _))
                return;

            if (string.IsNullOrEmpty(instanceAccountId))
                return;

            if (instanceAccountId != PatchConstants.GetPHPSESSID())
            {
                return;
            }

            var listener = HealthListener.Instance;
            var insthealthController = PatchConstants.GetFieldOrPropertyFromInstance<object>(__instance, "HealthController", false);
           
            if (healthController != null)
            {
                listener.Init(healthController, true); 
            }

        }
    }
}
