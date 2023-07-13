using SIT.Coop.Core.Player;
using SIT.Core.Coop.NetworkPacket;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    public class FirearmController_SetTriggerPressed_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "SetTriggerPressed";

        //public override bool DisablePatch => true;

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public override void Enable()
        {
            base.Enable();
            LastPress.Clear();
        }

        public static Dictionary<string, bool> CallLocally = new();

        public static Dictionary<string, bool> LastPress = new();


        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player.FirearmController __instance
            , EFT.Player ____player
            , bool pressed
            )
        {
            if (CoopGameComponent.GetCoopGameComponent() == null)
                return false;

            if (CoopGameComponent.GetCoopGameComponent().HighPingMode && ____player.IsYourPlayer)
            {
                return true;
            }

            var player = ____player;
            if (player == null)
                return false;

            var result = false;
            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
            EFT.Player.FirearmController __instance
            , bool pressed
            , EFT.Player ____player
            )
        {
            var player = ____player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            if (player.TryGetComponent<PlayerReplicatedComponent>(out var prc) && prc.IsClientDrone)
                return;

            // Handle LastPress
            if (LastPress.ContainsKey(player.ProfileId) && LastPress[player.ProfileId] == pressed)
                return;

            if (!LastPress.ContainsKey(player.ProfileId))
                LastPress.Add(player.ProfileId, pressed);

            LastPress[player.ProfileId] = pressed;

            TriggerPressedPacket triggerPressedPacket = new();
            triggerPressedPacket.AccountId = player.Profile.AccountId;
            triggerPressedPacket.pr = pressed;
            if (pressed)
            {
                triggerPressedPacket.rX = player.Rotation.x;
                triggerPressedPacket.rY = player.Rotation.y;
            }
            var serialized = triggerPressedPacket.Serialize();
            AkiBackendCommunication.Instance.SendDataToPool(serialized);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (CoopGameComponent.GetCoopGameComponent().HighPingMode && player.IsYourPlayer)
            {
                // You would have already run this. Don't bother
                return;
            }

            if (!player.PlayerHealthController.IsAlive)
            {
                if (player.HandsController is EFT.Player.FirearmController firearmCont)
                {
                    firearmCont.SetTriggerPressed(false);
                }
                return;
            }

            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            taskScheduler.Do((s) =>
            {
                TriggerPressedPacket tpp = new();

                //Logger.LogInfo("Pressed:Replicated");
                if (!dict.ContainsKey("data"))
                    return;

                tpp = tpp.DeserializePacketSIT(dict["data"].ToString());

                if (HasProcessed(GetType(), player, tpp))
                    return;

                if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                    return;

                if (CallLocally.ContainsKey(player.Profile.AccountId))
                    return;

                CallLocally.Add(player.Profile.AccountId, true);

                bool pressed = tpp.pr; // bool.Parse(dict["pr"].ToString());

                if (player.HandsController is EFT.Player.FirearmController firearmCont)
                {
                    try
                    {
                        firearmCont.SetTriggerPressed(pressed);
                        //if (pressed && dict.ContainsKey("rX"))
                        if (prc.IsClientDrone && pressed && tpp.rX != 0)
                        {
                            var rotat = new Vector2(tpp.rX, tpp.rY);
                            player.Rotation = rotat;
                        }

                        //ReplicatedShotEffects(player, pressed);


                    }
                    catch (Exception e)
                    {
                        Logger.LogInfo(e);
                    }
                }
            });
        }

        public class TriggerPressedPacket : BasePlayerPacket
        {
            public bool pr { get; set; }
            public float rX { get; set; }
            public float rY { get; set; }

            public TriggerPressedPacket() : base()
            {
                Method = "SetTriggerPressed";
            }

        }

    }
}
