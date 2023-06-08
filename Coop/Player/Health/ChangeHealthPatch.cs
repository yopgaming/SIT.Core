using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Misc;
using SIT.Core.SP.PlayerPatches.Health;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.Player.Health
{
    internal class ChangeHealthPatch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(PlayerHealthController);

        public override string MethodName => "ChangeHealth";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(PlayerHealthController), MethodName, findFirst: true);

        }

        [PatchPostfix]
        public static void PatchPostfix(
            PlayerHealthController __instance
            , EBodyPart bodyPart
            , float value
            )
        {
            var player = __instance.Player; // ReflectionHelpers.GetFieldOrPropertyFromInstance<EFT.Player>(__instance, "Player", false);
            if(player != null)
            {
                if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                {
                    if (!prc.IsClientDrone)
                        return;
                }

                // Only run this when we are healing. All damage is handled elsewhere!
                if (value <= 0)
                    return;

                ChangeHealthPacket changeHealthPacket = new ChangeHealthPacket();
                changeHealthPacket.Value = value;
                changeHealthPacket.BodyPart = bodyPart;
                changeHealthPacket.PartValue = __instance.GetBodyPartHealth(bodyPart, true).Current + value;
                changeHealthPacket.AccountId = player.Profile.AccountId;
                changeHealthPacket.Method = "ChangeHealth";
                changeHealthPacket.Time = DateTime.Now.Ticks;
                var json = changeHealthPacket.ToJson();
                Request.Instance.SendDataToPool(json);
            }
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (player == null) return;

            if (player.PlayerHealthController == null)
                return;

            if (!player.PlayerHealthController.IsAlive)
                return;

            Logger.LogDebug("ChangeHealthPatch.Replicated");
            Logger.LogDebug("Received");
            Logger.LogDebug(dict.ToJson());

            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (!prc.IsClientDrone)
                    return;

                // Convert back to ChangeHealthPacket 
                ChangeHealthPacket changeHealthPacket = Json.Deserialize<ChangeHealthPacket>(dict.ToJson());
                //HealthListener.SetCurrentHealth(player.PlayerHealthController, )
                //player.PlayerHealthController.
                //player.PlayerHealthController.ChangeHealth(changeHealthPacket.BodyPart, changeHealthPacket.PartValue, default(DamageInfo));
            }

        }

        public class ChangeHealthPacket : BasePlayerPacket
        {
            public EBodyPart BodyPart { get; set; }
            public float Value { get; set; }
            public float PartValue { get; set; }

        }
    }
}
