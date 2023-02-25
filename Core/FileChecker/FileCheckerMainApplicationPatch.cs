using EFT;
using FilesChecker;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.FileChecker
{
    public class FileCheckerMainApplicationPatch : ModulePatch
    {
        public FileCheckerMainApplicationPatch()
        {

        }

        protected override MethodBase GetTargetMethod() => PatchConstants.GetAllMethodsForType(typeof(TarkovApplication))
            .Single(x => x.GetParameters().Length >= 2
                && x.GetParameters()[0].Name == "ordinaryFileEnsuranceMode"
                && x.GetParameters()[1].Name == "criticalFileEnsuranceMode");

        [PatchPrefix]
        public static bool PatchPrefix(
            TarkovApplication __instance
            , ConsistencyEnsuranceMode ordinaryFileEnsuranceMode
            , ConsistencyEnsuranceMode criticalFileEnsuranceMode
            , object timeHasComeScreenController
            , CancellationToken token
            , Task __result
            )
        {
            __result.GetAwaiter().GetResult();
            return true;
        }
    }
}
