using SIT.Core.Misc;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core
{
    public class BattlEyePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var methodName = "RunValidation";
            var flags = BindingFlags.Public | BindingFlags.Instance;

            return PatchConstants.EftTypes.Single(x => x.GetMethod(methodName, flags) != null)
                .GetMethod(methodName, flags);
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task __result, ref bool ___bool_0)
        {
            ___bool_0 = true;
            __result = Task.CompletedTask;
            return false;
        }
    }

    public class BattlEyePatchFirstPassRun : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BattlEye.BEClient), "Run", false, false);
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }

    public class BattlEyePatchFirstPassUpdate : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BattlEye.BEClient), "Update", false, false);
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            return false;
        }
    }

    public class BattlEyePatchFirstPassReceivedPacket : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BattlEye.BEClient), "ReceivedPacket", false, false);
        }

        [PatchPrefix]
        private static bool PatchPrefix()
        {
            Logger.LogInfo("BattlEyePatchFirstPassReceivedPacket:PatchPrefix");
            return false;
        }
    }
}
