using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.AkiSupport.Custom
{

    /// <summary>
    /// Prevent BotSpawnerClass from adjusting the spawn process value to be below 0
    /// This fixes aiamount = high spawning 80+ bots on maps like streets/customs
    /// int_0 = all bots alive
    /// int_1 = followers alive
    /// int_2 = bosses currently alive
    /// int_3 = spawn process? - current guess is open spawn positions - bsg doesnt seem to handle negative vaues well
    /// int_4 = max bots
    /// </summary>
    public class SpawnProcessNegativeValuePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(AbstractBotSpawner);
            var desiredMethod = ReflectionHelpers.GetMethodForType(desiredType, "CheckOnMax");

            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        private static void PatchPreFix(ref int ___int_3)
        {
            // Spawn process
            if (___int_3 < 0)
            {
                ___int_3 = 0;
            }
        }
    }
}