using EFT.UI.Matchmaker;
using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using EFT.UI;
using UnityEngine.UIElements;
using EFT;
using HarmonyLib;
using UnityEngine.Events;
using System.Text.RegularExpressions;

namespace SIT.Coop.Core.Matchmaker
{
    public class MatchmakerAcceptScreenClosePatch : ModulePatch
    {
		static BindingFlags privateFlags = BindingFlags.NonPublic | BindingFlags.Instance;

		public static Type GetThisType()
		{
            return Tarkov.Core.PatchConstants.EftTypes
                 .Single(x => x == typeof(EFT.UI.Matchmaker.MatchMakerAcceptScreen));
        }

        protected override MethodBase GetTargetMethod()
        {

            var methodName = "Close";

            return GetThisType().GetMethods(privateFlags).First(x=>x.Name == methodName);

        }


        private static object screenController;

        //private static object grouping => MatchmakerAcceptPatches.GetGrouping();

        private static Button _updateListButton;

        private static CanvasGroup _canvasGroup;

        private static Profile profile;

		[PatchPrefix]
        private static bool PatchPrefix(
            ref EFT.UI.Matchmaker.MatchMakerAcceptScreen __instance
            )
        {
            Logger.LogInfo("MatchmakerAcceptScreenAwakePatch.PatchPrefix");
            return true; // run the original

        }

    }

	
}
