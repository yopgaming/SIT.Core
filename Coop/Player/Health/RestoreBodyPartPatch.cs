using EFT.HealthSystem;
using SIT.Coop.Core.Player;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SIT.Core.Coop.Player.Health
{
    internal class RestoreBodyPartPatch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(PlayerHealthController);

        public override string MethodName => "RestoreBodyPart";

        public static Dictionary<string, bool> CallLocally = new();

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            //Logger.LogDebug("RestoreBodyPartPatch:PrePatch");
            var result = false;
            return result;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            PlayerHealthController __instance
            , EBodyPart bodyPart
            , float healthPenalty
            )
        {
            //Logger.LogDebug("RestoreBodyPartPatch:PatchPostfix");

            var player = __instance.Player;

            // If it is a client Drone, do not resend the packet again!
            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
            {
                if (prc.IsClientDrone)
                    return;
            }


            RestoreBodyPartPacket restoreBodyPartPacket = new();
            restoreBodyPartPacket.AccountId = player.Profile.AccountId;
            restoreBodyPartPacket.BodyPart = bodyPart;
            restoreBodyPartPacket.HealthPenalty = healthPenalty;
            var json = restoreBodyPartPacket.ToJson();
            //Logger.LogInfo(json);
            AkiBackendCommunication.Instance.SendDataToPool(json);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if (player.HealthController != null && player.HealthController.IsAlive)
            {
                //Logger.LogDebug("Replicated: Calling RestoreBodyPart");
                if (dict == null)
                {
                    Logger.LogError($"Dictionary packet is null?");
                    return;

                }
                //Logger.LogInfo(dict.ToJson());

                RestoreBodyPartPacket restoreBodyPartPacket = Json.Deserialize<RestoreBodyPartPacket>(dict.ToJson());
                var bodyPartDict = GetBodyPartDictionary(player);

                var state = bodyPartDict[restoreBodyPartPacket.BodyPart];
                if (state == null)
                {
                    Logger.LogError($"Could not retreive {player.ProfileId}'s Health State for Body Part {restoreBodyPartPacket.BodyPart}");
                    return;
                }
                bodyPartDict[restoreBodyPartPacket.BodyPart].IsDestroyed = false;
                var healthPenalty = restoreBodyPartPacket.HealthPenalty + (1f - restoreBodyPartPacket.HealthPenalty) * (float)player.Skills.SurgeryReducePenalty;
                Logger.LogDebug("RestoreBodyPart::HealthPenalty::" + healthPenalty);
                bodyPartDict[restoreBodyPartPacket.BodyPart].Health
                    = new HealthValue(1f, Mathf.Max(1f, Mathf.Ceil(bodyPartDict[restoreBodyPartPacket.BodyPart].Health.Maximum * healthPenalty)), 0f);
            }


        }

        private IReadOnlyDictionary<EBodyPart, AHealthController<AbstractHealth.AbstractHealthEffect>.BodyPartState> GetBodyPartDictionary(EFT.Player player)
        {
            try
            {
                var bodyPartDict
                = ReflectionHelpers.GetFieldOrPropertyFromInstance<IReadOnlyDictionary<EBodyPart, AHealthController<AbstractHealth.AbstractHealthEffect>.BodyPartState>>(player.PlayerHealthController, "IReadOnlyDictionary_0", false);
                if (bodyPartDict == null)
                {
                    Logger.LogError($"Could not retreive {player.Id}'s Health State Dictionary");
                    return null;
                }
                //Logger.LogInfo(bodyPartDict.ToJson());
                return bodyPartDict;
            }
            catch (Exception)
            {

                var field = ReflectionHelpers.GetFieldFromType(player.PlayerHealthController.GetType(), "IReadOnlyDictionary_0");
                Logger.LogError(field);
                var type = field.DeclaringType;
                Logger.LogError(type);
                var val = field.GetValue(player.PlayerHealthController);
                Logger.LogError(val);
                var valType = field.GetValue(player.PlayerHealthController).GetType();
                Logger.LogError(valType);
            }

            return null;
        }

        public class RestoreBodyPartPacket : BasePlayerPacket
        {
            public EBodyPart BodyPart { get; set; }
            public float HealthPenalty { get; set; }

            public RestoreBodyPartPacket() : base()
            {
                Method = "RestoreBodyPart";
            }
        }

        //protected sealed class BodyPartState
        //{
        //    public bool IsDestroyed;

        //    public HealthValue Health;
        //}
    }
}
