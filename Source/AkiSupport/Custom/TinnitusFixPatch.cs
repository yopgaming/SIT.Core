using SIT.Tarkov.Core;
using System.Collections;
using System.Reflection;

namespace SIT.Core.AkiSupport.Custom
{
    public class TinnitusFixPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BetterAudio).GetMethod("StartTinnitusEffect", BindingFlags.Instance | BindingFlags.Public);
        }

        // checks on invoke whether the player is stunned before allowing tinnitus
        [PatchPrefix]
        static bool PatchPrefix()
        {
            return false;
        }
        static IEnumerator CoroutinePassthrough()
        {
            yield return null;
            yield break;
        }
    }
}