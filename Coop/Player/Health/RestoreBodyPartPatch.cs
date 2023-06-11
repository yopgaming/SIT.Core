using EFT;
using EFT.HealthSystem;
using HarmonyLib;
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
using static SIT.Core.Coop.Player.Health.ChangeHealthPatch;

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
            Logger.LogDebug("RestoreBodyPartPatch:PrePatch");
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
            Logger.LogDebug("RestoreBodyPartPatch:PatchPostfix");

            var player = __instance.Player; // ReflectionHelpers.GetFieldOrPropertyFromInstance<EFT.Player>(__instance, "Player", false);
            RestoreBodyPartPacket restoreBodyPartPacket = new RestoreBodyPartPacket();
            restoreBodyPartPacket.AccountId = player.Profile.AccountId;
            restoreBodyPartPacket.BodyPart = bodyPart;
            restoreBodyPartPacket.HealthPenalty = healthPenalty;
            restoreBodyPartPacket.Time = DateTime.Now.Ticks;
            restoreBodyPartPacket.Method = "RestoreBodyPart";
            var json = restoreBodyPartPacket.ToJson();
            Logger.LogInfo(json);
            Request.Instance.SendDataToPool(json);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if(player.HealthController != null && player.HealthController.IsAlive)
            {
                Logger.LogDebug("Replicated: Calling RestoreBodyPart");
                if(dict == null)
                {
                    Logger.LogError($"Dictionary packet is null?");
                    return;

                }
                Logger.LogInfo(dict.ToJson());

                RestoreBodyPartPacket restoreBodyPartPacket = Json.Deserialize<RestoreBodyPartPacket>(dict.ToJson());
                //phc.RestoreBodyPart(restoreBodyPartPacket.BodyPart, restoreBodyPartPacket.HealthPenalty);
                var bodyPartDict = GetBodyPartDictionary(player);
                /*
                 * protected sealed class BodyPartState
	                {
		                public bool IsDestroyed;

		                public HealthValue Health;
	                }
                 */
                var state = bodyPartDict[restoreBodyPartPacket.BodyPart];
                if (state == null)
                {
                    Logger.LogError($"Could not retreive {player.ProfileId}'s Health State for Body Part {restoreBodyPartPacket.BodyPart.ToString()}");
                    return;
                }
                Logger.LogInfo(state.ToJson());

                var health = ReflectionHelpers.GetFieldOrPropertyFromInstance<HealthValue>(state, "Health", false);
                if (health == null)
                {
                    Logger.LogError($"Could not retreive {player.ProfileId}'s Body Part {restoreBodyPartPacket.BodyPart.ToString()} Health");
                    return;
                }
                ReflectionHelpers.SetFieldOrPropertyFromInstance(state, "IsDestroyed", false);
                health.Current = 1;
                ReflectionHelpers.SetFieldOrPropertyFromInstance(state, "Health", health);

            }


        }

        private IReadOnlyDictionary<EBodyPart, object> GetBodyPartDictionary(EFT.Player player)
        {
            var bodyPartDict = ReflectionHelpers.GetFieldOrPropertyFromInstance<IReadOnlyDictionary<EBodyPart, object>>(player.HealthController, "IReadOnlyDictionary_0", true);
            if (bodyPartDict == null)
            {
                Logger.LogError($"Could not retreive {player.Id}'s Health State Dictionary");
                return null;
            }
            Logger.LogInfo(bodyPartDict.ToJson());
            return bodyPartDict;
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
    }
}
