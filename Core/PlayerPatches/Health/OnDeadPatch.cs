using EFT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SIT.Tarkov.Core.PlayerPatches.Health
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

        protected override MethodBase GetTargetMethod() => PatchConstants.GetMethodForType(typeof(Player), "OnDead");

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

            var killedBy = PatchConstants.GetFieldOrPropertyFromInstance<Player>(deadPlayer, "LastAggressor", false);
            if (killedBy == null)
                return;

            var killedByLastAggressor = PatchConstants.GetFieldOrPropertyFromInstance<Player>(killedBy, "LastAggressor", false);
            if (killedByLastAggressor == null)
                return;

            if (DisplayDeathMessage)
            {
                if (killedBy != null)
                    PatchConstants.DisplayMessageNotification($"{killedBy.Profile.Info.Nickname} killed {deadPlayer.Profile.Nickname}");
                else
                    PatchConstants.DisplayMessageNotification($"{deadPlayer.Profile.Nickname} has died by {damageType}");
            }

            Dictionary<string, object> map = new Dictionary<string, object>
            {
                { "diedAID", __instance.Profile.AccountId }
            };
            if (__instance.Profile.Info != null)
            {
                map.Add("diedFaction", __instance.Side);
                if (__instance.Profile.Info.Settings != null)
                    map.Add("diedWST", __instance.Profile.Info.Settings.Role);
            }
            if (killedBy != null)
            {
                map.Add("killedByAID", killedBy.Profile.AccountId);
                map.Add("killerFaction", killedBy.Side);
            }
            if (killedByLastAggressor != null)
            {
                map.Add("killedByLastAggressorAID", killedByLastAggressor.Profile.AccountId);
            }
            _ = new Request().PostJsonAsync("/client/raid/person/killed", JsonConvert.SerializeObject(map));
        }
    }
}
