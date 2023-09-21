using Comfort.Common;
using EFT;
using SIT.Core.SP.Components;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Core.SP.Raid
{
    /// <summary>
    /// Original class writen by SPT-Aki Devs. 
    /// </summary>
    public class LighthouseBridgePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null || gameWorld.MainPlayer.Location.ToLower() != "lighthouse") return;

            gameWorld.GetOrAddComponent<LighthouseProgressionComponent>();
        }
    }
}