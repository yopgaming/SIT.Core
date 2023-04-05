using EFT;
using EFT.Interactive;
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class Player_InteractWithDoor_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "Door";

        public MethodInfo Method { get; set; } = null;

        /// <summary>
        /// Targetting vmethod_1
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod()
        {
            Method = ReflectionHelpers.GetAllMethodsForType(InstanceType)
                .FirstOrDefault(x => x.GetParameters().Length == 2
                && x.GetParameters()[0].Name == "door"
                && x.GetParameters()[1].Name == "interactionResult"
                );

            //Logger.LogInfo($"OnInteractWithDoorPatch:{InstanceType.Name}:{method.Name}");
            return Method;
        }

        public static Dictionary<string, bool> CallLocally
          = new Dictionary<string, bool>();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            //Logger.LogInfo("PlayerOnInteractWithDoorPatch:PrePatch");

            return result;
        }

        [PatchPostfix]
        public static void Patch(
                EFT.Player __instance,
                WorldInteractiveObject door
                , InteractionResult interactionResult
                )
        {
            //if (Matchmaker.MatchmakerAcceptPatches.IsSinglePlayer)
            //    return;

            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(__instance.Profile.AccountId);
                return;
            }

            //Logger.LogInfo("OnInteractWithDoorPatch.PatchPostfix");
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("doorId", door.Id);
            dictionary.Add("type", interactionResult.InteractionType.ToString());
            dictionary.Add("m", "Door");
            ServerCommunication.PostLocalPlayerData(__instance, dictionary);
            //Logger.LogInfo("OnInteractWithDoorPatch.PatchPostfix:Sent");
        }

        private static List<long> ProcessedCalls = new List<long>();


        public override void Replicated(EFT.Player player, Dictionary<string, object> packet)
        {
            var timestamp = long.Parse(packet["t"].ToString());
            if ((DateTime.Now - new DateTime(timestamp)).TotalSeconds > 10)
                return;

            if (!ProcessedCalls.Contains(timestamp))
                ProcessedCalls.Add(timestamp);
            else
            {
                ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddHours(-1).Ticks);
                return;
            }

            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC == null)
                return;

            try
            {
                Enum.TryParse<EInteractionType>(packet["type"].ToString(), out EInteractionType interactionType);

                var foundDoor = coopGC.ListOfInteractiveObjects
                    .FirstOrDefault(
                    x => x.Id == packet["doorId"].ToString());
                if (foundDoor == null)
                    return;

                CallLocally.Add(player.Profile.AccountId, true);
                //Logger.LogInfo("Replicated: Calling Door Opener");
                Method.Invoke(player, new object[] { foundDoor, new InteractionResult(interactionType) });
            }
            catch (Exception)
            {

            }

        }
    }
}
