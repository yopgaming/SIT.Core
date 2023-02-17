using BepInEx.Logging;
using Newtonsoft.Json;
using SIT.Coop.Core.Matchmaker;
using SIT.Tarkov.Core;
using SIT.Coop.Core.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//using CoopTarkovGameServer;
using System.Collections.Concurrent;
using BepInEx.Configuration;
using SIT.Coop.Core.Player;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace SIT.Coop.Core.LocalGame
{
    /// <summary>
    /// Target that smethod_3 like
    /// </summary>
    public class LocalGameStartingPatch : ModulePatch
    {
        //public static EchoGameServer gameServer;
        private static ConfigFile _config;

        //private static LocalGameSpawnAICoroutinePatch gameSpawnAICoroutinePatch;

        public LocalGameStartingPatch(ConfigFile config)
        {
            _config = config;
            //gameSpawnAICoroutinePatch = new SIT.Coop.Core.LocalGame.LocalGameSpawnAICoroutinePatch(_config);
        }

        protected override MethodBase GetTargetMethod()
        {
            //foreach(var ty in SIT.Tarkov.Core.PatchConstants.EftTypes.Where(x => x.Name.StartsWith("BaseLocalGame")))
            //{
            //    Logger.LogInfo($"LocalGameStartingPatch:{ty}");
            //}
            _ = typeof(EFT.BaseLocalGame<GamePlayerOwner>);

            //var t = SIT.Tarkov.Core.PatchConstants.EftTypes.FirstOrDefault(x => x.FullName.StartsWith("EFT.LocalGame"));
            var t = typeof(EFT.LocalGame);
            //var t = typeof(EFT.BaseLocalGame<GamePlayerOwner>);
            if (t == null)
                Logger.LogInfo($"LocalGameStartingPatch:Type is NULL");

            var method = PatchConstants.GetAllMethodsForType(t, false)
                .FirstOrDefault(x => x.GetParameters().Length >= 3
                && x.GetParameters().Any(x => x.Name.Contains("botsSettings"))
                && x.GetParameters().Any(x => x.Name.Contains("backendUrl"))
                && x.GetParameters().Any(x => x.Name.Contains("runCallback"))
                );

            Logger.LogInfo($"LocalGameStartingPatch:{t.Name}:{method.Name}");
            return method;
        }

        [PatchPrefix]
        public static async void PatchPrefix(
            BaseLocalGame<GamePlayerOwner> __instance
            , Task __result
            )
        {

            //Logger.LogInfo($"LocalGameStartingPatch:PatchPrefix");
            LocalGamePatches.LocalGameInstance = __instance;

            //new LocalGameSpawnAICoroutinePatch(_config, __instance).Enable();
            new WaveSpawnScenarioPatch(_config).Enable();
            new NonWaveSpawnScenarioPatch(_config).Enable();
            new LocalGameBotWaveSystemPatch().Enable();

            //await StartAndConnectToServer(__instance);
        }

        [PatchPostfix]
        public static async void PatchPostfix(
            BaseLocalGame<GamePlayerOwner> __instance
            , Task __result
            )
        {
            LocalGamePatches.LocalGameInstance = __instance;
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld.TryGetComponent<CoopGameComponent>(out CoopGameComponent coopGameComponent))
            {
                GameObject.Destroy(coopGameComponent);
            }
            var coopGC = gameWorld.GetOrAddComponent<CoopGameComponent>();

            if (!string.IsNullOrEmpty(MatchmakerAcceptPatches.GetGroupId()))
                coopGC.ServerId = MatchmakerAcceptPatches.GetGroupId();
            else
                coopGC.ServerId = PatchConstants.GetPHPSESSID();

            //await StartAndConnectToServer(__instance);
        }

        //public static async Task StartAndConnectToServer(object __instance)
        //{
        //    //if (!(__instance.GetType().Name.Contains("HideoutGame")) && MatchmakerAcceptPatches.MatchingType != EMatchmakerType.Single)
        //    //{
        //    //    if (MatchmakerAcceptPatches.MatchingType == EMatchmakerType.GroupLeader)
        //    //    {
        //    //        // ------ As Host, Notify Central Server --------
        //    //        await new SIT.Tarkov.Core.Request().PostJsonAsync("/client/match/group/server/start", JsonConvert.SerializeObject(""));
        //    //        await Task.Delay(500);
        //    //    }
        //    //    else
        //    //    {

        //    //        await new SIT.Tarkov.Core.Request().PostJsonAsync("/client/match/group/server/join", JsonConvert.SerializeObject(MatchmakerAcceptPatches.GetGroupId()));
        //    //        await Task.Delay(500);
        //    //    }
        //    //}
        //    //ServerCommunication.OnDataReceived += ServerCommunication_PingPong;
        //    //ServerCommunication.OnDataArrayReceived += ServerCommunication_OnDataArrayReceived;
        //}

        //private static void ServerCommunication_OnDataArrayReceived(string[] array)
        //{
        //    for (var i = 0; i < array.Length; i++)
        //    {
        //        var @string = array[i];
        //        if (@string.Length == 4 && @string == "Ping")
        //        {
        //            ServerCommunication.SendDataDownWebSocket("Pong");
        //            return;
        //        }
        //    }
        //}

        private static void EchoGameServer_OnLog(string text)
        {
            Logger.LogInfo(text);
        }

        //private static void ServerCommunication_PingPong(byte[] buffer)
        //{
        //    if (buffer.Length == 0)
        //        return;


        //    //using (StreamReader streamReader = new StreamReader(new MemoryStream(buffer)))
        //    {
        //        {
        //            try
        //            {
        //                //string @string = streamReader.ReadToEnd();
        //                string @string = UTF8Encoding.UTF8.GetString(buffer);

        //                if (@string.Length == 4 && @string == "Ping")
        //                {
        //                    //this.DataEnqueued.Enqueue(Encoding.ASCII.GetBytes("Pong"));
        //                    //ServerCommunication.SendDataDownWebSocket("Pong");
        //                    return;
        //                }
        //                else
        //                {
        //                    Task.Run(() =>
        //                    {
        //                        //Logger.LogInfo($"LocalGameStartingPatch:OnDataReceived:{buffer.Length}");

        //                        if (@string.Length > 0 && @string[0] == '{' && @string[@string.Length - 1] == '}')
        //                        {
        //                            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(@string);

        //                            if (dictionary != null && dictionary.Count > 0)
        //                            {
        //                                if (!dictionary.ContainsKey("SERVER") && dictionary.ContainsKey("accountId"))
        //                                {
        //                                    //var player = CoopGameComponent.GetPlayerByAccountId(dictionary["accountId"].ToString());
        //                                    //if (player != null)
        //                                    //{
        //                                    //    player.GetOrAddComponent<PlayerReplicatedComponent>().QueuedPackets.Enqueue(dictionary);
        //                                    //}
        //                                }
                                       
        //                            }
        //                        }
                                
        //                    });
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                return;
        //            }
        //        }
        //    }
        //}

        private static void SetMatchmakerStatus(string status, float? progress = null)
        {
            if (LocalGamePatches.LocalGameInstance == null)
                return;

            var method = PatchConstants.GetAllMethodsForType(LocalGamePatches.LocalGameInstance.GetType()).First(x => x.Name == "SetMatchmakerStatus");
            if (method != null)
            {
                method.Invoke(LocalGamePatches.LocalGameInstance, new object[] { status, progress });
            }

        }
    }
}
