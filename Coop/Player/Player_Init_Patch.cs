using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class Player_Init_Patch : ModulePatch
    {
        private static ConfigFile _config;
        public Player_Init_Patch(ConfigFile config)
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
        }

        public static void SendPlayerDataToServer(EFT.LocalPlayer player)
        {
            Dictionary<string, object> packet = new Dictionary<string, object>
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
                        {
                            "p.info",
                            JsonConvert.SerializeObject(player.Profile.Info
                                , Formatting.None
                                , new JsonSerializerSettings() { })//.SITToJson()
                        },
                        {
                            "p.cust",
                             player.Profile.Customization.ToJson()
                        },
                        {
                            "p.equip",
                            player.Profile.Inventory.Equipment.SITToJson()
                        },
                        {
                            "side",
                            player.Profile.Side.ToString()
                        },
                        { "m", "PlayerSpawn" },
                    };


            //Logger.LogDebug(packet.ToJson());

            var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
            prc.player = player;
            ServerCommunication.PostLocalPlayerData(player, packet);
        }

        public static void SendOrReceiveSpawnPoint(EFT.Player player)
        {
            var position = player.Transform.position;
            if (!Matchmaker.MatchmakerAcceptPatches.IsClient)
            {
                Dictionary<string, object> value2 = new Dictionary<string, object>
                {
                    {
                        "m",
                        "SpawnPointForCoop"
                    },
                    {
                        "playersSpawnPointx",
                        position.x
                    },
                    {
                        "playersSpawnPointy",
                        position.y
                    },
                    {
                        "playersSpawnPointz",
                        position.z
                    }
                };
                Logger.LogInfo("Setting Spawn Point to " + position);
                ServerCommunication.PostLocalPlayerData(player, value2);
            }
            else
            {

            }
        }
    }
}
