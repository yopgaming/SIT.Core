using Bsg.GameSettings;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.Utilities;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Coop.Player;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop
{
    /// <summary>
    /// Coop Game Component is the User 1-2-1 communication to the Server
    /// </summary>
    public class CoopGameComponent : MonoBehaviour, IFrameIndexer
    {
        #region Fields/Properties        
        public WorldInteractiveObject[] ListOfInteractiveObjects { get; set; }
        private Request RequestingObj { get; set; }
        public string ServerId { get; set; } = null;
        public ConcurrentDictionary<string, EFT.Player> Players { get; private set; } = new();
        public ConcurrentQueue<Dictionary<string, object>> QueuedPackets { get; } = new();

        BepInEx.Logging.ManualLogSource Logger { get; set; }

        public static Vector3? ClientSpawnLocation { get; set; }

        private long ReadFromServerLastActionsLastTime { get; set; } = -1;
        private long ApproximatePing { get; set; } = 1;

        public ConcurrentDictionary<string, ESpawnState> PlayersToSpawn { get; private set; } = new();
        public ConcurrentDictionary<string, Dictionary<string, object>> PlayersToSpawnPacket { get; private set; } = new();
        public ConcurrentDictionary<string, Profile> PlayersToSpawnProfiles { get; private set; } = new();
        public ConcurrentDictionary<string, Vector3> PlayersToSpawnPositions { get; private set; } = new();
        public ulong LocalIndex { get; set; }

        public double LocalTime => 0;

        public bool SETTING_DEBUGSpawnDronesOnServer { get; set; } = false;
        public bool SETTING_DEBUGShowPlayerList { get; set; } = false;
        public bool SETTING_Actions_AlwaysProcessAllActions { get; private set; }
        public int SETTING_Actions_CutoffTimeInSeconds { get; private set; }
        public int SETTING_PlayerStateTickRateInMS { get; set; } = -1000;
        public bool SETTING_AlwaysProcessEverything { get; set; } = false;

        #endregion

        #region Public Voids
        public static CoopGameComponent GetCoopGameComponent()
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
                return null;

            var coopGC = gameWorld.GetComponent<CoopGameComponent>();
            return coopGC;
        }
        public static string GetServerId()
        {
            var coopGC = GetCoopGameComponent();
            if (coopGC == null)
                return null;

            return coopGC.ServerId;
        }
        #endregion

        #region Unity Component Methods

        void Awake()
        {

            // ----------------------------------------------------
            // Create a BepInEx Logger for CoopGameComponent
            Logger = BepInEx.Logging.Logger.CreateLogSource("CoopGameComponent");
            Logger.LogDebug("CoopGameComponent:Awake");

        }

        void Start()
        {
            Logger.LogDebug("CoopGameComponent:Start");

            // ----------------------------------------------------
            // Always clear "Players" when creating a new CoopGameComponent
            Players = new ConcurrentDictionary<string, EFT.Player>();
            var ownPlayer = (EFT.LocalPlayer)Singleton<GameWorld>.Instance.RegisteredPlayers.First(x => x.IsYourPlayer);
            Players.TryAdd(ownPlayer.Profile.AccountId, ownPlayer);

            RequestingObj = Request.GetRequestInstance(true, Logger);

            //StartCoroutine(ReadFromServerLastActions());
            StartCoroutine(ReadFromServerCharacters());
            StartCoroutine(ProcessServerCharacters());
            Task.Run(() => ReadFromServerLastActions());
            Task.Run(() => ProcessFromServerLastActions());

            ListOfInteractiveObjects = FindObjectsOfType<WorldInteractiveObject>();
            PatchConstants.Logger.LogDebug($"Found {ListOfInteractiveObjects.Length} interactive objects");

            CoopPatches.EnableDisablePatches();
            //GCHelpers.EnableGC();

            GetSettings();
            Player_Init_Patch.SendPlayerDataToServer((EFT.LocalPlayer)Singleton<GameWorld>.Instance.RegisteredPlayers.First(x => x.IsYourPlayer));
        }

        void OnDestroy()
        {
            PatchConstants.Logger.LogDebug($"CoopGameComponent:OnDestroy");

            Players.Clear();
            PlayersToSpawnProfiles.Clear();
            PlayersToSpawnPositions.Clear();
            PlayersToSpawnPacket.Clear();
            while(QueuedPackets.Count > 0)
                QueuedPackets.TryDequeue(out var packet);
            StopCoroutine(ReadFromServerCharacters());
            StopCoroutine(ProcessServerCharacters());
            RunAsyncTasks = false;

            CoopPatches.EnableDisablePatches();
        }

        void LateUpdate()
        {
            if (m_ActionPackets.Count > 0)
            {
                if (m_ActionPackets.TryDequeue(out var result))
                {
                    ReadFromServerLastActionsParseData(result);
                }
            }

            List<Dictionary<string, object>> playerStates = new List<Dictionary<string, object>>();
            if (LastPlayerStateSent < DateTime.Now.AddMilliseconds(SETTING_PlayerStateTickRateInMS))
            {
                foreach (var player in Players.Values)
                {
                    if (!player.TryGetComponent<PlayerReplicatedComponent>(out PlayerReplicatedComponent prc))
                        continue;

                    if (prc.IsClientDrone)
                        continue;

                    CreatePlayerStatePacketFromPRC(ref playerStates, player, prc);
                }

                foreach (var player in Singleton<GameWorld>.Instance.RegisteredPlayers)
                {
                    if (!player.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                        continue;

                    if (prc.IsClientDrone)
                        continue;

                    // TODO: This needs to double check I dont send the same stuff twice!
                    CreatePlayerStatePacketFromPRC(ref playerStates, player, prc);
                }

                RequestingObj.PostJsonAndForgetAsync("/coop/server/update", playerStates.SITToJson());


                LastPlayerStateSent = DateTime.Now;
            }
        }

        #endregion

        void GetSettings()
        {
            SETTING_DEBUGSpawnDronesOnServer = Plugin.Instance.Config.Bind<bool>
                ("Coop", "ShowDronesOnServer", false, new BepInEx.Configuration.ConfigDescription("Whether to spawn the client drones on the server -- for debugging")).Value;

            SETTING_DEBUGShowPlayerList = Plugin.Instance.Config.Bind<bool>
               ("Coop", "ShowPlayerList", false, new BepInEx.Configuration.ConfigDescription("Whether to show the player list on the GUI -- for debugging")).Value;

            SETTING_PlayerStateTickRateInMS = Plugin.Instance.Config.Bind<int>
              ("Coop", "PlayerStateTickRateInMS", 1000, new BepInEx.Configuration.ConfigDescription("The rate at which Player States will be sent to the Server. DEFAULT = 1000ms")).Value;
            if (SETTING_PlayerStateTickRateInMS > 0)
                SETTING_PlayerStateTickRateInMS = SETTING_PlayerStateTickRateInMS * -1;
            else if (SETTING_PlayerStateTickRateInMS == 0)
                SETTING_PlayerStateTickRateInMS = -1000;

            SETTING_Actions_AlwaysProcessAllActions = Plugin.Instance.Config.Bind<bool>
               ("Coop", "AlwaysProcessAllActions", false, new BepInEx.Configuration.ConfigDescription("Whether to show process all actions, ignoring the time it was sent. This can cause EXTREME lag.")).Value;

            SETTING_Actions_CutoffTimeInSeconds = Plugin.Instance.Config.Bind<int>
             ("Coop", "CutoffTimeInSeconds", 3, new BepInEx.Configuration.ConfigDescription("The time at which actions are ignored. DEFAULT = 3s. MIN = 1s. MAX = 10s")).Value;
            SETTING_Actions_CutoffTimeInSeconds = Math.Max(1, SETTING_Actions_CutoffTimeInSeconds);
            SETTING_Actions_CutoffTimeInSeconds = Math.Min(10, SETTING_Actions_CutoffTimeInSeconds);

            Logger.LogDebug($"SETTING_DEBUGSpawnDronesOnServer: {SETTING_DEBUGSpawnDronesOnServer}");
            Logger.LogDebug($"SETTING_DEBUGShowPlayerList: {SETTING_DEBUGShowPlayerList}");
            Logger.LogDebug($"SETTING_PlayerStateTickRateInMS: {SETTING_PlayerStateTickRateInMS}");
            Logger.LogDebug($"SETTING_Actions_AlwaysProcessAllActions: {SETTING_Actions_AlwaysProcessAllActions}");
            Logger.LogDebug($"SETTING_Actions_CutoffTimeInSeconds: {SETTING_Actions_CutoffTimeInSeconds}");
        }

        private IEnumerator ReadFromServerCharacters()
        {
            var waitEndOfFrame = new WaitForEndOfFrame();

            if (GetServerId() == null)
                yield return waitEndOfFrame;

            var waitSeconds = new WaitForSeconds(10f);

            Dictionary<string, object> d = new Dictionary<string, object>();
            d.Add("serverId", GetServerId());
            var playerList = new List<string>();
            d.Add("pL", playerList);
            while (RunAsyncTasks)
            {
                yield return waitSeconds;

                if (Players == null)
                    continue;


                // -----------------------------------------------------------------------------------------------------------
                // We must filter out characters that already exist on this match!
                //
                if (!SETTING_DEBUGSpawnDronesOnServer)
                {
                    if (PlayersToSpawn.Count > 0)
                        playerList.AddRange(PlayersToSpawn.Keys.ToArray());
                    if (Players.Keys.Any())
                        playerList.AddRange(Players.Keys.ToArray());
                    if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any())
                        playerList.AddRange(Singleton<GameWorld>.Instance.RegisteredPlayers.Select(x => x.Profile.AccountId));
                }
                //
                // -----------------------------------------------------------------------------------------------------------

                var jsonDataToSend = d.ToJson();

                try
                {
                    m_CharactersJson = RequestingObj.PostJsonAsync<Dictionary<string, object>[]>("/coop/server/read/players", jsonDataToSend, 9999).Result;
                    //m_CharactersJson = RequestingObj.PostJsonAsync<Dictionary<string, object>[]>("/coop/server/read/players", jsonDataToSend).Result;
                    if (m_CharactersJson == null)
                        continue;

                    if (!m_CharactersJson.Any())
                        continue;

                    //Logger.LogDebug($"CoopGameComponent.ReadFromServerCharacters:{actionsToValues.Length}");

                    var packets = m_CharactersJson
                         .Where(x => x != null);
                    if (packets == null)
                        continue;

                    foreach (var queuedPacket in packets)
                    {
                        if (queuedPacket != null && queuedPacket.Count > 0)
                        {
                            if (queuedPacket != null)
                            {
                                if (queuedPacket.ContainsKey("m"))
                                {
                                    var method = queuedPacket["m"].ToString();
                                    if (method != "PlayerSpawn")
                                        continue;

                                    string accountId = queuedPacket["accountId"].ToString();
                                    if (!SETTING_DEBUGSpawnDronesOnServer)
                                    {
                                        if (Players == null
                                            || Players.ContainsKey(accountId)
                                            || Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.Profile.AccountId == accountId))
                                        {
                                            Logger.LogDebug($"Ignoring call to Spawn player {accountId}. The player already exists in the game.");
                                            continue;
                                        }
                                    }

                                    if (PlayersToSpawn.ContainsKey(accountId))
                                        continue;

                                    if (!PlayersToSpawnPacket.ContainsKey(accountId))
                                        PlayersToSpawnPacket.TryAdd(accountId, queuedPacket);

                                    if (!PlayersToSpawn.ContainsKey(accountId))
                                        PlayersToSpawn.TryAdd(accountId, ESpawnState.None);

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                    Logger.LogError(ex.ToString());

                }
                finally
                {

                }



                //actionsToValuesJson = null;
                yield return waitEndOfFrame;
            }
        }

        private IEnumerator ProcessServerCharacters()
        {
            var waitEndOfFrame = new WaitForEndOfFrame();

            if (GetServerId() == null)
                yield return waitEndOfFrame;

            var waitSeconds = new WaitForSeconds(1f);

            while (RunAsyncTasks)
            {
                yield return waitSeconds;
                foreach (var p in PlayersToSpawn)
                {
                    // If not showing drones. Check whether the "Player" has been registered, if they have, then ignore the drone
                    if (!SETTING_DEBUGSpawnDronesOnServer)
                    {
                        if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.Profile.AccountId == p.Key))
                        {
                            if (PlayersToSpawn.ContainsKey(p.Key))
                                PlayersToSpawn[p.Key] = ESpawnState.Ignore;

                            continue;
                        }

                        if (Players.Any(x => x.Key == p.Key))
                        {
                            if (PlayersToSpawn.ContainsKey(p.Key))
                                PlayersToSpawn[p.Key] = ESpawnState.Ignore;

                            continue;
                        }
                    }


                    if (PlayersToSpawn[p.Key] == ESpawnState.Ignore)
                        continue;

                    if (PlayersToSpawn[p.Key] == ESpawnState.Spawned)
                        continue;

                    Vector3 newPosition = Vector3.zero;
                    if (PlayersToSpawnPacket[p.Key].ContainsKey("sPx")
                        && PlayersToSpawnPacket[p.Key].ContainsKey("sPy")
                        && PlayersToSpawnPacket[p.Key].ContainsKey("sPz"))
                    {
                        string npxString = PlayersToSpawnPacket[p.Key]["sPx"].ToString();
                        newPosition.x = float.Parse(npxString);
                        string npyString = PlayersToSpawnPacket[p.Key]["sPy"].ToString();
                        newPosition.y = float.Parse(npyString);
                        string npzString = PlayersToSpawnPacket[p.Key]["sPz"].ToString();
                        newPosition.z = float.Parse(npzString) + 0.5f;
                        ProcessPlayerBotSpawn(PlayersToSpawnPacket[p.Key], p.Key, newPosition, false);
                    }
                    else
                    {
                        Logger.LogError($"ReadFromServerCharacters::PlayersToSpawnPacket does not have positional data for {p.Key}");
                    }
                }
                yield return waitEndOfFrame;
            }
        }

        private void ProcessPlayerBotSpawn(Dictionary<string, object> packet, string accountId, Vector3 newPosition, bool isBot)
        {
            // If not showing drones. Check whether the "Player" has been registered, if they have, then ignore the drone
            if (!SETTING_DEBUGSpawnDronesOnServer)
            {
                if(Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x=>x.Profile.AccountId == accountId))
                {
                    if (PlayersToSpawn.ContainsKey(accountId))
                        PlayersToSpawn[accountId] = ESpawnState.Ignore;

                    return;
                }

                if (Players.Keys.Any(x => x == accountId))
                {
                    if (PlayersToSpawn.ContainsKey(accountId))
                        PlayersToSpawn[accountId] = ESpawnState.Ignore;

                    return;
                }
            }

            Logger.LogDebug($"ProcessPlayerBotSpawn:{accountId}");

            // If CreatePhysicalOtherPlayerOrBot has been done before. Then ignore the Deserialization section and continue.
            if (PlayersToSpawn.ContainsKey(accountId)
                && PlayersToSpawnProfiles.ContainsKey(accountId)
                && PlayersToSpawnProfiles[accountId] != null
                ) 
            {
                CreatePhysicalOtherPlayerOrBot(PlayersToSpawnProfiles[accountId], newPosition);
                return;
            }

            if (PlayersToSpawnProfiles.ContainsKey(accountId))
                return;

            PlayersToSpawnProfiles.TryAdd(accountId, null);

            Profile profile = MatchmakerAcceptPatches.Profile.Clone();
            profile.AccountId = accountId;

            try
            {
                //Logger.LogDebug("PlayerBotSpawn:: Adding " + accountId + " to spawner list");
                profile.Id = accountId;
                profile.Info.Nickname = "BSG Employee " + Players.Count;
                profile.Info.Side = isBot ? EPlayerSide.Savage : EPlayerSide.Usec;
                if (packet.ContainsKey("p.info"))
                {
                    //Logger.LogDebug("PlayerBotSpawn:: Converting Profile data");
                    profile.Info = packet["p.info"].ToString().ParseJsonTo<ProfileInfo>(Array.Empty<JsonConverter>());
                    //Logger.LogDebug("PlayerBotSpawn:: Converted Profile data:: Hello " + profile.Info.Nickname);
                }
                if (packet.ContainsKey("p.cust"))
                {
                    var parsedCust = packet["p.cust"].ToString().ParseJsonTo<Dictionary<EBodyModelPart, string>>(Array.Empty<JsonConverter>());
                    if (parsedCust != null && parsedCust.Any())
                    {
                        profile.Customization = new Customization(parsedCust);
                        //Logger.LogDebug("PlayerBotSpawn:: Set Profile Customization for " + profile.Info.Nickname);

                    }
                }
                if (packet.ContainsKey("p.equip"))
                {
                    var pEquip = packet["p.equip"].ToString();
                    var equipment = packet["p.equip"].ToString().SITParseJson<Equipment>();
                    profile.Inventory.Equipment = equipment;
                    //Logger.LogDebug("PlayerBotSpawn:: Set Equipment for " + profile.Info.Nickname);

                }
                if (packet.ContainsKey("isHost"))
                {
                }

                // Send to be loaded
                PlayersToSpawnProfiles[accountId] = profile;
            }
            catch (Exception ex)
            {
                Logger.LogError($"PlayerBotSpawn::ERROR::" + ex.Message);
            }

        }

        private void CreatePhysicalOtherPlayerOrBot(Profile profile, Vector3 position)
        {
            try
            {
                if (Players == null)
                {
                    Logger.LogError("Players is NULL!");
                    return;
                }

                int playerId = Players.Count + Singleton<GameWorld>.Instance.RegisteredPlayers.Count + 1;
                if (profile == null)
                {
                    Logger.LogError("CreatePhysicalOtherPlayerOrBot Profile is NULL!");
                    return;
                }

                PlayersToSpawn.TryAdd(profile.AccountId, ESpawnState.None);
                if (PlayersToSpawn[profile.AccountId] == ESpawnState.None)
                {
                    PlayersToSpawn[profile.AccountId] = ESpawnState.Loading;
                    IEnumerable<ResourceKey> allPrefabPaths = profile.GetAllPrefabPaths();
                    if (allPrefabPaths.Count() == 0)
                    {
                        Logger.LogError($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::PrefabPaths are empty!");
                        PlayersToSpawn[profile.AccountId] = ESpawnState.Error;
                        return;
                    }

                    Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Local, allPrefabPaths.ToArray(), JobPriority.General)
                        .ContinueWith(delegate
                        {
                            PlayersToSpawn[profile.AccountId] = ESpawnState.Spawning;
                            Logger.LogDebug($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Load Complete.");
                            return;
                        });

                    return;
                }

                // ------------------------------------------------------------------
                // Its loading on the previous pass, ignore this one until its finished
                if (PlayersToSpawn[profile.AccountId] == ESpawnState.Loading)
                {
                    return;
                }

                // ------------------------------------------------------------------
                // It has already spawned, we should never reach this point if Players check is working in previous step
                if (PlayersToSpawn[profile.AccountId] == ESpawnState.Spawned)
                {
                    Logger.LogDebug($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Is already spawned");
                    return;
                }

                // ------------------------------------------------------------------
                // Create Local Player drone
                LocalPlayer localPlayer = LocalPlayer.Create(playerId
                    , position
                    , Quaternion.identity
                    ,
                    "Player",
                    ""
                    , EPointOfView.ThirdPerson
                    , profile
                    , aiControl: false
                    , EUpdateQueue.Update
                    , EFT.Player.EUpdateMode.Auto
                    , EFT.Player.EUpdateMode.Auto
                    , BackendConfigManager.Config.CharacterController.ClientPlayerMode
                    , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseSensitivity
                    , () => Singleton<SettingsManager>.Instance.Control.Settings.MouseAimingSensitivity
                    , new CoopStatisticsManager()
                    , FilterCustomizationClass.Default
                    , null
                    , isYourPlayer: false).Result;

                if (localPlayer == null)
                    return;

                PlayersToSpawn[profile.AccountId] = ESpawnState.Spawned;

                // ----------------------------------------------------------------------------------------------------
                // Add the player to the custom Players list
                if (!Players.ContainsKey(profile.AccountId))
                    Players.TryAdd(profile.AccountId, localPlayer);

                if (!Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.Profile.AccountId == profile.AccountId))
                    Singleton<GameWorld>.Instance.RegisteredPlayers.Add(localPlayer);

                // Create/Add PlayerReplicatedComponent to the LocalPlayer
                var prc = localPlayer.GetOrAddComponent<PlayerReplicatedComponent>();
                prc.IsClientDrone = true;

                Logger.LogDebug($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Spawned.");

                SetWeaponInHandsOfNewPlayer(localPlayer);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

        }

        /// <summary>
        /// Doesn't seem to work :(
        /// </summary>
        /// <param name="profile"></param>
        //private void MakeOriginalPlayerInvisible(Profile profile)
        //{
        //    if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.Profile.AccountId == profile.AccountId))
        //    {
        //        var originalPlayer = Singleton<GameWorld>.Instance.RegisteredPlayers.FirstOrDefault(x => x.Profile.AccountId == profile.AccountId);
        //        if (originalPlayer != null)
        //        {
        //            Logger.LogDebug($"Make {profile.AccountId} invisible?");
        //            originalPlayer.IsVisible = false;
        //        }
        //        else
        //        {
        //            Logger.LogDebug($"Unable to find {profile.AccountId} to make them invisible");
        //        }
        //    }
        //    else
        //    {
        //        Logger.LogDebug($"Unable to find {profile.AccountId} to make them invisible");
        //    }
        //}

        /// <summary>
        /// Attempts to set up the New Player with the current weapon after spawning
        /// </summary>
        /// <param name="person"></param>
        public void SetWeaponInHandsOfNewPlayer(EFT.Player person)
        {
            // Set first available item...
            //person.SetFirstAvailableItem((IResult) =>
            //{



            //});

            Logger.LogDebug($"SetWeaponInHandsOfNewPlayer: {person.Profile.AccountId}");

            var equipment = person.Profile.Inventory.Equipment;
            if (equipment == null)
            {
                Logger.LogError($"SetWeaponInHandsOfNewPlayer: {person.Profile.AccountId} has no Equipment!");
                return;
            }
            Item item = null;

            if (equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem != null)
                item = equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem;

            if (item == null && equipment.GetSlot(EquipmentSlot.SecondPrimaryWeapon).ContainedItem != null)
                item = equipment.GetSlot(EquipmentSlot.SecondPrimaryWeapon).ContainedItem;

            if (item == null && equipment.GetSlot(EquipmentSlot.Holster).ContainedItem != null)
                item = equipment.GetSlot(EquipmentSlot.Holster).ContainedItem;

            if (item == null && equipment.GetSlot(EquipmentSlot.Scabbard).ContainedItem != null)
                item = equipment.GetSlot(EquipmentSlot.Scabbard).ContainedItem;

            if (item == null)
            {
                Logger.LogError($"SetWeaponInHandsOfNewPlayer:Unable to find any weapon for {person.Profile.AccountId}");
                return;
            }

            Logger.LogDebug($"SetWeaponInHandsOfNewPlayer: {person.Profile.AccountId} {item.TemplateId}");

            person.SetItemInHands(item, (IResult) =>
            {

                if (IResult.Failed == true)
                {
                    Logger.LogError($"SetWeaponInHandsOfNewPlayer:Unable to set item {item} in hands for {person.Profile.AccountId}");
                }

            });
        }

        private string m_ActionsToValuesJson { get; set; }
        private ConcurrentQueue<string> m_ActionsToValuesJson2 { get; } = new ConcurrentQueue<string>();
        private ConcurrentQueue<Dictionary<string, object>> m_ActionPackets { get; } = new ConcurrentQueue<Dictionary<string, object>>();
        private ConcurrentBag<string> m_ProcessedActionPackets { get; } = new ConcurrentBag<string>();

        private Dictionary<string, object>[] m_CharactersJson;

        private bool RunAsyncTasks = true;

        /// <summary>
        /// Gets the Last Actions Dictionary from the Server and stores them to ActionsToValuesJson
        /// </summary>
        /// <returns></returns>
        private async Task ReadFromServerLastActions(CancellationToken cancellationToken = default(CancellationToken))
        {
            var fTimeToWaitInMS = 250;
            var jsonDataServerId = new Dictionary<string, object>
            {
                { "serverId", GetServerId() },
                { "t", ReadFromServerLastActionsLastTime }
            };

            if (RequestingObj == null)
                RequestingObj = Request.GetRequestInstance(true, Logger);

            while (RunAsyncTasks)
            {

                await Task.Delay(fTimeToWaitInMS);

                jsonDataServerId["t"] = ReadFromServerLastActionsLastTime;
                if (Players == null)
                {
                    PatchConstants.Logger.LogError("CoopGameComponent:No Players Found! Nothing to process!");
                    continue;
                }

                m_ActionsToValuesJson = await RequestingObj.GetJsonAsync($"/coop/server/read/lastActions/{GetServerId()}/{ReadFromServerLastActionsLastTime}");
                ApproximatePing = new DateTime(DateTime.Now.Ticks - ReadFromServerLastActionsLastTime).Millisecond - fTimeToWaitInMS;
                ReadFromServerLastActionsLastTime = DateTime.Now.Ticks;
            }
        }

        /// <summary>
        /// Process the ActionsToValuesJson every 1ms
        /// </summary>
        /// <returns></returns>
        private async Task ProcessFromServerLastActions()
        {
            while (RunAsyncTasks)
            {
                await Task.Delay(1);
                ReadFromServerLastActionsByAccountParseData(m_ActionsToValuesJson);
            }
        }

        private long LastReadFromServerLastActionsByAccountParseData { get; set; } = DateTime.Now.Ticks;

        public void ReadFromServerLastActionsByAccountParseData(string actionsToValuesJson)
        {
            if (string.IsNullOrEmpty(actionsToValuesJson))
                return;

            if (actionsToValuesJson.StartsWith("["))
            {
                Logger.LogDebug("ReadFromServerLastActionsByAccountParseData: Has Array. This wont work!");
                return;
            }
            
            Dictionary<string, JObject> actionsToValues = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(actionsToValuesJson);
            if (actionsToValues == null)
            {
                return;
            }

            var packets = actionsToValues.Values
                 .Where(x => x != null)
                 .Where(x => x.Count > 0)
                 .Select(x => x.ToObject<Dictionary<string, object>>());

            foreach (var packets2 in packets.Select(x=>x.Values))
            {
                foreach (var packet in packets2)
                {
                    var packetJson = packet.SITToJson();
                    if (m_ProcessedActionPackets.Contains(packetJson))
                        continue;

                    if (m_ActionPackets.Any(x => x.SITToJson() == packetJson))
                        continue;

                    try
                    {
                        var packetToProcess = JsonConvert.DeserializeObject<Dictionary<string, object>>(packetJson);
                        if(packetToProcess.ContainsKey("t") && !SETTING_AlwaysProcessEverything)
                        {
                            var useTimestamp = true;
                            if (packetToProcess.ContainsKey("m"))
                            {
                                if (packetToProcess["m"].ToString() == "Proceed" 
                                    || packetToProcess["m"].ToString() == "TryProceed"
                                    || packetToProcess["m"].ToString() == "Door"
                                    || packetToProcess["m"].ToString() == "WIO_Interact"
                                    || packetToProcess["m"].ToString() == "ApplyDamageInfo"
                                    )
                                {
                                    useTimestamp = false;
                                }
                            }

                            if (useTimestamp &&
                                    long.Parse(packetToProcess["t"].ToString()) 
                                    < LastReadFromServerLastActionsByAccountParseData - new TimeSpan(0, 0, 3).Ticks)
                                continue;
                        }


                        m_ActionPackets.Enqueue(packetToProcess);
                        m_ProcessedActionPackets.Add(packetJson);
                    }
                    catch (Exception)
                    { 
                    }
                }
            }
            LastReadFromServerLastActionsByAccountParseData = DateTime.Now.Ticks;
            packets = null;
            actionsToValues = null;

            m_ActionsToValuesJson = null;
        }


        public void ReadFromServerLastActionsParseData(Dictionary<string, object> packet)
        {
            if (Singleton<GameWorld>.Instance == null)
                return;

            if (packet == null || packet.Count == 0)
            {
                PatchConstants.Logger.LogInfo("CoopGameComponent:No Data Returned from Last Actions!");
                return;
            }

            ProcessPlayerPacket(packet);
            ProcessWorldPacket(packet);

        }

        private void ProcessWorldPacket(Dictionary<string, object> packet)
        {
            if (packet.ContainsKey("accountId"))
                return;

            switch (packet["m"].ToString())
            {
                case "WIO_Interact":
                    WorldInteractiveObject_Interact_Patch.Replicated(packet);
                    break;
                case "Door_Interact":
                    Door_Interact_Patch.Replicated(packet);
                    break;
            }
        }

        private void ProcessPlayerPacket(Dictionary<string, object> packet)
        {
            if (!packet.ContainsKey("accountId"))
                return;

            var accountId = packet["accountId"].ToString();

            foreach (var plyr in
                Players.ToArray()
                .Where(x => x.Key == accountId)
                )
            {
                plyr.Value.TryGetComponent<PlayerReplicatedComponent>(out var prc);

                if (prc == null)
                {
                    Logger.LogError($"Player {accountId} doesn't have a PlayerReplicatedComponent!");
                    continue;
                }

                prc.ProcessPacket(packet);
            }

            try
            {
                // Deal to all versions of this guy
                foreach (var plyr in Singleton<GameWorld>.Instance.RegisteredPlayers
                    .Where(x => x.Profile != null && x.Profile.AccountId == accountId))
                {
                    if (!plyr.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                    {

                        Logger.LogError($"Player {accountId} doesn't have a PlayerReplicatedComponent!");
                        continue;
                    }

                    prc.ProcessPacket(packet);
                }
            }
            catch (Exception) { }
        }

        private static void CreatePlayerStatePacketFromPRC(ref List<Dictionary<string, object>> playerStates, EFT.Player player, PlayerReplicatedComponent prc)
        {
            Dictionary<string, object> dictPlayerState = new Dictionary<string, object>();

            if (prc.ReplicatedDirection.HasValue)
            {
                dictPlayerState.Add("dX", prc.ReplicatedDirection.Value.x);
                dictPlayerState.Add("dY", prc.ReplicatedDirection.Value.y);
            }
            dictPlayerState.Add("pX", player.Position.x);
            dictPlayerState.Add("pY", player.Position.y);
            dictPlayerState.Add("pZ", player.Position.z);
            dictPlayerState.Add("rX", player.Rotation.x);
            dictPlayerState.Add("rY", player.Rotation.y);
            dictPlayerState.Add("pose", player.MovementContext.PoseLevel);
            dictPlayerState.Add("spd", player.MovementContext.CharacterMovementSpeed);
            dictPlayerState.Add("spr", player.MovementContext.IsSprintEnabled);
            dictPlayerState.Add("accountId", player.Profile.AccountId);
            dictPlayerState.Add("serverId", GetServerId());
            dictPlayerState.Add("t", DateTime.Now.Ticks);
            dictPlayerState.Add("m", "PlayerState");

            playerStates.Add(dictPlayerState);
        }

        private DateTime LastPlayerStateSent { get; set; } = DateTime.Now;

        int GuiX = 10;
        int GuiWidth = 400;

        ConcurrentQueue<long> RTTQ = new ConcurrentQueue<long>();

        void OnGUI()
        {
            var rect = new Rect(GuiX, 5, GuiWidth, 100);

            rect.y = 5;
            GUI.Label(rect, $"SIT Coop: " + (MatchmakerAcceptPatches.IsClient ? "CLIENT" : "SERVER"));
            rect.y += 15;

            GUI.Label(rect, $"Ping:{(ApproximatePing >= 0 ? ApproximatePing : 0)}");
            rect.y += 15;
            if (Request.Instance != null)
            {
                if (RTTQ.Count > 350) 
                    RTTQ.TryDequeue(out _);

                RTTQ.Enqueue(ApproximatePing + Request.Instance.PostPing);
                var rtt = Math.Round(RTTQ.Average()); // ApproximatePing + Request.Instance.PostPing;

                GUI.Label(rect, $"RTT:{(rtt >= 0 ? rtt : 0)}");
                rect.y += 15;
            }

            if (!SETTING_DEBUGShowPlayerList)
                return;

            if (PlayersToSpawn.Any(p => p.Value != ESpawnState.Spawned))
            {
                GUI.Label(rect, $"Spawning Players:");
                rect.y += 15;
                foreach (var p in PlayersToSpawn.Where(p => p.Value != ESpawnState.Spawned))
                {
                    GUI.Label(rect, $"{p.Key}:{p.Value}");
                    rect.y += 15;
                }
            }

            if (Singleton<GameWorld>.Instance != null)
            {
                var players = Singleton<GameWorld>.Instance.RegisteredPlayers.ToList();
                players.AddRange(Players.Values);

                rect.y += 15;
                GUI.Label(rect, $"Players [{players.Count}]:");
                rect.y += 15;
                foreach (var p in players)
                {
                    GUI.Label(rect, $"{p.Profile.Nickname}:{(p.IsAI ? "AI" : "Player")}:{(p.HealthController.IsAlive ? "Alive" : "Dead")}");
                    rect.y += 15;
                }

                players.Clear();
                players = null;
            }
        }

    }

    public enum ESpawnState
    {
        None = 0,
        Loading = 1,
        Spawning = 2,
        Spawned = 3,
        Ignore = 98,
        Error = 99,
    }
}
