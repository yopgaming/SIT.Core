//using EFT;
//using SIT.Coop.Core.Matchmaker;
//using SIT.Coop.Core.Player;
//using SIT.Coop.Core.Web;
//using SIT.Core.Coop.NetworkPacket;
//using SIT.Core.Misc;
//using SIT.Core.SP.PlayerPatches.Health;
//using SIT.Tarkov.Core;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Reflection;
//using static AbstractActiveHealthController;

//namespace SIT.Core.Coop.Player.Health
//{
//    internal class ChangeHealthPatch : ModuleReplicationPatch
//    {
//        public override Type InstanceType => typeof(PlayerHealthController);

//        public override string MethodName => "ChangeHealth";

//        private static readonly Dictionary<string, bool> CallingLocally = new();

//        protected override MethodBase GetTargetMethod()
//        {
//            return ReflectionHelpers.GetMethodForType(typeof(PlayerHealthController), MethodName, findFirst: true);

//        }

//        [PatchPrefix]
//        public static bool Prefix(
//            PlayerHealthController __instance
//            , EBodyPart bodyPart
//            , float value
//            )
//        {
//            //var player = __instance.Player;
//            //if (CallingLocally.ContainsKey(player.ProfileId))
//            //{
//            //    Logger.LogDebug("ChangeHealthPatch. Has CallingLocally");
//            //}
//            //else
//            //{
//            //    Logger.LogDebug("ChangeHealthPatch. Doesn't have CallingLocally");

//            //}


//            //return (CallingLocally.ContainsKey(player.ProfileId));
//            return true;
//        }

//        [PatchPostfix]
//        public static void PatchPostfix(
//            PlayerHealthController __instance
//            , EBodyPart bodyPart
//            , float value
//            , DamageInfo damageInfo
//            )
//        {
//            var player = __instance.Player;
//            if(player != null)
//            {

//                // If it is a client Drone, do not resend the packet again!
//                if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
//                {
//                    if (prc.IsClientDrone)
//                        return;
//                }

//                ChangeHealthPacket changeHealthPacket = new ChangeHealthPacket();
//                changeHealthPacket.Value = value;
//                changeHealthPacket.BodyPart = bodyPart;
//                changeHealthPacket.PartValue = __instance.GetBodyPartHealth(bodyPart, true).Current + value;
//                changeHealthPacket.AccountId = player.Profile.AccountId;
//                changeHealthPacket.Method = "ChangeHealth";
//                //changeHealthPacket.Time = DateTime.Now.Ticks;
//                var json = changeHealthPacket.SITToJson();
//                //Logger.LogDebug("Sending");
//                //Logger.LogDebug(json);
//                //Logger.LogDebug("ChangeHealthPatch.PatchPostfix");

//                //Request.Instance.PostDownWebSocketImmediately(json);
//                Request.Instance.SendDataToPool(json);
//            }
//        }

//        public static Dictionary<PlayerHealthController,
//            IReadOnlyDictionary<EBodyPart, AHealthController<AbstractActiveHealthController.AbstractHealthEffect>.BodyPartState>>
//            CachedBodyPartStates = new();

//        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
//        {
//            if (HasProcessed(GetType(), player, dict))
//                return;

//            if (player == null) return;

//            if (player.PlayerHealthController == null)
//                return;

//            if (!player.PlayerHealthController.IsAlive)
//                return;

//            //Logger.LogDebug("ChangeHealthPatch.Replicated");
//            //Logger.LogDebug("Received");
//            //Logger.LogDebug(dict.ToJson());

//            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
//            {
//                // Convert back to ChangeHealthPacket 
//                ChangeHealthPacket changeHealthPacket = Json.Deserialize<ChangeHealthPacket>(dict.ToJson());

//                IReadOnlyDictionary<EBodyPart, AHealthController<AbstractActiveHealthController.AbstractHealthEffect>.BodyPartState> bodyPartStates = null;
//                if (!CachedBodyPartStates.ContainsKey(player.PlayerHealthController))
//                {
//                    Logger.LogInfo($"Adding PHC of {player.ProfileId} to Cached List");
//                    CachedBodyPartStates.Add(player.PlayerHealthController, ReflectionHelpers.GetFieldOrPropertyFromInstance<IReadOnlyDictionary<EBodyPart, AHealthController<AbstractActiveHealthController.AbstractHealthEffect>.BodyPartState>>(player.PlayerHealthController, "IReadOnlyDictionary_0", false));
//                }
//                bodyPartStates = CachedBodyPartStates[player.PlayerHealthController];
//                bodyPartStates[changeHealthPacket.BodyPart].Health.Current = changeHealthPacket.PartValue;

//            }

//        }

//        public class ChangeHealthPacket : BasePlayerPacket
//        {
//            public EBodyPart BodyPart { get; set; }
//            public float Value { get; set; }
//            public float PartValue { get; set; }

//        }
//    }
//}
