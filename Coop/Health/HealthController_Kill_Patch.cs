using EFT;
using EFT.InventoryLogic;
using SIT.Coop.Core.Matchmaker;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Health
{
    public class HealthController_Kill_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(AbstractActiveHealthController);

        public override string MethodName => "Kill";

     

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if(MatchmakerAcceptPatches.IsServer)
            {
                Request.Instance.PostDownWebSocketImmediately(new Dictionary<string, object>() {

                    { "serverId", CoopGameComponent.GetServerId() }
                    , { "m", "HostDied" }
                
                });
            }
            //Logger.LogDebug("HealthController_Kill_Patch:Prefix");

            //return !(MatchmakerAcceptPatches.IsClient);
            return true;
        }

        [PatchPostfix]
        public static void PostPatch(
          AbstractActiveHealthController __instance
           )
        {
            //Logger.LogDebug("HealthController_Kill_Patch:PostPatch");
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            //Logger.LogDebug("HealthController_Kill_Patch:Replicated");

            if (Enum.TryParse<EDamageType>(dict["dmt"].ToString(), out var damageType)) 
            {
                player.ActiveHealthController.Kill(damageType);
            }
        }
    }
}
