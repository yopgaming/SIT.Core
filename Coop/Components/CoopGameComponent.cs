using Comfort.Common;
using Dissonance.Datastructures;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Player;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Profiling;

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
        }

        void Start()
        {
            Logger.LogInfo("CoopGameComponent:Start");

            // ----------------------------------------------------
            // Always clear "Players" when creating a new CoopGameComponent
            Players = new ConcurrentDictionary<string, EFT.Player>();

            StartCoroutine(ReadFromServerLastActions());
            //StartCoroutine(ReadFromServerLastMoves());
            StartCoroutine(ReadFromServerCharacters());

            ListOfInteractiveObjects = FindObjectsOfType<WorldInteractiveObject>();
            PatchConstants.Logger.LogInfo($"Found {ListOfInteractiveObjects.Length} interactive objects");

            CoopPatches.EnableDisablePatches();
            GCHelpers.EnableGC();
        }

        void OnDestroy()
        {
            CoopPatches.EnableDisablePatches();
        }

        #endregion



        private IEnumerator ReadFromServerCharacters()
        {
            var waitEndOfFrame = new WaitForEndOfFrame();

            if (GetServerId() == null)
                yield return waitEndOfFrame;

            var waitSeconds = new WaitForSeconds(10f);

            Dictionary<string, object> d = new Dictionary<string, object>();
            d.Add("serverId", GetServerId());
            d.Add("pL", null);
            while (true)
            {
                yield return waitSeconds;

                if (Players == null)
                    continue;

                d["pL"] = PlayersToSpawn.Keys.ToArray();
                var jsonDataToSend = d.ToJson();

                if (RequestingObj == null)
                    RequestingObj = Request.Instance;

                //var actionsToValuesJson = RequestingObj.PostJsonAsync("/coop/server/read/players", jsonDataToSend).Result;
                //if (actionsToValuesJson == null)
                //    continue;

                //Logger.LogDebug(actionsToValuesJson);
                try
                {
                    //Dictionary<string, object>[] actionsToValues = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(actionsToValuesJson);
                    var actionsToValues = RequestingObj.PostJsonAsync<Dictionary<string, object>[]>("/coop/server/read/players", jsonDataToSend).Result;
                    if (actionsToValues == null)
                        continue;

                    //Logger.LogDebug($"CoopGameComponent.ReadFromServerCharacters:{actionsToValues.Length}");

                    var packets = actionsToValues
                         .Where(x => x != null)
                         .Select(x => x);
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
                                    // TODO: Put this back in after testing in Creation functions
                                    //if (Players == null || Players.ContainsKey(accountId))
                                    //{
                                    //    Logger.LogDebug($"Ignoring call to Spawn player {accountId}. The player already exists in the game.");
                                    //    continue;
                                    //}
                                    if(PlayersToSpawn.ContainsKey(accountId))
                                        continue;

                                    if (!PlayersToSpawnPacket.ContainsKey(accountId))
                                        PlayersToSpawnPacket.TryAdd(accountId, queuedPacket);

                                    Vector3 newPosition = Players.First().Value.Position;
                                    if (queuedPacket.ContainsKey("sPx")
                                        && queuedPacket.ContainsKey("sPy")
                                        && queuedPacket.ContainsKey("sPz"))
                                    {
                                        string npxString = queuedPacket["sPx"].ToString();
                                        newPosition.x = float.Parse(npxString);
                                        string npyString = queuedPacket["sPy"].ToString();
                                        newPosition.y = float.Parse(npyString);
                                        string npzString = queuedPacket["sPz"].ToString();
                                        newPosition.z = float.Parse(npzString) + 0.5f;

                                        if(!PlayersToSpawnPositions.ContainsKey(accountId))
                                            PlayersToSpawnPositions.TryAdd(accountId, newPosition); 

                                        PlayerBotSpawn(queuedPacket, accountId, newPosition, false);
                                    }


                                }
                            }
                        }
                    }
                }
                catch(Exception ex) 
                {

                    Logger.LogError(ex.ToString());
                
                }
                finally
                {

                }

                foreach(var p in PlayersToSpawn)
                {
                    if (p.Value != ESpawnState.Spawned)
                    {
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
                            PlayerBotSpawn(PlayersToSpawnPacket[p.Key], p.Key, newPosition, false);
                        }
                        else
                        {
                            Logger.LogError($"ReadFromServerCharacters::PlayersToSpawnPacket does not have positional data for {p.Key}");
                        }
                    }
                }


                //actionsToValuesJson = null;
                yield return waitEndOfFrame;
            }
        }

        private void PlayerBotSpawn(Dictionary<string, object> packet, string accountId, Vector3 newPosition, bool isBot)
        {
            Profile profile = MatchmakerAcceptPatches.Profile.Clone();
            profile.AccountId = accountId;

            if (PlayersToSpawn.ContainsKey(accountId) 
                && PlayersToSpawn[accountId] != ESpawnState.Loading
                && PlayersToSpawn[accountId] != ESpawnState.Spawned
                && PlayersToSpawnProfiles.ContainsKey(accountId)
                ) // CreatePhysicalOtherPlayerOrBot has been done before?
            {
                CreatePhysicalOtherPlayerOrBot(PlayersToSpawnProfiles[accountId], newPosition);
                return;
            }

            if (PlayersToSpawnProfiles.ContainsKey(accountId))
                return;

            try
            {
                //Logger.LogDebug("PlayerBotSpawn:: Adding " + accountId + " to spawner list");
                profile.Id = accountId;
                profile.Info.Nickname = "Nikita " + Players.Count;
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
                        //PatchConstants.SetFieldOrPropertyFromInstance(
                        //    profile
                        //    , "Customization"
                        //    , Activator.CreateInstance(PatchConstants.TypeDictionary["Profile.Customization"], parsedCust)
                        //    );
                        //Logger.LogDebug("PlayerBotSpawn:: Set Profile Customization for " + profile.Info.Nickname);

                    }
                }
                if (packet.ContainsKey("p.equip"))
                {
                    var pEquip = packet["p.equip"].ToString();
                    //var equipment = parsedDict["p.equip"].ToString().ParseJsonTo<Equipment>(Array.Empty<JsonConverter>());
                    var equipment = packet["p.equip"].ToString().SITParseJson<Equipment>();//.ParseJsonTo<Equipment>(Array.Empty<JsonConverter>());
                    profile.Inventory.Equipment = equipment;
                    //Logger.LogDebug("PlayerBotSpawn:: Set Equipment for " + profile.Info.Nickname);

                }
                if (packet.ContainsKey("isHost"))
                {
                }

                // Send to be loaded
                CreatePhysicalOtherPlayerOrBot(profile, newPosition);
                PlayersToSpawnProfiles.TryAdd(accountId, profile);
            }
            catch (Exception ex)
            {
                Logger.LogError($"PlayerBotSpawn::ERROR::" + ex.Message);
            }

        }

        //private LocalPlayer CreatePhysicalOtherPlayerOrBot(Profile profile, Vector3 position)
        private void CreatePhysicalOtherPlayerOrBot(Profile profile, Vector3 position)
        {
            try
            {
                EFT.Player.EUpdateMode armsUpdateMode = EFT.Player.EUpdateMode.Auto;
                //EFT.Player.EUpdateMode bodyUpdateMode = EFT.Player.EUpdateMode.Auto;
                
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
                    //Logger.LogDebug($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Loading...");
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
                            //CreatePhysicalOtherPlayerOrBot(profile, position);
                            return;
                        });

                    return;
                }

                // Its loading on the previous pass, ignore this one until its finished
                if (PlayersToSpawn[profile.AccountId] == ESpawnState.Loading)
                {
                    //Logger.LogDebug($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Is still Loading");

                    return;
                }

                // It has already spawned, we should never reach this point if Players check is working in previous step
                if (PlayersToSpawn[profile.AccountId] == ESpawnState.Spawned)
                {
                    Logger.LogDebug($"CreatePhysicalOtherPlayerOrBot::{profile.Info.Nickname}::Is already spawned");
                    return;
                }

                //Logger.LogDebug("CreatePhysicalOtherPlayerOrBot: Attempting to Create Player " + profile.Info.Nickname);

                // Local Player idea
                LocalPlayer localPlayer = LocalPlayer.Create(playerId
                    , position
                    , Quaternion.identity
                    ,
                    "Player",
                    ""
                    , EPointOfView.ThirdPerson
                    , profile
                    //, aiControl: false
                    , aiControl: true
                    , EUpdateQueue.Update
                    , armsUpdateMode
                    , EFT.Player.EUpdateMode.Auto
                    , BackendConfigManager.Config.CharacterController.ClientPlayerMode
                    , () => Singleton<OriginalSettings>.Instance.Control.Settings.MouseSensitivity
                    , () => Singleton<OriginalSettings>.Instance.Control.Settings.MouseAimingSensitivity
                    , new StatisticsManagerForPlayer1()
                    , new FilterCustomizationClass()
                    , null
                    , isYourPlayer: false).Result;

                var assetBundles = PatchConstants.EftTypes.FirstOrDefault(x
                    => x.GetProperty("PLAYER_BUNDLE_NAME", bindingAttr: System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public) != null);


                //EFT.Player localPlayer = EFT.Player.Create<EFT.Player>(
                //    GClass1337.PLAYER_BUNDLE_NAME
                //    , playerId
                //    , position
                //    , EUpdateQueue.Update
                //    , armsUpdateMode
                //    , armsUpdateMode
                //    , BackendConfigManager.Config.CharacterController.ClientPlayerMode
                //    , () => Singleton<OriginalSettings>.Instance.Control.Settings.MouseSensitivity
                //    , () => Singleton<OriginalSettings>.Instance.Control.Settings.MouseAimingSensitivity
                //    , "ClientDrone"
                //    , true);
                //var inventoryController = new InventoryController(profile, false);
                //localPlayer.Init(Quaternion.identity
                //    //, "Clones"
                //    , "Player"
                //    , EPointOfView.ThirdPerson
                //    , profile
                //    , inventoryController
                //    , new PlayerHealthController(profile.Health, localPlayer, inventoryController, profile.Skills, false)
                //    , new StatisticsManagerForPlayer1()
                //    , null
                //    , new FilterCustomizationClass()
                //    , EFT.Player.EVoipState.NotAvailable
                //    , false
                //    , false
                //    );
                //localPlayer._animators[0].enabled = true;

                // Observed Player idea (more like Live BSG option)
                //ObservedPlayer.smethod_3(playerId, position, this, EUpdateQueue.Update, true);

                if (localPlayer == null)
                    return;

                PlayersToSpawn[profile.AccountId] = ESpawnState.Spawned;

                if (!Players.ContainsKey(profile.AccountId))
                    Players.TryAdd(profile.AccountId, localPlayer);

                //Logger.LogDebug("CreatePhysicalOtherPlayerOrBot: Created Player " + profile.Info.Nickname);
                var prc = localPlayer.GetOrAddComponent<PlayerReplicatedComponent>();
                prc.IsClientDrone = true;

                // ----------------------------------------------------------------------------------------------------
                // Find the Original version of this Player/Bot and hide them. This is so the SERVER sees the same as CLIENTS.
                //
                if (Singleton<GameWorld>.Instance.RegisteredPlayers.Any(x => x.Profile.AccountId == profile.AccountId))
                {
                    var originalPlayer = Singleton<GameWorld>.Instance.RegisteredPlayers.FirstOrDefault(x => x.Profile.AccountId == profile.AccountId);
                    if (originalPlayer != null)
                    {
                        if (originalPlayer.TryGetComponent<MeshRenderer>(out var meshRenderer))
                        {
                            Logger.LogDebug($"{profile.AccountId} disable meshRenderer");
                            meshRenderer.enabled = false;
                        }
                        if (originalPlayer.TryGetComponent<Renderer>(out var renderer))
                        {
                            Logger.LogDebug($"{profile.AccountId} disable renderer");
                            renderer.enabled = false;
                        }
                    }
                }
                //
                // ----------------------------------------------------------------------------------------------------
                SetWeaponInHandsOfNewPlayer(localPlayer);
                //Singleton<GameWorld>.Instance.RegisterPlayer(localPlayer);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

        }

        private void SetWeaponInHandsOfNewPlayer(EFT.Player person)
        {
            var equipment = person.Profile.Inventory.Equipment;
            if (equipment == null)
            {
                Logger.LogError($"SetWeaponInHandsOfNewPlayer: {person.Profile.AccountId} has no Equipment!");
                return;
            }
            Item item = null;

            if (equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem != null)
                item = equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem;
            //equipment.GetSlot(EquipmentSlot.FirstPrimaryWeapon).ContainedItem
            //    ?? equipment.GetSlot(EquipmentSlot.SecondPrimaryWeapon).ContainedItem
            //    ?? equipment.GetSlot(EquipmentSlot.Holster).ContainedItem
            //    ?? equipment.GetSlot(EquipmentSlot.Scabbard).ContainedItem;
            if (item == null)
            {
                Logger.LogError($"SetWeaponInHandsOfNewPlayer:Unable to find any weapon for {person.Profile.AccountId}");
                return;
            }

            person.SetItemInHands(item, (IResult)=> {
            
                if(IResult.Failed == true)
                {
                    Logger.LogError($"SetWeaponInHandsOfNewPlayer:Unable to set item {item} in hands for {person.Profile.AccountId}");
                }

            });
        }

        /// <summary>
        /// Gets the Last Actions Dictionary from the Server. This should not be used for things like Moves. Just other stuff.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadFromServerLastActions()
        {
            Stopwatch swDebugPerformance = new Stopwatch();
            Stopwatch swRequests = new Stopwatch();

            var waitEndOfFrame = new WaitForEndOfFrame();

            if (GetServerId() == null)
                yield return waitEndOfFrame;

            var fTimeToWaitInMS = 500;
            var waitSeconds = new WaitForSeconds(fTimeToWaitInMS / 1000f);

            var jsonDataServerId = new Dictionary<string, object>
            {
                { "serverId", GetServerId() },
                { "t", ReadFromServerLastActionsLastTime }
            };
            while (true)
            {
                yield return waitSeconds;

                jsonDataServerId["t"] = ReadFromServerLastActionsLastTime;
                swDebugPerformance.Reset();
                swDebugPerformance.Start();
                if (Players == null)
                {
                    PatchConstants.Logger.LogInfo("CoopGameComponent:No Players Found! Nothing to process!");
                    yield return waitSeconds;
                    continue;
                }

                if (RequestingObj == null)
                    RequestingObj = Request.Instance;

                swRequests.Reset();
                swRequests.Start();
                var actionsToValuesJson = RequestingObj.PostJsonAsync("/coop/server/read/lastActions", jsonDataServerId.ToJson()).Result;
                ReadFromServerLastActionsParseData(actionsToValuesJson);
                ApproximatePing = new DateTime(DateTime.Now.Ticks - ReadFromServerLastActionsLastTime).Millisecond - fTimeToWaitInMS;
                ReadFromServerLastActionsLastTime = DateTime.Now.Ticks;
                
                actionsToValuesJson = null;
                swRequests.Stop();
            }
        }

        public void ReadFromServerLastActionsParseData(string actionsToValuesJson)
        {
            if (Singleton<GameWorld>.Instance == null)
                return;

            if (actionsToValuesJson == null)
            {
                PatchConstants.Logger.LogInfo("CoopGameComponent:No Data Returned from Last Actions!");
                return;
            }

            //Logger.LogInfo($"CoopGameComponent:ReadFromServerLastActions:{actionsToValuesJson}");
            Dictionary<string, JObject> actionsToValues = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(actionsToValuesJson);
            if (actionsToValues == null)
            {
                return;

            }

            var packets = actionsToValues.Values
                 .Where(x => x != null)
                 .Where(x => x.Count > 0)
                 .Select(x => x.ToObject<Dictionary<string, object>>());
            //.Where(x => x.ContainsKey("m") && x["m"].ToString() != "Move")
            //.Where(x => x.ContainsKey("accountId"));
            if (packets == null)
            {
                //PatchConstants.Logger.LogInfo("CoopGameComponent:No Data Returned from Last Actions!");
                return;

            }

            if (!packets.Any())
            {
                //PatchConstants.Logger.LogInfo("CoopGameComponent:No Data Returned from Last Actions!");
                return;

            }

            foreach (var packet in packets.Where(x => x.ContainsKey("m")))
            {
                if (packet == null || packet.Count == 0)
                    continue;

                var accountId = packet["accountId"].ToString();
                if (!Players.ContainsKey(accountId))
                {
                    Logger.LogInfo($"TODO: FIXME: Players does not contain {accountId}. Searching. This is SLOW. FIXME! Don't do this!");
                    foreach (var p in FindObjectsOfType<LocalPlayer>())
                    {
                        if (!Players.ContainsKey(p.Profile.AccountId))
                        {
                            Players.TryAdd(p.Profile.AccountId, p);
                            var nPRC = p.GetOrAddComponent<PlayerReplicatedComponent>();
                            nPRC.player = p;
                        }
                    }
                    continue;
                }

                try
                {
                    foreach (var plyr in Players.Where(x => x.Key == packet["accountId"].ToString()))
                    {
                        plyr.Value.TryGetComponent<PlayerReplicatedComponent>(out var prc);

                        if (prc == null)
                        {
                            Logger.LogError($"Player {accountId} doesn't have a PlayerReplicatedComponent!");
                            continue;
                        }

                        prc.HandlePacket(packet);
                    }
                }
                catch (Exception) { }

                try
                {
                    // Deal to all versions of this guy (this shouldnt happen but good for testing)
                    foreach (var plyr in Singleton<GameWorld>.Instance.RegisteredPlayers.Where(x => x.Profile != null && x.Profile.AccountId == packet["accountId"].ToString()))
                    {
                        if (!plyr.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                        {

                            Logger.LogError($"Player {accountId} doesn't have a PlayerReplicatedComponent!");
                            continue;
                        }

                        prc.HandlePacket(packet);
                    }
                }
                catch (Exception) { }

                
            }
        }

        ///// <summary>
        ///// Gets the Last Moves Dictionary from the Server. This should be the last move action each account id (character) made
        ///// </summary>
        ///// <returns></returns>
        //private IEnumerator ReadFromServerLastMoves()
        //{
        //    var waitEndOfFrame = new WaitForEndOfFrame();
        //    var waitSeconds = new WaitForSeconds(1f);

        //    if (GetServerId() == null)
        //        yield return waitSeconds;

        //    var jsonDataServerId = JsonConvert.SerializeObject(new Dictionary<string, object>
        //    {
        //        { "serverId", GetServerId() }
        //    });
        //    while (true)
        //    {
        //        yield return waitSeconds;

        //        if (Players == null)
        //            continue;

        //        if (RequestingObj == null)
        //            RequestingObj = Request.Instance;

        //        try
        //        {
        //            var actionsToValuesJson = RequestingObj.PostJsonAsync("/coop/server/read/lastMoves", jsonDataServerId).Result;
        //            ReadFromServerLastMoves_ParseData(actionsToValuesJson);
        //        }
        //        catch (Exception ex)
        //        {
        //            Logger.LogError(ex.ToString());
        //        }
        //        //yield return waitEndOfFrame;

        //    }
        //}

        //public void ReadFromServerLastMoves_ParseData(string actionsToValuesJson)
        //{
        //    if (actionsToValuesJson == null)
        //        return;

        //    try
        //    {
        //        //Logger.LogInfo($"CoopGameComponent:ReadFromServerLastMoves:{actionsToValuesJson}");
        //        Dictionary<string, JObject> actionsToValues = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(actionsToValuesJson);
        //        if (actionsToValues == null)
        //            return;

        //        var packets = actionsToValues.Values
        //            .Where(x => x != null)
        //            .Where(x => x.Count > 0)
        //            .Select(x => x.ToObject<Dictionary<string, object>>());
        //        if (packets == null)
        //            return;

        //        foreach (var packet in packets)
        //        {
        //            if (packet != null && packet.Count > 0)
        //            {
        //                if (!packet.ContainsKey("accountId"))
        //                    continue;

        //                if (Players == null)
        //                    continue;

        //                if (!Players.ContainsKey(packet["accountId"].ToString()))
        //                    continue;

        //                if (!Players[packet["accountId"].ToString()].TryGetComponent<PlayerReplicatedComponent>(out var prc))
        //                    continue;

        //                if (prc == null)
        //                    continue;

        //                //PlayerOnMovePatch.MoveReplicated(prc.player, packet);
        //            }
        //        }
        //    }
        //    finally
        //    {

        //    }
        //}

        int GuiX = Screen.width - 400;
        int GuiWidth = 400;

        void OnGUI()
        {
            var rect = new Rect(GuiX, 5, GuiWidth, 100);

            rect.y = 5;
            GUI.Label(rect, $"Coop Server");
            rect.y += 15;

            GUI.Label(rect, $"Ping:{(ApproximatePing >= 0 ? ApproximatePing : 0)}");
            rect.y += 15;
            if(Request.Instance != null)
            {
                var rtt = ApproximatePing + Request.Instance.PostPing;
                GUI.Label(rect, $"RTT:{(rtt >= 0 ? rtt : 0)}");
                rect.y += 15;
            }

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
                rect.y += 15;
                GUI.Label(rect, $"Players [{Singleton<GameWorld>.Instance.RegisteredPlayers.Count}]:");
                rect.y += 15;
                foreach (var p in Singleton<GameWorld>.Instance.RegisteredPlayers)
                {
                    GUI.Label(rect, $"{p.Profile.Nickname}:{(p.IsAI ? "AI" : "Player")}:{(p.HealthController.IsAlive ? "Alive" : "Dead")}");
                    rect.y += 15;
                }
            }
        }

    }

    public enum ESpawnState
    {
        None = 0,
        Loading = 1,
        Spawning = 2,
        Spawned = 3,
        Error = 99,
    }
}
