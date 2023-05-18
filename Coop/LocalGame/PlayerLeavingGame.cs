using SIT.Core.SP.PlayerPatches;
using SIT.Tarkov.Core;
using System.Reflection;
using UnityEngine;

namespace SIT.Core.Coop.LocalGame
{
    internal class Player_LeavingGame_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return OfflineSaveProfile.GetMethod();
        }

        [PatchPostfix]
        public static void Postfix(string profileId)
        {
            Logger.LogDebug("PlayerLeavingGame.Postfix");
            if (CoopGameComponent.TryGetCoopGameComponent(out var component))
            {
                Logger.LogDebug("PlayerLeavingGame.Postfix.Destroying CoopGameComponent component");
                GameObject.Destroy(component);
            }
        }
    }
}
