using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Web;
using SIT.Core.Configuration;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class Player_Init_Coop_Patch : ModulePatch
    {
        private static ConfigFile _config;
        public Player_Init_Coop_Patch(ConfigFile config)
        {
            _config = config;
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(EFT.LocalPlayer), "Init");
        }

        [PatchPostfix]
        public static void PatchPostfix(EFT.LocalPlayer __instance)
        {
            if (__instance is HideoutPlayer)
                return;

            var player = __instance;
            var accountId = player.Profile.AccountId;

            //await __result;
            //Logger.LogInfo($"{nameof(EFT.LocalPlayer)}.Init:{accountId}:IsAi={player.IsAI}");


            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC == null)
            {
                Logger.LogError("Cannot add player to Coop Game Component because its NULL");
                return;
            }


            if (Singleton<GameWorld>.Instance != null)
            {
                if (!coopGC.Players.ContainsKey(accountId))
                    coopGC.Players.Add(accountId, player);

                if (!Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.Profile.AccountId == accountId))
                    Singleton<GameWorld>.Instance.RegisterPlayer(player);
            }
            else
            {
                Logger.LogError("Cannot add player because GameWorld is NULL");
                return;
            }

            SendPlayerDataToServer(player);

            if (PluginConfigSettings.Instance.CoopSettings.SETTING_ShowFeed)
                DisplayMessageNotifications.DisplayMessageNotification($"{__instance.Profile.Nickname}[{__instance.Side}][{__instance.Profile.Info.Settings.Role}] has spawned");

        }

        public static void SendPlayerDataToServer(EFT.LocalPlayer player)
        {
            var profileJson = player.Profile.SITToJson();


            Dictionary<string, object> packet = new()
            {
                        {
                            "serverId",
                            MatchmakerAcceptPatches.GetGroupId()
                        },
                        {
                        "isAI",
                            player.IsAI || !player.Profile.Id.StartsWith("pmc")
                        },
                        {
                            "accountId",
                            player.Profile.AccountId
                        },
                        {
                            "profileId",
                            player.ProfileId
                        },
                        {
                            "groupId",
                            Matchmaker.MatchmakerAcceptPatches.GetGroupId()
                        },
                        {
                            "sPx",
                            player.Transform.position.x
                        },
                        {
                            "sPy",
                            player.Transform.position.y
                        },
                        {
                            "sPz",
                            player.Transform.position.z
                        },
                        //{
                        //    "p.info",
                        //    JsonConvert.SerializeObject(player.Profile.Info
                        //        , Formatting.None
                        //        , new JsonSerializerSettings() { })//.SITToJson()
                        //},
                        //{
                        //    "p.cust",
                        //     player.Profile.Customization.ToJson()
                        //},
                        //{
                        //    "p.equip",
                        //    player.Profile.Inventory.Equipment.SITToJson()
                        //},
                        //{
                        //    "side",
                        //    player.Profile.Side.ToString()
                        //},
                        {
                            "profileJson",
                            profileJson
                        },
                        { "m", "PlayerSpawn" },
                    };


            //Logger.LogDebug(packet.ToJson());

            var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
            prc.player = player;
            AkiBackendCommunicationCoopHelpers.PostLocalPlayerData(player, packet);
        }
    }
}
