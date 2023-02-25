using Comfort.Common;
using EFT;
using EFT.Airdrop;
using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;

namespace SIT.Core.AkiSupport.Airdrops
{
    public class AirdropPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            var points = LocationScene.GetAll<AirdropPoint>().Any();

            if (gameWorld != null && points)
            {
                gameWorld.gameObject.AddComponent<AirdropsManager>();
            }
        }
    }
}