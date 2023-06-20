using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SIT.Core.Coop.Player.Health.RestoreBodyPartPatch;

namespace SIT.Core.Coop.Player.Health
{
    internal class RemoveNegativeEffectsPatch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(PlayerHealthController);

        public override string MethodName => "RemoveNegativeEffects";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool PrePatch(PlayerHealthController __instance)
        {
            var player = __instance.Player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            PlayerHealthController __instance
            , EBodyPart bodyPart
            )
        {
            var player = __instance.Player;

            // If it is a client Drone, do not resend the packet again!
            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                    return;
            }

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }


            RemoveNegativeEffectsPacket removeNegativeEffectsPacket = new ();
            removeNegativeEffectsPacket.AccountId = player.Profile.AccountId;
            removeNegativeEffectsPacket.BodyPart = bodyPart;
            var json = removeNegativeEffectsPacket.ToJson();
            Request.Instance.SendDataToPool(json);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (player.HealthController != null && player.HealthController.IsAlive)
            {
                if (dict == null)
                {
                    Logger.LogError($"Dictionary packet is null?");
                    return;

                }

                if (CallLocally.ContainsKey(player.Profile.AccountId))
                    return;

                RemoveNegativeEffectsPacket removeNegativeEffectsPacket  = Json.Deserialize<RemoveNegativeEffectsPacket>(dict.ToJson());
                CallLocally.Add(player.Profile.AccountId, true);
                player.PlayerHealthController.RemoveNegativeEffects(removeNegativeEffectsPacket.BodyPart);
            }


        }

        public class RemoveNegativeEffectsPacket : BasePlayerPacket
        {
            public EBodyPart BodyPart { get; set; }

            public RemoveNegativeEffectsPacket() : base()
            {
                Method = "RemoveNegativeEffects";
            }
        }

    }
}
