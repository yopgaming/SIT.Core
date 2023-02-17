using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SIT.Coop.Core.Web;
using SIT.Tarkov.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_SetTriggerPressed_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "SetTriggerPressed";

        protected override MethodBase GetTargetMethod()
        {
            var method = PatchConstants.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public Dictionary<string, bool> CallLocally
            = new Dictionary<string, bool>();


        [PatchPrefix]
        public bool PrePatch(EFT.Player.FirearmController __instance)
        {
            var player = PatchConstants.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;
           
            return result;
        }

        [PatchPrefix]
        public void PostPatch(
            EFT.Player.FirearmController __instance
            , bool pressed
            )
        {
            var player = PatchConstants.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            dictionary.Add("pr", pressed.ToString());
            dictionary.Add("m", MethodName);
            ServerCommunication.PostLocalPlayerData(player, dictionary);
        }

        private static List<long> ProcessedCalls = new List<long>();

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());
            if (!ProcessedCalls.Contains(timestamp))
                ProcessedCalls.Add(timestamp);
            else
            {
                ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddMinutes(-5).Ticks);
                return;
            }

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    bool pressed = bool.Parse(dict["pr"].ToString());
                    CallLocally.Add(player.Profile.AccountId, true);
                    firearmCont.SetTriggerPressed(pressed);
                }
                catch(Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
        }
    }
}
