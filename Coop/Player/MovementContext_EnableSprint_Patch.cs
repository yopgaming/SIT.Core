using EFT;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.Player
{
    internal class MovementContext_EnableSprint_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(MovementContext);

        public override string MethodName => "EnableSprint";

        public override void Enable()
        {
            base.Enable();
            Plugin.Instance.StartCoroutine(ContinueSprintingCoroutine());
        }

        public override void Disable()
        {
            base.Disable();
            Plugin.Instance.StopCoroutine(ContinueSprintingCoroutine());
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("data"))
                return;

            EnableSprintPacket enableSprintPacket = new EnableSprintPacket();
            enableSprintPacket.DeserializePacketSIT(dict["data"].ToString());

            if (HasProcessed(InstanceType, player, enableSprintPacket))
                return;

            if (!DesiredReplicatedSprint.ContainsKey(player.Profile.AccountId))
                DesiredReplicatedSprint.Add(player.Profile.AccountId, enableSprintPacket.Enable);

            DesiredReplicatedSprint[player.Profile.AccountId] = enableSprintPacket.Enable;  

            //GetLogger(typeof(MovementContext_EnableSprint_Patch)).LogInfo($"Replicated:Enable:{enableSprintPacket.Enable}");
            //player.Physical.Sprint(enableSprintPacket.Enable);

        }

        public Dictionary<string, bool> DesiredReplicatedSprint = new();

        public IEnumerator ContinueSprintingCoroutine()
        {
            while (true)
            {
                if (CoopGameComponent.TryGetCoopGameComponent(out var gameComponent))
                {
                    foreach (var kvp in DesiredReplicatedSprint)
                    {
                        gameComponent.Players[kvp.Key].Physical.Sprint(kvp.Value);
                    }
                }
                yield return new WaitForFixedUpdate();

            }
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName, findFirst: true);
        }

        public static Dictionary<string, bool> Last = new();

        [PatchPrefix]
        public static bool Prefix(ref bool enable, EFT.Player ___player_0)
        {
            if (Last.ContainsKey(___player_0.ProfileId) && enable == Last[___player_0.ProfileId])
            {
                ___player_0.Physical.Sprint(enable);
                return false;
            }

            if(!Last.ContainsKey(___player_0.ProfileId))
                Last.Add(___player_0.ProfileId, enable);

            Last[___player_0.ProfileId] = enable;
            //GetLogger(typeof(MovementContext_EnableSprint_Patch)).LogInfo($"Prefix:Enable:{enable}");
            ___player_0.Physical.Sprint(enable);
            EnableSprintPacket enableSprintPacket = new EnableSprintPacket(___player_0.AccountId, "EnableSprint", enable);
            var serialized = enableSprintPacket.Serialize();
            AkiBackendCommunication.Instance.SendDataToPool(serialized);

            return false;
        }

        private class EnableSprintPacket : BasePlayerPacket
        {
            public bool Enable { get; set; }

            public EnableSprintPacket() { }
            
            public EnableSprintPacket(string accountId, string method, bool enable) : base(accountId, method)
            {
                Enable = enable;
            }
        }
    }
}
