using Comfort.Common;
using EFT;
using SIT.Core.Misc;
using SIT.Core.SP.PlayerPatches;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            if(Singleton<GameWorld>.Instance != null) 
            { 
                if(Singleton<GameWorld>.Instance.TryGetComponent<CoopGameComponent>(out var component))
                {
                    GameObject.Destroy(component);
                }
            
            }
        }
    }
}
