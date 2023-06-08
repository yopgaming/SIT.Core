using Comfort.Common;
using EFT;
using EFT.Interactive;
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
    public class ExfiltrationExpansion : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(typeof(GameWorld), "OnGameStarted");
            return method;
        }

        [PatchPostfix]
        public static void PatchPostFix()
        {
            var gameWorld = Singleton<GameWorld>.Instance;

            if (gameWorld == null || gameWorld.RegisteredPlayers == null || gameWorld.ExfiltrationController == null)
            {
                Logger.LogError("Unable to Find Gameworld or RegisterPlayers.");
                return;
            }

            EFT.Player player = gameWorld.RegisteredPlayers.Find(p => p.IsYourPlayer);

            var exfilController = gameWorld.ExfiltrationController;
            

            if (player.Location != "hideout")
            {
                foreach (var exfil in exfilController.ExfiltrationPoints)
                {
                    //Logger.LogInfo(exfil.Description);
                    exfil.Enable();
                }

                if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
                {
                    ExfiltrationControllerClass.Instance.InitAllExfiltrationPoints(coopGameComponent.LocalGameInstance.Location_0.exits, false, "", true);
                    foreach (ExfiltrationPoint exfiltrationPoint in ExfiltrationControllerClass.Instance.ExfiltrationPoints)
                    {
                        ReflectionHelpers.GetMethodForType(coopGameComponent.LocalGameInstance.GetType(), "UpdateExfiltrationUi")
                            .Invoke(coopGameComponent.LocalGameInstance, new object[] { exfiltrationPoint, false, true });
                    }
                }

                foreach (var exfil in exfilController.ExfiltrationPoints)
                {
                    //Logger.LogInfo(exfil.Description);
                    exfil.Enable();
                }
            }
        }
    }
}
