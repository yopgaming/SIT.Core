using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using EFT.Bots;
using Newtonsoft.Json;
using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using SIT.Tarkov.Core.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Coop.Core.LocalGame
{
    internal class LocalGameSpawnAICoroutinePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var targetMethod = ReflectionHelpers.GetAllMethodsForType(typeof(EFT.LocalGame))
                .LastOrDefault(
                m =>
                !m.IsVirtual
                && m.GetParameters().Length >= 4
                && m.GetParameters()[0].ParameterType == typeof(float)
                && m.GetParameters()[0].Name == "startDelay"
                && m.GetParameters()[1].Name == "controllerSettings"
                );

            return targetMethod;
        }

        [PatchPrefix]
        public static bool PatchPrefix(ref BotControllerSettings controllerSettings)
        {
            Logger.LogInfo($"LocalGameSpawnAICoroutinePatch:PatchPrefix");

            if (controllerSettings.BotAmount != EBotAmount.NoBots)
                controllerSettings.BotAmount = EBotAmount.Low; // Keep bots levels low for Coop, reduce performance lag

            return true;
        }

        [PatchPostfix]
        public static IEnumerator PatchPostfix(
            IEnumerator __result,
            EFT.LocalGame __instance,
            ISpawnSystem spawnSystem,
            Callback runCallback,
            WavesSpawnScenario ___wavesSpawnScenario_0,
            NonWavesSpawnScenario ___nonWavesSpawnScenario_0
        )
        {
            Logger.LogInfo($"LocalGameSpawnAICoroutinePatch:PatchPostfix");

            yield return __result;
        }

            //        //    // Run normally.
            //        //    if (!Matchmaker.MatchmakerAcceptPatches.IsClient)
            //        //    {

            //        //        // Wait for client to join 
            //        //        // comment out to test general stuff
            //        //        var waitForClients = _config.Bind("Server", "Wait for Clients", true).Value;
            //        //        if (waitForClients && Matchmaker.MatchmakerAcceptPatches.IsServer)
            //        //        {
            //        //            if (CoopGameComponent.Players.Any())
            //        //            {
            //        //                var filteredPlayersOnlyCount = CoopGameComponent.Players.Count(x => !x.Value.IsAI);
            //        //                while (filteredPlayersOnlyCount < Matchmaker.MatchmakerAcceptPatches.HostExpectedNumberOfPlayers)
            //        //                {
            //        //                    filteredPlayersOnlyCount = CoopGameComponent.Players.Count(x => !x.Value.IsAI);
            //        //                    yield return new WaitForSeconds(1);
            //        //                }
            //        //            }
            //        //        }


            //        //        //Logger.LogInfo($"BotSystemHelpers.RoleLimitDifficultyType.Name_0:{BotSystemHelpers.RoleLimitDifficultyType.Name + "_0"}");
            //        //        //CoopGameComponent.Players.Clear();
            //        //        var nonSpawnWaveShit = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(
            //        //            LocalGamePatches.LocalGameInstance
            //        //            //, BotSystemHelpers.RoleLimitDifficultyType.Name + "_0"
            //        //            , "nonWavesSpawnScenario_0"
            //        //            , false);
            //        //        //if (nonSpawnWaveShit != null)
            //        //        {
            //        //            Logger.LogInfo($"nonSpawnWaveShit:{nonSpawnWaveShit.GetType().Name}");

            //        //            // this doesn't work because the gclass name isn't the same as the member name, need a work around
            //        //            //var openZones = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(LocalGamePatches.LocalGameInstance, BotSystemHelpers.LocationBaseType.Name + "_0", false);
            //        //            var openZones = LocationSettings;// ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(__instance, "GClass1113_0", false);
            //        //            var openZones2 = ReflectionHelpers.GetFieldOrPropertyFromInstance<string>(openZones, "OpenZones", false);

            //        //            //Logger.LogInfo($"BotSystemHelpers.ProfileCreatorType:{BotSystemHelpers.ProfileCreatorType.Name}");
            //        //            // Construct Profile Creator
            //        //            var profileCreator = new BotPresetFactory1(PatchConstants.BackEndSession, ___wavesSpawnScenario_0.SpawnWaves, (BossLocationSpawn[])BossSpawnerWaves, null, false);

            //        //            BotCreator1 botCreator = new BotCreator1(__instance, profileCreator, BotCreationMethod);

            //        //            BotZone[] botZones = LocationScene.GetAllObjects<BotZone>().ToArray();
            //        //            bool enableWaveControl = true;

            //        //            __instance.BotsController.Init(
            //        //                __instance
            //        //                , botCreator
            //        //                , botZones
            //        //                , spawnSystem
            //        //                , ___wavesSpawnScenario_0.BotLocationModifier
            //        //                , true
            //        //                , false
            //        //                , enableWaveControl
            //        //                , false
            //        //                , false
            //        //                , Singleton<GameWorld>.Instance
            //        //                , openZones2);

            //        //            var AICountOverride = _config.Bind("Server", "Override Number of AI", false).Value;
            //        //            var NumberOfAI = _config.Bind("Server", "Number of AI", 15).Value;
            //        //            if (AICountOverride)
            //        //            {
            //        //                maxCountOfBots = NumberOfAI;
            //        //            }
            //        //            var backendConfig = PatchConstants.BackEndSession.BackEndConfig;
            //        //            var botPresets = backendConfig.BotPresets;
            //        //            var botWeaponScatterings = backendConfig.BotWeaponScatterings;

            //        //            Logger.LogInfo($"Max Number of Bots:{maxCountOfBots}");
            //        //            __instance.BotsController.SetSettings(maxCountOfBots, botPresets, botWeaponScatterings);
            //        //            //BotSystemHelpers.SetSettings(maxCountOfBots, botPresets, botWeaponScatterings);
            //        //            var AIIgnorePlayers = _config.Bind("Server", "AI Ignore Players", false).Value;
            //        //            if (!AIIgnorePlayers)
            //        //            {
            //        //                //var gparam = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(LocalGamePatches.LocalGameInstance, "gparam_0", false);
            //        //                //var player = ReflectionHelpers.GetFieldOrPropertyFromInstance<EFT.Player>(gparam, "Player", false);
            //        //                //BotSystemHelpers.AddActivePlayer(player);
            //        //                __instance.BotsController.AddActivePLayer(__instance.AllPlayers[0]);
            //        //            }
            //        //            yield return new WaitForSeconds(1);

            //        //            var EnableAISpawnWaveSystem = _config.Bind("Server", "Enable AI Spawn Wave System", true
            //        //                , new ConfigDescription("Whether to run the Wave Spawner System. Useful for testing.")).Value;
            //        //            if (EnableAISpawnWaveSystem)
            //        //            {
            //        //                if (___wavesSpawnScenario_0.SpawnWaves != null && ___wavesSpawnScenario_0.SpawnWaves.Length != 0)
            //        //                {
            //        //                    ___wavesSpawnScenario_0.Run();
            //        //                }
            //        //                else
            //        //                {
            //        //                    ___nonWavesSpawnScenario_0.Run();
            //        //                }

            //        //                ReflectionHelpers.GetMethodForType(BossSpawner.GetType(), "Run").Invoke(BossSpawner, new object[] { EBotsSpawnMode.Anyway });
            //        //            }
            //        //            yield return new WaitForSeconds(3);

            //        //            using (PatchConstants.StartWithToken("SessionRun"))
            //        //            {
            //        //                // TODO: This needs removing!
            //        //                ReflectionHelpers.GetMethodForType(LocalGamePatches.LocalGameInstance.GetType(), "vmethod_4").Invoke(LocalGamePatches.LocalGameInstance, new object[0]);
            //        //            }
            //        //            yield return new WaitForSeconds(1);


            //        //        }

            //        //        runCallback.Succeed();
            //        //    }
            //        //    else 
            //        //    {
            //        //        BotSystemHelpers.SetSettingsNoBots();
            //        //        try
            //        //        {
            //        //            BotSystemHelpers.Stop();
            //        //        }
            //        //        catch
            //        //        {

            //        //        }
            //        //        yield return new WaitForSeconds(1);
            //        //        using (PatchConstants.StartWithToken("SessionRun"))
            //        //        {
            //        //            // TODO: This needs removing!
            //        //            ReflectionHelpers.GetMethodForType(LocalGamePatches.LocalGameInstance.GetType(), "vmethod_4").Invoke(LocalGamePatches.LocalGameInstance, new object[0]);
            //        //        }
            //        //        runCallback.Succeed();
            //        //    }
            //        //}


            //        private static MethodInfo method_8 = null;

            //        /// <summary>
            //        /// 
            //        /// </summary>
            //        /// <param name="profile"></param>
            //        /// <param name="position"></param>
            //        /// <returns></returns>
            //        public static async Task<LocalPlayer> BotCreationMethod(Profile profile, Vector3 position)
            //        {
            //            // If its Client. Out!
            //            if (Matchmaker.MatchmakerAcceptPatches.IsClient)
            //                return null;

            //            // If there is no profile. Out!
            //            if (profile == null)
            //                return null;

            //            if (CoopGameComponent.Players.Count >= maxCountOfBots) 
            //            {
            //                //Logger.LogInfo($"BotCreationMethod. [ERROR] CoopGameComponent.Players is full");
            //                return null;
            //            }

            //            if (CoopGameComponent.Players.ContainsKey(profile.AccountId))
            //            {
            //                //Logger.LogInfo($"BotCreationMethod. [ERROR] Bot already exists");
            //                return null;
            //            }

            //            // TODO: Rewrite the following method into BotCreationMethod
            //            if (method_8 == null)
            //                method_8 = ReflectionHelpers.GetMethodForType(LocalGamePatches.LocalGameInstance.GetType().BaseType, "method_8");

            //            var player = await (Task<LocalPlayer>)method_8
            //            .Invoke(LocalGamePatches.LocalGameInstance, new object[] { profile, position });

            //            if (Matchmaker.MatchmakerAcceptPatches.IsServer)
            //            {
            //                var prc = player.GetOrAddComponent<PlayerReplicatedComponent>();
            //                prc.player = player;
            //                CoopGameComponent.Players.TryAdd(profile.AccountId, player);
            //                Dictionary<string, object> dictionary2 = new Dictionary<string, object>
            //                    {
            //                        {
            //                            "SERVER",
            //                            "SERVER"
            //                        },
            //                        {
            //                            "isAI",
            //                            true
            //                        },
            //                        {
            //                            "accountId",
            //                            player.Profile.AccountId
            //                        },
            //                        {
            //                            "profileId",
            //                            player.ProfileId
            //                        },
            //                        {
            //                            "groupId",
            //                            Matchmaker.MatchmakerAcceptPatches.GetGroupId()
            //                        },
            //                        {
            //                            "sPx",
            //                            position.x
            //                        },
            //                        {
            //                            "sPy",
            //                            position.y
            //                        },
            //                        {
            //                            "sPz",
            //                            position.z
            //                        },
            //                        { "m", "PlayerSpawn" },
            //                        {
            //                            "p.info",
            //                            JsonConvert.SerializeObject(player.Profile.Info
            //                                , Formatting.None 
            //                                , new JsonSerializerSettings() { })//.SITToJson()
            //                        },
            //                        {
            //                            "p.cust",
            //                             player.Profile.Customization.SITToJson()
            //                        },
            //                        {
            //                            "p.equip",
            //                            //player.Profile.Inventory.Equipment.CloneItem().ToJson()
            //                            player.Profile.Inventory.Equipment.SITToJson()
            //                        }
            //                    };
            //                //Request.Instance.PostJson("/client/match/group/server/players/spawn", dictionary2.ToJson());
            //                await ServerCommunication.PostLocalPlayerDataAsync(player, dictionary2);
            //                //Logger.LogInfo($"BotCreationMethod. [SUCCESS] Adding AI {profile.AccountId} to CoopGameComponent.Players list");
            //            }
            //            return player;
            //        }
        }
}
