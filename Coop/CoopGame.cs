using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.InputSystem;
using EFT.UI;
using EFT.Weather;
using JsonType;
using SIT.Coop.Core.Matchmaker;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EFT.Player;

namespace SIT.Core.Coop
{
    /// <summary>
    /// A custom Game Type
    /// </summary>
    public sealed class CoopGame : BaseLocalGame<GamePlayerOwner>, IBotGame
    {

        // Token: 0x0600844F RID: 33871 RVA: 0x0025D580 File Offset: 0x0025B780
        internal static CoopGame Create(
            InputTree inputTree
            , Profile profile
            , GameDateTime backendDateTime
            , Insurance insurance
            , MenuUI menuUI
            , CommonUI commonUI
            , PreloaderUI preloaderUI
            , GameUI gameUI
            , LocationSettings.Location location
            , TimeAndWeatherSettings timeAndWeather
            , WavesSettings wavesSettings
            , EDateTime dateTime
            , Callback<ExitStatus, TimeSpan, ClientMetrics> callback
            , float fixedDeltaTime
            , EUpdateQueue updateQueue
            , ISession backEndSession
            , TimeSpan sessionTime)
        {
            botControllerClass = null;

            CoopGame coopGame = BaseLocalGame<GamePlayerOwner>
                .smethod_0<CoopGame>(inputTree, profile, backendDateTime, insurance, menuUI, commonUI, preloaderUI, gameUI, location, timeAndWeather, wavesSettings, dateTime
                , callback, fixedDeltaTime, updateQueue, backEndSession, new TimeSpan?(sessionTime));
            WildSpawnWave[] array = CoopGame.smethod_6(wavesSettings, location.waves);
            coopGame.nonWavesSpawnScenario_0 = (NonWavesSpawnScenario)ReflectionHelpers.GetMethodForType(typeof(NonWavesSpawnScenario) ,"smethod_0").Invoke
                (null, new object[] { coopGame, location, coopGame.PBotsController });
            coopGame.wavesSpawnScenario_0 = (WavesSpawnScenario)ReflectionHelpers.GetMethodForType(typeof(WavesSpawnScenario), "smethod_0").Invoke
                (null, new object[] { coopGame.gameObject, array, new Action<Wave>((wave) => coopGame.PBotsController.ActivateBotsByWave(wave)), location });// WavesSpawnScenario.smethod_0(@class.game.gameObject, array, new Action<Wave>(@class.method_0), location);
            BossLocationSpawn[] array2 = CoopGame.smethod_7(wavesSettings, location.BossLocationSpawn);

            var bosswavemanagerValue = ReflectionHelpers.GetMethodForType(typeof(BossWaveManager), "smethod_0").Invoke
                (null, new object[] { array2, new Action<BossLocationSpawn>((bossWave) => { coopGame.PBotsController.ActivateBotsByWave(bossWave); }) });
            ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(CoopGame), typeof(BossWaveManager)).SetValue(coopGame, bosswavemanagerValue);

            coopGame.StartCoroutine(coopGame.ReplicatedWeather());

            return coopGame;
        }

        public IEnumerator ReplicatedWeather()
        {
            var waitSeconds = new WaitForSeconds(15f);

            while (true)
            {
                yield return waitSeconds;
                if(WeatherController.Instance != null)
                    WeatherController.Instance.SetWeatherForce(new WeatherClass() { });
            }
        }

        public ISession BackEndSession { get { return PatchConstants.BackEndSession; } }

        BotControllerClass IBotGame.BotsController
        {
            get
            {
                if (botControllerClass == null)
                {
                    botControllerClass = (BotControllerClass)ReflectionHelpers.GetFieldFromTypeByFieldType(base.GetType(), typeof(BotControllerClass)).GetValue(this);
                }
                return botControllerClass;
            }
        }

        private static BotControllerClass botControllerClass;

        public BotControllerClass PBotsController
        {
            get
            {
                if (botControllerClass == null)
                {
                    botControllerClass = (BotControllerClass)ReflectionHelpers.GetFieldFromTypeByFieldType(base.GetType(), typeof(BotControllerClass)).GetValue(this);
                }
                return botControllerClass;
            }
        }

        public IWeatherCurve WeatherCurve
        {
            get
            {
                if(WeatherController.Instance != null)
                    return WeatherController.Instance.WeatherCurve;

                return null;
            }
        }



        public override IEnumerator vmethod_4(float startDelay, BotControllerSettings controllerSettings, ISpawnSystem spawnSystem, Callback runCallback)
        {
            if(MatchmakerAcceptPatches.IsClient)
            {
                yield return new WaitForSeconds(startDelay);
                yield return this.method_17(startDelay, controllerSettings, spawnSystem, runCallback);
                yield break;
            }

            BotsPresets botsPresets =
                new BotsPresets(BackEndSession
                , this.wavesSpawnScenario_0.SpawnWaves
                , new BossLocationSpawn[1] { new BossLocationSpawn() { Activated = false } }
                , (WaveInfo[])ReflectionHelpers.GetFieldFromTypeByFieldType(this.nonWavesSpawnScenario_0.GetType(), typeof(WaveInfo[])).GetValue(this.nonWavesSpawnScenario_0)
                , false);
            BotCreator botCreator = new BotCreator(this, botsPresets, new Func<Profile, Vector3, Task<LocalPlayer>>(this.method_16));
            BotZone[] array = LocationScene.GetAllObjects<BotZone>(false).ToArray<BotZone>();
            //    bool flag = controllerSettings.BotAmount == EBotAmount.Horde;
            this.PBotsController.Init(this, botCreator, array, spawnSystem, this.wavesSpawnScenario_0.BotLocationModifier, controllerSettings.IsEnabled, controllerSettings.IsScavWars, false, false, false, Singleton<GameWorld>.Instance, base.Location_0.OpenZones);
            //    int num;
            //    switch (controllerSettings.BotAmount)
            //    {
            //        case EBotAmount.AsOnline:
            //            num = 20;
            //            goto IL_12C;
            //        case EBotAmount.Low:
            //            num = 15;
            //            goto IL_12C;
            //        case EBotAmount.Medium:
            //            num = 20;
            //            goto IL_12C;
            //        case EBotAmount.High:
            //            num = 25;
            //            goto IL_12C;
            //        case EBotAmount.Horde:
            //            num = 35;
            //            goto IL_12C;
            //    }
            //    num = 15;
            //IL_12C:
            this.PBotsController.SetSettings(17, this.BackEndSession.BackEndConfig.BotPresets, this.BackEndSession.BackEndConfig.BotWeaponScatterings);
            this.PBotsController.AddActivePLayer(this.gparam_0.Player);
            yield return new WaitForSeconds(startDelay);
            //    if (!base.Location_0.NewSpawn)
            //    {
            if (this.wavesSpawnScenario_0.SpawnWaves != null && this.wavesSpawnScenario_0.SpawnWaves.Length != 0)
            {
                this.wavesSpawnScenario_0.Run(EBotsSpawnMode.Anyway);
            }
            else
            {
                this.nonWavesSpawnScenario_0.Run();
            }
            //    }
            this.BossWaveManager.Run(EBotsSpawnMode.Anyway);
            yield return this.method_17(startDelay, controllerSettings, spawnSystem, runCallback);
            yield break;
        }

        private static WildSpawnWave[] smethod_6(WavesSettings wavesSettings, WildSpawnWave[] waves)
        {
            foreach (WildSpawnWave wildSpawnWave in waves)
            {
                wildSpawnWave.slots_min = (wildSpawnWave.slots_max = wavesSettings.BotAmount.ToBotAmountSlots(wildSpawnWave.slots_min, wildSpawnWave.slots_max));
                if (wavesSettings.IsTaggedAndCursed && wildSpawnWave.WildSpawnType == WildSpawnType.assault)
                {
                    wildSpawnWave.WildSpawnType = WildSpawnType.cursedAssault;
                }
                if (wavesSettings.IsBosses)
                {
                    wildSpawnWave.time_min += 5;
                    wildSpawnWave.time_max += 15;
                }
                wildSpawnWave.BotDifficulty = wavesSettings.BotDifficulty.ToBotDifficulty();
            }
            return waves;
        }

        private static BossLocationSpawn[] smethod_7(WavesSettings wavesSettings, BossLocationSpawn[] bossLocationSpawn)
        {
            if (!wavesSettings.IsBosses)
            {
                return new BossLocationSpawn[0];
            }
            int i = 0;
            while (i < bossLocationSpawn.Length)
            {
                BossLocationSpawn bossLocationSpawn2 = bossLocationSpawn[i];
                List<int> list;
                try
                {
                    list = bossLocationSpawn2.BossEscortAmount.Split(new char[] { ',' }).Select(new Func<string, int>(int.Parse)).ToList<int>();
                    bossLocationSpawn2.ParseMainTypesTypes();
                }
                catch (Exception)
                {
                    goto IL_1A2;
                }
                goto IL_57;
            IL_1A2:
                i++;
                continue;
            IL_57:
                float num = ((bossLocationSpawn2.BossChance > 0f) ? 100f : (-1f));
                if (bossLocationSpawn2.BossType == WildSpawnType.sectantPriest || bossLocationSpawn2.BossType == WildSpawnType.sectantWarrior || bossLocationSpawn2.BossType == WildSpawnType.bossZryachiy || bossLocationSpawn2.BossType == WildSpawnType.followerZryachiy)
                {
                    num = -1f;
                }
                bossLocationSpawn2.BossChance = num;
                switch (wavesSettings.BotAmount)
                {
                    case EBotAmount.Low:
                        bossLocationSpawn2.BossEscortAmount = "1";
                        goto IL_1A2;
                    case EBotAmount.Medium:
                        {
                            bossLocationSpawn2.BossEscortAmount = "3";
                            goto IL_1A2;
                        }
                    case EBotAmount.High:
                    case EBotAmount.Horde:
                        bossLocationSpawn2.BossEscortAmount = "5";
                        goto IL_1A2;
                    default:
                        goto IL_1A2;
                }
            }
            return bossLocationSpawn;
        }

        public Dictionary<string, EFT.Player> Players { get; set; } = new Dictionary<string, EFT.Player>();

        private async Task<LocalPlayer> method_16(Profile profile, Vector3 position)
        {
            LocalPlayer localPlayer;
            if (!base.Status.IsRunned())
            {
                localPlayer = null;
            }
            else if (this.Players.ContainsKey(profile.Id))
            {
                GClass656 gclass656_ = this.gclass656_0;
                if (gclass656_ != null)
                {
                    gclass656_.LogError("Bot already registered. ProfileId:" + profile.Id + " bots:" + string.Join(", ", this.Players.Keys), Array.Empty<object>());
                }
                localPlayer = null;
            }
            else
            {
                int num = base.method_11();
                profile.SetSpawnedInSession(profile.Info.Side == EPlayerSide.Savage);
                LocalPlayer localPlayer2 = await LocalPlayer.Create(num, position, Quaternion.identity, "Player", "", EPointOfView.ThirdPerson, profile, true, base.UpdateQueue, EUpdateMode.Auto, EUpdateMode.Auto, BackendConfigManager.Config.CharacterController.BotPlayerMode
                    , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
                    , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
                    , new StatisticsManager(), FilterCustomizationClass1.Default, null, false);
                localPlayer2.Location = base.Location_0.Id;
                if (this.Players.ContainsKey(localPlayer2.ProfileId))
                {
                    GClass656 gclass656_2 = this.gclass656_0;
                    if (gclass656_2 != null)
                    {
                        gclass656_2.LogError("Bot duplication ProfileId:" + localPlayer2.ProfileId + " bots:" + string.Join(", ", this.Players.Keys), Array.Empty<object>());
                    }
                }
                else
                {
                    this.Players.Add(localPlayer2.ProfileId, localPlayer2);
                }
                localPlayer = localPlayer2;
            }
            return localPlayer;
        }

        public override void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f)
        {
            this.BossWaveManager.Stop();
            this.nonWavesSpawnScenario_0.Stop();
            this.wavesSpawnScenario_0.Stop();
            base.Stop(profileId, exitStatus, exitName, delay);
        }

        public override void CleanUp()
        {
            base.CleanUp();
            BaseLocalGame<GamePlayerOwner>.smethod_3(this.Players);
        }

        private IEnumerator method_17(float startDelay, BotControllerSettings controllerSettings, ISpawnSystem spawnSystem, Callback runCallback)
        {
            return base.vmethod_4(startDelay, controllerSettings, spawnSystem, runCallback);
        }

        private BossWaveManager BossWaveManager;

        private WavesSpawnScenario wavesSpawnScenario_0;

        private NonWavesSpawnScenario nonWavesSpawnScenario_0;

    }
   
}
