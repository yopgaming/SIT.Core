using EFT;
using Comfort.Common;
using System.Reflection;
using System.Collections;
using SIT.Tarkov.Core;

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
            //bool shouldInvoke = typeof(ActiveHealthControllerClass)
            //    .GetMethod("FindActiveEffect", BindingFlags.Instance | BindingFlags.Public)
            //    .MakeGenericMethod(typeof(ActiveHealthControllerClass)
            //    .GetNestedType("Stun", BindingFlags.Instance | BindingFlags.NonPublic))
            //    .Invoke(Singleton<GameWorld>.Instance.AllPlayers[0].ActiveHealthController, new object[] { EBodyPart.Common }) != null;

            //return shouldInvoke;
            return false;
        }

        // prevent null coroutine exceptions
        static IEnumerator CoroutinePassthrough()
        {
            yield return null;
            yield break;
        }
    }
}