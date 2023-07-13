using EFT;
using SIT.Tarkov.Core;
using System.Reflection;
using UnityEngine;

/***
 * Full Credit for this patch goes to SPT-Aki team
 * Original Source is found here - https://dev.sp-tarkov.com/SPT-AKI/Modules
 * Paulov. Made changes to have better reflection and less hardcoding
 */
namespace SIT.Core.SP.Menus
{
    /// <summary>
    /// Force ERaidMode to online to make interface show insurance page
    /// </summary>
    class InsuranceScreenPatch : ModulePatch
    {
        static InsuranceScreenPatch()
        {
            _ = nameof(MainMenuController.InventoryController);
        }

        protected override MethodBase GetTargetMethod()
        {
            var desiredType = typeof(MainMenuController);
            var desiredMethod = desiredType.GetMethod("method_66", BindingFlags.NonPublic | BindingFlags.Instance);

            Logger.LogDebug($"{GetType().Name} Type: {desiredType?.Name}");
            Logger.LogDebug($"{GetType().Name} Method: {desiredMethod?.Name}");

            return desiredMethod;
        }

        [PatchPrefix]
        private static void PrefixPatch(RaidSettings ___raidSettings_0)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Online;
        }

        [PatchPostfix]
        private static void PostfixPatch(RaidSettings ___raidSettings_0)
        {
            ___raidSettings_0.RaidMode = ERaidMode.Local;

            var insuranceReadyButton = GameObject.Find("ReadyButton");
            if (insuranceReadyButton != null)
            {
                insuranceReadyButton.active = false;
            }
        }
    }
}
