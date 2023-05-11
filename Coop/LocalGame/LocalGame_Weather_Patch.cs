using EFT;
using EFT.Bots;
using SIT.Coop.Core.LocalGame;
using SIT.Coop.Core.Matchmaker;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.LocalGame
{
    public class LocalGame_Weather_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.LocalGame);

            var method = ReflectionHelpers.GetAllMethodsForType(t)
                .LastOrDefault(x => x.GetParameters().Length == 1
                && x.GetParameters()[0].Name.Contains("timeAndWeather")
                );
            return method;
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref TimeAndWeatherSettings timeAndWeather)
        {
            Logger.LogDebug("LocalGame_Weather_Patch:PatchPrefix");

            if (MatchmakerAcceptPatches.IsClient)
            {

            }
            else
            {
                LocalGameStartingPatch.TimeAndWeather = timeAndWeather;
            }

            return true;
        }
    }
}
