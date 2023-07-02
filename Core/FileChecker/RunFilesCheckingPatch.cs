using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Core.Core.FileChecker
{
    internal class RunFilesCheckingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(CommonClientApplication<ISession>), "RunFilesChecking");
        }

        [PatchPrefix]
        public static bool Prepatch()
        {
            return false;
        }

        [PatchPostfix]
        public static async Task Postpatch(
            Task __result
            )
        {
            await Task.Delay(1);
            __result = Task.CompletedTask;
        }
    }
}
