using EFT.UI;
using SIT.Coop.Core.Matchmaker;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.Matchmaker
{
    public class EnvironmentUIRootPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(EnvironmentUIRoot).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPostfix]
        private static void PatchPostfix(EnvironmentUIRoot __instance)
        {
            MatchmakerAcceptPatches.EnvironmentUIRoot = ((Component)__instance).gameObject;
        }
    }
}
