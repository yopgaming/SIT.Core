using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.WeaponSoundPlayerPatches
{
    internal class WeaponSoundPlayer_FireBullet_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(WeaponSoundPlayer);

        public override string MethodName => "WSPFireBullet";

        public static Dictionary<string, bool> CallLocally
           = new Dictionary<string, bool>();

        [PatchPrefix]
        public static bool PrePatch()
        {
            return true;
            //var player = ___Player;
            //if (player == null)
            //    return false;

            //var result = false;
            //if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            //    result = true;

            //return result;
        }


        [PatchPostfix]
        public static void PostPatch(WeaponSoundPlayer __instance)
        {
            var player = ReflectionHelpers.GetFieldOrPropertyFromInstance<EFT.Player>(__instance, "Player", false);
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> packet = new Dictionary<string, object>();
            packet.Add("t", DateTime.Now.Ticks);
            packet.Add("m", "WSPFireBullet");
            ServerCommunication.PostLocalPlayerData(player, packet);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {


        }


        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, "FireBullet", false, true);
        }
    }
}
