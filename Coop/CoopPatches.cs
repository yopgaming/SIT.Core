using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Player;
using SIT.Coop.Core.Player.Weapon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GClass1641;

namespace SIT.Core.Coop
{
    internal class CoopPatches
    {
        public static void Run(BepInEx.Configuration.ConfigFile config)
        {
            // ------ MATCHMAKER -------------------------
            MatchmakerAcceptPatches.Run();

            // ------ PLAYER -------------------------
            new PlayerOnInitPatch(config).Enable();
            //new PlayerOnApplyCorpseImpulsePatch().Enable();
            //new PlayerOnDamagePatch().Enable();
            new PlayerOnDeadPatch(config).Enable();
            new PlayerOnDropBackpackPatch().Enable();
            new PlayerOnEnableSprintPatch().Enable();
            new PlayerOnGesturePatch().Enable();
            new PlayerOnHealPatch().Enable();
            //new PlayerOnInteractWithDoorPatch().Enable();
            //new PlayerOnInventoryOpenedPatch().Enable();
            new PlayerOnJumpPatch().Enable();
            new PlayerOnMovePatch().Enable();
            new PlayerOnSayPatch().Enable();
            //new PlayerOnSetItemInHandsPatch().Enable();
            //new PlayerOnTryProceedPatch().Enable();

            // ------ WEAPON -------------------------
            new WeaponOnDropPatch().Enable();
            new WeaponOnTriggerPressedPatch().Enable();
            //new WeaponOnReloadMagPatch().Enable();

          

        }
    }
}
