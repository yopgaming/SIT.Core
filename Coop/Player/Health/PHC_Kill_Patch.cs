using EFT;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.Coop.Player.Health
{
    internal class PHC_Kill_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(PlayerHealthController);

        public override string MethodName => "Kill";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool PrePatch(
            PlayerHealthController __instance
            )
        {
            var player = __instance.Player;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;
            return result;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            PlayerHealthController __instance
            , EDamageType damageType
            )
        {
            //Logger.LogDebug("RestoreBodyPartPatch:PatchPostfix");

            var player = __instance.Player;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }



            KillPacket killPacket = new();
            killPacket.AccountId = player.Profile.AccountId;
            killPacket.DamageType = damageType;
            var json = killPacket.ToJson();
            //Logger.LogInfo(json);
            AkiBackendCommunication.Instance.SendDataToPool(json);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            KillPacket killPacket = Json.Deserialize<KillPacket>(dict.ToJson());

            if (HasProcessed(GetType(), player, killPacket))
                return;

            if (CallLocally.ContainsKey(player.Profile.AccountId))
                return;

            Logger.LogDebug($"Replicated KILL {player.Profile.AccountId}");

            CallLocally.Add(player.Profile.AccountId, true);
            player.ActiveHealthController.Kill(killPacket.DamageType);
            //player.PlayerHealthController.Kill(killPacket.DamageType);
            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                firearmCont.SetTriggerPressed(false);
                ReflectionHelpers.GetMethodForType(firearmCont.WeaponSoundPlayer.GetType(), "Release").Invoke(firearmCont.WeaponSoundPlayer, new object[1] { 0f });
                ReflectionHelpers.GetMethodForType(firearmCont.WeaponSoundPlayer.GetType(), "StopSoundCoroutine").Invoke(firearmCont.WeaponSoundPlayer, new object[0]);
            }
        }

        class KillPacket : BasePlayerPacket
        {
            public EDamageType DamageType { get; set; }

            public KillPacket()
            {
                Method = "Kill";
            }
        }
    }
}
