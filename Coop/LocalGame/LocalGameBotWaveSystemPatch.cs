using EFT;
using EFT.Bots;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Coop.Core.LocalGame
{
    internal class LocalGameBotWaveSystemPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.LocalGame);
            //var t = LocalGamePatches.LocalGameInstance.GetType();

            var method = PatchConstants.GetAllMethodsForType(t)
                .FirstOrDefault(x => x.GetParameters().Length >= 2
                && x.GetParameters()[0].Name.Contains("botsSettings")
                && x.GetParameters()[1].Name.Contains("spawnSystem")
                );
            return method;
        }

        [PatchPrefix]
        public static void PatchPrefix(BotControllerSettings botsSettings, ISpawnSystem spawnSystem)
        {
            if (Matchmaker.MatchmakerAcceptPatches.IsClient)
            {
                botsSettings.BotAmount = EBotAmount.NoBots;
            }
        }
    }
}
