using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;

namespace SIT.Core.SP.Raid
{
    internal class WavesSpawnScenarioInitPatch : ModulePatch
    {
        private static Random Random = new Random();

        private static EFT.WavesSpawnScenario CurrentInstance;

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(EFT.WavesSpawnScenario), "Init");
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.WavesSpawnScenario __instance, WildSpawnWave[] waves)
        {
            CurrentInstance = __instance;

            ReflectionHelpers.SetFieldOrPropertyFromInstance(__instance, "Enabled", true);
            SpawnWaves[] spawnWaves;
            if (waves != null)
            {
                spawnWaves = waves.Select(new Func<WildSpawnWave, SpawnWaves>(method_1)).ToArray();
            }
            else
            {
                spawnWaves = new SpawnWaves[0];
            }
            ReflectionHelpers.SetFieldOrPropertyFromInstance(__instance, "SpawnWaves", spawnWaves);
            return false;
        }


        private static SpawnWaves method_1(WildSpawnWave wave)
        {
            if (Random == null)
                Random = new Random();

            int botsCount = Random.Next(wave.slots_min, wave.slots_max);
            SpawnWaves spawnWaves = new SpawnWaves
            {
                Time = (float)UnityEngine.Random.Range(wave.time_min, wave.time_max),
                BotsCount = botsCount,
                Difficulty = wave.GetDifficulty(),
                WildSpawnType = wave.WildSpawnType,
                SpawnAreaName = wave.SpawnPoints,
                Side = wave.BotSide,
                IsPlayers = wave.isPlayers,
                ChanceGroup = wave.ChanceGroup
            };
            WaveInfo item = new WaveInfo(spawnWaves.BotsCount, spawnWaves.WildSpawnType, spawnWaves.Difficulty);
            CurrentInstance.BotsCountProfiles.Add(item);
            return spawnWaves;
        }



    }
}
