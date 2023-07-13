using EFT;
using Newtonsoft.Json;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Core.SP.PlayerPatches.Health
{
    public class OnDeadPatch : ModulePatch
    {
        public static event Action<Player, EDamageType> OnPersonKilled;
        public static bool DisplayDeathMessage = true;

        public OnDeadPatch(BepInEx.Configuration.ConfigFile config)
        {
            var enableDeathMessage = config.Bind("SIT", "Enable Death Message", true);
            if (enableDeathMessage != null && enableDeathMessage.Value == true)
            {
                DisplayDeathMessage = enableDeathMessage.Value;

            }
        }

        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(typeof(Player), "OnDead");

        [PatchPostfix]
        public static void PatchPostfix(Player __instance, EDamageType damageType)
        {
            Player deadPlayer = __instance;
            if (deadPlayer == null)
                return;

            if (OnPersonKilled != null)
            {
                OnPersonKilled(__instance, damageType);
            }

            var killedBy = ReflectionHelpers.GetFieldOrPropertyFromInstance<Player>(deadPlayer, "LastAggressor", false);
            if (killedBy == null)
                return;

            var killedByLastAggressor = ReflectionHelpers.GetFieldOrPropertyFromInstance<Player>(killedBy, "LastAggressor", false);
            if (killedByLastAggressor == null)
                return;

            if (DisplayDeathMessage)
            {
                if (killedBy != null)
                    DisplayMessageNotifications.DisplayMessageNotification($"{killedBy.Profile.Info.Nickname} killed {deadPlayer.Profile.Nickname}");
                else
                    DisplayMessageNotifications.DisplayMessageNotification($"{deadPlayer.Profile.Nickname} has died by {damageType}");
            }

            Dictionary<string, object> packet = new()
            {
                { "diedAID", __instance.Profile.AccountId }
            };
            if (__instance.Profile.Info != null)
            {
                packet.Add("diedFaction", __instance.Side);
                if (__instance.Profile.Info.Settings != null)
                    packet.Add("diedWST", __instance.Profile.Info.Settings.Role);
            }
            if (killedBy != null)
            {
                packet.Add("killedByAID", killedBy.Profile.AccountId);
                packet.Add("killerFaction", killedBy.Side);
            }
            if (killedByLastAggressor != null)
            {
                packet.Add("killedByLastAggressorAID", killedByLastAggressor.Profile.AccountId);
            }
            AkiBackendCommunication.Instance.PostJsonAndForgetAsync("/client/raid/person/killed", JsonConvert.SerializeObject(packet));
        }
    }
}
