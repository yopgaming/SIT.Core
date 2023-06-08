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
            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            Logger.LogDebug("RestoreBodyPartPatch:PrePatch");

            return result;
        }

        [PatchPostfix]
        public static void PatchPostfix(
            PlayerHealthController __instance
            , EBodyPart bodyPart
            , float healthPenalty
            )
        {
            var player = __instance.Player; // ReflectionHelpers.GetFieldOrPropertyFromInstance<EFT.Player>(__instance, "Player", false);

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }



            RestoreBodyPartPacket restoreBodyPartPacket = new RestoreBodyPartPacket();
            restoreBodyPartPacket.BodyPart = bodyPart;
            restoreBodyPartPacket.Time = DateTime.Now.Ticks;
            var json = restoreBodyPartPacket.ToJson();
            Request.Instance.SendDataToPool(json);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            if(player.HealthController != null && player.HealthController.IsAlive && player.HealthController is PlayerHealthController phc)
            {
                CallLocally.Add(player.Profile.AccountId, true);
                Logger.LogDebug("Replicated: Calling RestoreBodyPart");
                RestoreBodyPartPacket restoreBodyPartPacket = Json.Deserialize<RestoreBodyPartPacket>(dict.ToJson());
                phc.RestoreBodyPart(restoreBodyPartPacket.BodyPart, restoreBodyPartPacket.HealthPenalty);
            }

         
        }

        public class RestoreBodyPartPacket : BasePlayerPacket
        {
            public EBodyPart BodyPart { get; set; }
            public float HealthPenalty { get; set; }
            public override string Method { get; set; } = "RestoreBodyPart";
        }
    }
}
