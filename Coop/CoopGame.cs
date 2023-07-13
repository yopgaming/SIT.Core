using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Bots;
using EFT.Game.Spawning;
using EFT.InputSystem;
using EFT.Interactive;
using EFT.UI;
using EFT.Weather;
using JsonType;
using SIT.Coop.Core.Matchmaker;
using SIT.Core.Configuration;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop
{
    /// <summary>
    /// A custom Game Type
    /// </summary>
    public sealed class CoopGame : BaseLocalGame<GamePlayerOwner>, IBotGame
    {

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
                if (WeatherController.Instance != null)
                    return WeatherController.Instance.WeatherCurve;

                return null;
            }
        }

        private static ManualLogSource Logger;


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

            Logger = BepInEx.Logging.Logger.CreateLogSource("Coop Game Mode");

            //Logger = new ManualLogSource(typeof(CoopGame).Name);
            Logger.LogInfo("CoopGame.Create");
            if (wavesSettings.BotAmount == EBotAmount.NoBots && MatchmakerAcceptPatches.IsServer)
                wavesSettings.BotAmount = EBotAmount.Medium;

            // pre changes
            //location.OldSpawn = true;
            //location.OfflineOldSpawn = true;
            //location.NewSpawn = false;
            //location.OfflineNewSpawn = false;
            //location.BotSpawnCountStep = 1;
            //location.BotSpawnPeriodCheck = 1;
            //location.BotSpawnTimeOffMin = 1;
            //location.BotSpawnTimeOffMax = int.MaxValue;
            //location.BotSpawnTimeOnMin = 1;
            //location.BotSpawnTimeOnMax = int.MaxValue;
            //// post changes
            //Logger.LogInfo(location.ToJson());

            CoopGame coopGame = BaseLocalGame<GamePlayerOwner>
                .smethod_0<CoopGame>(inputTree, profile, backendDateTime, insurance, menuUI, commonUI, preloaderUI, gameUI, location, timeAndWeather, wavesSettings, dateTime
                , callback, fixedDeltaTime, updateQueue, backEndSession, new TimeSpan?(sessionTime));

            //Logger.LogInfo($"wavesettings: {wavesSettings.BotAmount.ToString()}");

            //Logger.LogInfo("location.waves");
            //Logger.LogInfo(location.waves.ToJson());

            //Logger.LogInfo("location.BossLocationSpawn");
            //Logger.LogInfo(location.BossLocationSpawn.ToJson());

            WildSpawnWave[] spawnWaveArray = CoopGame.CreateSpawnWaveArray(wavesSettings, location.waves);
            //Logger.LogInfo("spawnWaveArray");
            //Logger.LogInfo(spawnWaveArray.ToJson());

            // Non Waves Scenario setup
            coopGame.nonWavesSpawnScenario_0 = (NonWavesSpawnScenario)ReflectionHelpers.GetMethodForType(typeof(NonWavesSpawnScenario), "smethod_0").Invoke
                (null, new object[] { coopGame, location, coopGame.PBotsController });
            coopGame.nonWavesSpawnScenario_0.ImplementWaveSettings(wavesSettings);

            //location.OldSpawn = true;
            // Waves Scenario setup
            coopGame.wavesSpawnScenario_0 = (WavesSpawnScenario)ReflectionHelpers.GetMethodForType(typeof(WavesSpawnScenario), "smethod_0").Invoke
                (null, new object[] {
                    coopGame.gameObject
                    , spawnWaveArray
                    , new Action<Wave>((wave) => coopGame.PBotsController.ActivateBotsByWave(wave))
                    , location });
            //Logger.LogInfo("spawn scenario waves");
            //Logger.LogInfo(coopGame.wavesSpawnScenario_0.SpawnWaves.ToJson());

            coopGame.bossSpawnAdjustments = CoopGame.AdjustBossSpawnParams(wavesSettings, location.BossLocationSpawn);

            var bosswavemanagerValue = ReflectionHelpers.GetMethodForType(typeof(BossWaveManager), "smethod_0").Invoke
                (null, new object[] { coopGame.bossSpawnAdjustments, new Action<BossLocationSpawn>((bossWave) => { coopGame.PBotsController.ActivateBotsByWave(bossWave); }) });
            //(null, new object[] { location.BossLocationSpawn, new Action<BossLocationSpawn>((bossWave) => { coopGame.PBotsController.ActivateBotsByWave(bossWave); }) });
            ReflectionHelpers.GetFieldFromTypeByFieldType(typeof(CoopGame), typeof(BossWaveManager)).SetValue(coopGame, bosswavemanagerValue);
            coopGame.BossWaveManager = bosswavemanagerValue as BossWaveManager;

            coopGame.StartCoroutine(coopGame.ReplicatedWeather());
            //coopGame.StartCoroutine(coopGame.DebugObjects());
            coopGame.func_1 = (EFT.Player player) => GamePlayerOwner.Create<GamePlayerOwner>(player, inputTree, insurance, backEndSession, commonUI, preloaderUI, gameUI, coopGame.GameDateTime, location);

            return coopGame;
        }

        BossLocationSpawn[] bossSpawnAdjustments;

        public void CreateCoopGameComponent()
        {
            var coopGameComponent = CoopGameComponent.GetCoopGameComponent();
            if (coopGameComponent != null)
            {
                GameObject.Destroy(coopGameComponent);
            }

            if (CoopPatches.CoopGameComponentParent == null)
                CoopPatches.CoopGameComponentParent = new GameObject("CoopGameComponentParent");

            coopGameComponent = CoopPatches.CoopGameComponentParent.GetOrAddComponent<CoopGameComponent>();
            coopGameComponent.LocalGameInstance = this;

            //coopGameComponent = gameWorld.GetOrAddComponent<CoopGameComponent>();
            if (!string.IsNullOrEmpty(MatchmakerAcceptPatches.GetGroupId()))
                coopGameComponent.ServerId = MatchmakerAcceptPatches.GetGroupId();
            else
            {
                GameObject.Destroy(coopGameComponent);
                coopGameComponent = null;
                Logger.LogError("========== ERROR = COOP ========================");
                Logger.LogError("No Server Id found, Deleting Coop Game Component");
                Logger.LogError("================================================");
                throw new Exception("No Server Id found");
            }

            if (!MatchmakerAcceptPatches.IsClient)
                StartCoroutine(HostPinger());

            if (!MatchmakerAcceptPatches.IsClient)
                StartCoroutine(GameTimerSync());
        }



        private IEnumerator DebugObjects()
        {
            var waitSeconds = new WaitForSeconds(10f);

            while (true)
            {
                if (PlayerOwner == null)
                    yield return waitSeconds;
                //foreach(var o in  .FindObjectsOfTypeAll(typeof(GameObject)))
                //{
                //   Logger.LogInfo(o.ToString());
                //}
                foreach (var c in PlayerOwner.Player.GetComponents(typeof(GameObject)))
                {
                    Logger.LogInfo(c.ToString());
                }
                yield return waitSeconds;

            }
        }

        private IEnumerator HostPinger()
        {
            var waitSeconds = new WaitForSeconds(1f);

            while (true)
            {
                yield return waitSeconds;
                AkiBackendCommunication.Instance.SendDataToPool("{ \"HostPing\": " + DateTime.Now.Ticks + " }");
            }
        }

        private IEnumerator GameTimerSync()
        {
            var waitSeconds = new WaitForSeconds(5f);

            while (true)
            {
                yield return waitSeconds;

                if (GameTimer.SessionTime.HasValue)
                    AkiBackendCommunication.Instance.SendDataToPool("{ \"RaidTimer\": " + GameTimer.SessionTime.Value.Ticks + " }");
            }
        }

        public IEnumerator ReplicatedWeather()
        {
            var waitSeconds = new WaitForSeconds(15f);

            while (true)
            {
                yield return waitSeconds;
                if (WeatherController.Instance != null)
                    WeatherController.Instance.SetWeatherForce(new WeatherClass() { });
            }
        }




        private static WildSpawnWave[] CreateSpawnWaveArray(WavesSettings wavesSettings, WildSpawnWave[] waves)
        {
            foreach (WildSpawnWave wildSpawnWave in waves)
            {
                //Logger.LogInfo(wildSpawnWave.WildSpawnType);
                wildSpawnWave.slots_min = Math.Max(wildSpawnWave.slots_min, 0);
                wildSpawnWave.slots_max = Math.Max(wildSpawnWave.slots_max, 1);

                wildSpawnWave.time_min = -1;
                //wildSpawnWave.time_max = 5;

                //if (wavesSettings.IsTaggedAndCursed && wildSpawnWave.WildSpawnType == WildSpawnType.assault)
                //{
                //    wildSpawnWave.WildSpawnType = WildSpawnType.cursedAssault;
                //}
                //if (wavesSettings.IsBosses)
                //{
                //    wildSpawnWave.time_min += 5;
                //    wildSpawnWave.time_max += 15;
                //}
                wildSpawnWave.BotDifficulty = wavesSettings.BotDifficulty.ToBotDifficulty();
                //if (wildSpawnWave.WildSpawnType == WildSpawnType.exUsec)
                //{
                //    wildSpawnWave.slots_min = wildSpawnWave.slots_min < 1 ? 1 : wildSpawnWave.slots_min;
                //}
                //if (wildSpawnWave.WildSpawnType == WildSpawnType.pmcBot)
                //{
                //    wildSpawnWave.slots_min = wildSpawnWave.slots_min < 1 ? 1 : wildSpawnWave.slots_min;
                //}
                //if ((int)wildSpawnWave.WildSpawnType == 34)
                //{
                //    wildSpawnWave.time_min = -1;
                //    wildSpawnWave.time_max = -1;
                //    wildSpawnWave.slots_min = Math.Max(wildSpawnWave.slots_min, 1);
                //    wildSpawnWave.slots_max = Math.Max(wildSpawnWave.slots_max, 2);
                //}

                //wildSpawnWave.slots_max = Math.Max(wildSpawnWave.slots_min, wildSpawnWave.slots_max);

            }
            return waves;
        }

        private static BossLocationSpawn[] AdjustBossSpawnParams(WavesSettings wavesSettings, BossLocationSpawn[] bossLocationSpawn)
        {
            //if (!wavesSettings.IsBosses)
            //{
            //    return new BossLocationSpawn[0];
            //}
            Logger.LogInfo($"bossLocationSpawn.Length:{bossLocationSpawn.Length}");
            //foreach (var wave in bossLocationSpawn)
            //{
            //    //wave.Time = -1;
            //    if (wave.BossName == "gifter")
            //    {
            //        wave.Activated = false;
            //    }
            //    else
            //    {
            //        Logger.LogInfo($"bossLocationSpawn.name:{wave.BossName}");
            //        //wave.Activated = true;
            //        //wave.Time = -1;
            //        //wave.Delay = 0;
            //        wave.BossChance = 100;
            //    }
            //}
            return bossLocationSpawn;
        }

        public Dictionary<string, EFT.Player> Bots { get; set; } = new Dictionary<string, EFT.Player>();

        private async Task<LocalPlayer> CreatePhysicalBot(Profile profile, Vector3 position)
        {
            if (MatchmakerAcceptPatches.IsClient)
                return null;

            Logger.LogDebug($"CreatePhysicalBot: {profile.ProfileId}");
            if (Bots != null && Bots.Count(x => x.Value != null && x.Value.PlayerHealthController.IsAlive) >= MaxBotCount)
                return null;

            LocalPlayer localPlayer;
            if (!base.Status.IsRunned())
            {
                localPlayer = null;
            }
            else if (this.Bots.ContainsKey(profile.Id))
            {
                localPlayer = null;
            }
            else
            {
                int num = 999 + Bots.Count;
                profile.SetSpawnedInSession(profile.Info.Side == EPlayerSide.Savage);
                //LocalPlayer botPlayer
                //    = await LocalPlayer.Create(num, position, Quaternion.identity, "Player", "", EPointOfView.ThirdPerson, profile, true, base.UpdateQueue, EFT.Player.EUpdateMode.Auto, EFT.Player.EUpdateMode.Auto, BackendConfigManager.Config.CharacterController.BotPlayerMode
                //    , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
                //    , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
                //    , new StatisticsManager(), FilterCustomizationClass1.Default, null, false);
                CoopPlayer botPlayer
                   = (CoopPlayer)(await CoopPlayer.Create(
                       num
                       , position
                       , Quaternion.identity
                       , "Player"
                       , ""
                       , EPointOfView.ThirdPerson
                       , profile
                       , true
                       , base.UpdateQueue
                       , EFT.Player.EUpdateMode.Auto
                       , EFT.Player.EUpdateMode.Auto
                       , BackendConfigManager.Config.CharacterController.BotPlayerMode
                   , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
                   , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
                    , FilterCustomizationClass1.Default));
                botPlayer.Location = base.Location_0.Id;
                if (this.Bots.ContainsKey(botPlayer.ProfileId))
                {
                    GameObject.Destroy(botPlayer);
                    return null;
                }
                else
                {
                    this.Bots.Add(botPlayer.ProfileId, botPlayer);
                }
                localPlayer = botPlayer;
            }
            return localPlayer;
        }


        public async Task<LocalPlayer> CreatePhysicalPlayer(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, QuestControllerClass questController)
        {
            profile.SetSpawnedInSession(value: false);
            return await LocalPlayer.Create(playerId, position, rotation, "Player", "", EPointOfView.FirstPerson, profile, aiControl: false, base.UpdateQueue, armsUpdateMode, EFT.Player.EUpdateMode.Auto, BackendConfigManager.Config.CharacterController.ClientPlayerMode, () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity, () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity, new StatisticsManagerForPlayer1(), new FilterCustomizationClass(), questController, isYourPlayer: true);
        }

        public string InfiltrationPoint;

        public override void vmethod_0()
        {
        }

        /// <summary>
        /// Matchmaker countdown
        /// </summary>
        /// <param name="timeBeforeDeploy"></param>
        public override void vmethod_1(float timeBeforeDeploy)
        {
            base.vmethod_1(timeBeforeDeploy);
        }

        /// <summary>
        /// Creating the EFT.LocalPlayer
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="layerName"></param>
        /// <param name="prefix"></param>
        /// <param name="pointOfView"></param>
        /// <param name="profile"></param>
        /// <param name="aiControl"></param>
        /// <param name="updateQueue"></param>
        /// <param name="armsUpdateMode"></param>
        /// <param name="bodyUpdateMode"></param>
        /// <param name="characterControllerMode"></param>
        /// <param name="getSensitivity"></param>
        /// <param name="getAimingSensitivity"></param>
        /// <param name="statisticsManager"></param>
        /// <param name="questController"></param>
        /// <returns></returns>
        public override Task<LocalPlayer> vmethod_2(int playerId, Vector3 position, Quaternion rotation, string layerName, string prefix, EPointOfView pointOfView, Profile profile, bool aiControl, EUpdateQueue updateQueue, EFT.Player.EUpdateMode armsUpdateMode, EFT.Player.EUpdateMode bodyUpdateMode, CharacterControllerSpawner.Mode characterControllerMode, Func<float> getSensitivity, Func<float> getAimingSensitivity, IStatisticsManager statisticsManager, QuestControllerClass questController)
        {
            //Logger.LogInfo("Creating CoopPlayer!");
            profile.SetSpawnedInSession(value: false);
            return CoopPlayer
                .Create(
                playerId
                , position
                , rotation
                , "Player"
                , ""
                , EPointOfView.FirstPerson
                , profile
                , aiControl: false
                , base.UpdateQueue
                , armsUpdateMode
                , EFT.Player.EUpdateMode.Auto
                , BackendConfigManager.Config.CharacterController.ClientPlayerMode
                , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
                , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
                , new FilterCustomizationClass()
                , questController
                , isYourPlayer: true);
            //return base.vmethod_2(playerId, position, rotation, layerName, prefix, pointOfView, profile, aiControl, updateQueue, armsUpdateMode, bodyUpdateMode, characterControllerMode, getSensitivity, getAimingSensitivity, statisticsManager, questController);
        }

        /// <summary>
        /// Reconnection handling.
        /// </summary>
        public override void vmethod_3()
        {
            base.vmethod_3();
        }

        /// <summary>
        /// Bot System Starter -> Countdown
        /// </summary>
        /// <param name="startDelay"></param>
        /// <param name="controllerSettings"></param>
        /// <param name="spawnSystem"></param>
        /// <param name="runCallback"></param>
        /// <returns></returns>
        public override IEnumerator vmethod_4(float startDelay, BotControllerSettings controllerSettings, ISpawnSystem spawnSystem, Callback runCallback)
        {
            //Logger.LogDebug("vmethod_4");

            var shouldSpawnBots = !MatchmakerAcceptPatches.IsClient && PluginConfigSettings.Instance.CoopSettings.EnableAISpawnWaveSystem;
            if (!shouldSpawnBots)
            {
                controllerSettings.BotAmount = EBotAmount.NoBots;
                Logger.LogDebug("Bot Spawner System has been turned off");
            }

            var nonwaves = (WaveInfo[])ReflectionHelpers.GetFieldFromTypeByFieldType(this.nonWavesSpawnScenario_0.GetType(), typeof(WaveInfo[])).GetValue(this.nonWavesSpawnScenario_0);

            LocalGameBotCreator profileCreator =
                new(BackEndSession
                , this.wavesSpawnScenario_0.SpawnWaves
                , bossSpawnAdjustments
                , nonwaves
                , true);
            BotCreator botCreator = new(this, profileCreator, this.CreatePhysicalBot);
            BotZone[] botZones = LocationScene.GetAllObjects<BotZone>(false).ToArray<BotZone>();
            this.PBotsController.Init(this
                , botCreator
                , botZones
                , spawnSystem
                , this.wavesSpawnScenario_0.BotLocationModifier
                , controllerSettings.IsEnabled
                , controllerSettings.IsScavWars
                , true
                , false
                , false
                , Singleton<GameWorld>.Instance
                , base.Location_0.OpenZones)
                ;

            Logger.LogInfo($"Location: {Location_0.Name}");

            MaxBotCount = controllerSettings.BotAmount switch
            {
                EBotAmount.AsOnline => 10,
                EBotAmount.Low => 11,
                EBotAmount.Medium => 12,
                EBotAmount.High => 13,
                EBotAmount.Horde => 13,
                _ => 13,
            };

            int numberOfBots = shouldSpawnBots ? MaxBotCount : 0;
            //Logger.LogDebug($"vmethod_4: Number of Bots: {numberOfBots}");

            this.PBotsController.SetSettings(numberOfBots, this.BackEndSession.BackEndConfig.BotPresets, this.BackEndSession.BackEndConfig.BotWeaponScatterings);
            this.PBotsController.AddActivePLayer(this.PlayerOwner.Player);

            // ---------------------------------------------
            // Here we can wait for other players, if desired

            // ---------------------------------------------
            yield return new WaitForSeconds(startDelay);
            if (shouldSpawnBots)
            {
                this.BossWaveManager.Run(EBotsSpawnMode.Anyway);

                if (this.nonWavesSpawnScenario_0 != null)
                    this.nonWavesSpawnScenario_0.Run();

                Logger.LogDebug($"Running Wave Scenarios");

                if (this.wavesSpawnScenario_0.SpawnWaves != null && this.wavesSpawnScenario_0.SpawnWaves.Length != 0)
                {
                    Logger.LogDebug($"Running Wave Scenarios with Spawn Wave length : {this.wavesSpawnScenario_0.SpawnWaves.Length}");
                    this.wavesSpawnScenario_0.Run(EBotsSpawnMode.Anyway);
                }
                //else
                //{
                //    this.nonWavesSpawnScenario_0.Run();
                //}

                StartCoroutine(StopBotSpawningAfterTimer());
            }
            else
            {
                if (this.wavesSpawnScenario_0 != null)
                    this.wavesSpawnScenario_0.Stop();
                if (this.nonWavesSpawnScenario_0 != null)
                    this.nonWavesSpawnScenario_0.Stop();
                if (this.BossWaveManager != null)
                    this.BossWaveManager.Stop();
            }
            yield return new WaitForEndOfFrame();
            Logger.LogInfo("vmethod_4.SessionRun");
            CreateExfiltrationPointAndInitDeathHandler();
            yield break;
        }

        private IEnumerator StopBotSpawningAfterTimer()
        {
            yield return new WaitForSeconds(180);
            if (this.wavesSpawnScenario_0 != null)
            {
                this.wavesSpawnScenario_0.Stop();
            }

            if (this.nonWavesSpawnScenario_0 != null)
            {
                this.nonWavesSpawnScenario_0.Stop();
            }

            if (this.BossWaveManager != null)
                this.BossWaveManager.Stop();
        }

        //public override void vmethod_5()
        //{
        //    return;
        //}
        /// <summary>
        /// Died event handler
        /// </summary>
        public void CreateExfiltrationPointAndInitDeathHandler()
        {
            Logger.LogInfo("CreateExfiltrationPointAndInitDeathHandler");

            SpawnPoints spawnPoints = SpawnPoints.CreateFromScene(DateTime.Now, base.Location_0.SpawnPointParams);
            int spawnSafeDistance = ((Location_0.SpawnSafeDistanceMeters > 0) ? Location_0.SpawnSafeDistanceMeters : 100);
            SpawnSystemSettings settings = new(Location_0.MinDistToFreePoint, Location_0.MaxDistToFreePoint, Location_0.MaxBotPerZone, spawnSafeDistance);
            SpawnSystem = SpawnSystemFactory.CreateSpawnSystem(settings, () => Time.time, Singleton<GameWorld>.Instance, PBotsController, spawnPoints);

            base.GameTimer.Start();
            //base.vmethod_5();
            gparam_0.vmethod_0();
            gparam_0.Player.ActiveHealthController.DiedEvent += HealthController_DiedEvent;
            gparam_0.Player.HealthController.DiedEvent += HealthController_DiedEvent;

            ISpawnPoint spawnPoint = SpawnSystem.SelectSpawnPoint(ESpawnCategory.Player, Profile_0.Info.Side);
            InfiltrationPoint = spawnPoint.Infiltration;
            Profile_0.Info.EntryPoint = InfiltrationPoint;
            Logger.LogDebug(InfiltrationPoint);
            ExfiltrationControllerClass.Instance.InitAllExfiltrationPoints(Location_0.exits, justLoadSettings: false, "");
            ExfiltrationPoint[] exfilPoints = ExfiltrationControllerClass.Instance.EligiblePoints(Profile_0);
            base.GameUi.TimerPanel.SetTime(DateTime.UtcNow, Profile_0.Info.Side, base.GameTimer.SessionSeconds(), exfilPoints);
            foreach (ExfiltrationPoint exfiltrationPoint in exfilPoints)
            {
                exfiltrationPoint.OnStartExtraction += ExfiltrationPoint_OnStartExtraction;
                exfiltrationPoint.OnCancelExtraction += ExfiltrationPoint_OnCancelExtraction;
                exfiltrationPoint.OnStatusChanged += ExfiltrationPoint_OnStatusChanged;
                UpdateExfiltrationUi(exfiltrationPoint, contains: false, initial: true);
            }

            base.dateTime_0 = DateTime.UtcNow;
            base.Status = GameStatus.Started;
            ConsoleScreen.ApplyStartCommands();
        }

        public Dictionary<string, (float, long, string)> ExtractingPlayers = new();
        public List<string> ExtractedPlayers = new();

        private void ExfiltrationPoint_OnCancelExtraction(ExfiltrationPoint point, EFT.Player player)
        {
            Logger.LogInfo("ExfiltrationPoint_OnCancelExtraction");
            Logger.LogInfo(point.Status);

            ExtractingPlayers.Remove(player.ProfileId);

            MyExitLocation = null;
            //player.SwitchRenderer(true);
        }

        private void ExfiltrationPoint_OnStartExtraction(ExfiltrationPoint point, EFT.Player player)
        {
            Logger.LogInfo("ExfiltrationPoint_OnStartExtraction");
            Logger.LogInfo(point.Settings.Name);
            Logger.LogInfo(point.Status);
            //Logger.LogInfo(point.ExfiltrationStartTime);
            Logger.LogInfo(point.Settings.ExfiltrationTime);
            bool playerHasMetRequirements = !point.UnmetRequirements(player).Any();
            //if (playerHasMetRequirements && !ExtractingPlayers.ContainsKey(player.ProfileId) && !ExtractedPlayers.Contains(player.ProfileId))
            if (!ExtractingPlayers.ContainsKey(player.ProfileId) && !ExtractedPlayers.Contains(player.ProfileId))
                ExtractingPlayers.Add(player.ProfileId, (point.Settings.ExfiltrationTime, DateTime.Now.Ticks, point.Settings.Name));
            //player.SwitchRenderer(false);

            MyExitLocation = point.Settings.Name;


        }

        private void ExfiltrationPoint_OnStatusChanged(ExfiltrationPoint point, EExfiltrationStatus status)
        {
            UpdateExfiltrationUi(point, point.Entered.Any((EFT.Player x) => x.ProfileId == Profile_0.Id));
            Logger.LogInfo("ExfiltrationPoint_OnStatusChanged");
            Logger.LogInfo(status);
        }

        public ExitStatus MyExitStatus { get; set; } = ExitStatus.Survived;
        public string MyExitLocation { get; set; } = null;
        private ISpawnSystem SpawnSystem { get; set; }
        public int MaxBotCount { get; private set; }

        private void HealthController_DiedEvent(EDamageType obj)
        {
            Logger.LogInfo(ScreenManager.Instance.CurrentScreenController.ScreenType);

            Logger.LogInfo("CoopGame.HealthController_DiedEvent");

            gparam_0.Player.HealthController.DiedEvent -= method_15;
            gparam_0.Player.HealthController.DiedEvent -= HealthController_DiedEvent;

            PlayerOwner.vmethod_1();
            MyExitStatus = ExitStatus.Killed;
            MyExitLocation = null;

        }

        public override void Stop(string profileId, ExitStatus exitStatus, string exitName, float delay = 0f)
        {
            Logger.LogInfo("CoopGame.Stop");

            if (this.BossWaveManager != null)
                this.BossWaveManager.Stop();

            if (this.nonWavesSpawnScenario_0 != null)
                this.nonWavesSpawnScenario_0.Stop();

            if (this.wavesSpawnScenario_0 != null)
                this.wavesSpawnScenario_0.Stop();

            base.Stop(profileId, exitStatus, exitName, delay);
        }

        public override void CleanUp()
        {
            base.CleanUp();
            BaseLocalGame<GamePlayerOwner>.smethod_4(this.Bots);
        }

        private BossWaveManager BossWaveManager;

        private WavesSpawnScenario wavesSpawnScenario_0;

        private NonWavesSpawnScenario nonWavesSpawnScenario_0;

        private Func<EFT.Player, GamePlayerOwner> func_1;

    }
}
