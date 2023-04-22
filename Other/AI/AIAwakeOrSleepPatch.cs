using Comfort.Common;
using EFT;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;

namespace SIT.Core.Other.AI
{
    /***
     * This is an adaptation of "Props" amazing Bush ESP patch! All credit goes to them!
     * https://hub.sp-tarkov.com/files/file/793-ai-limit/
     * 
     * 
     * 
     */
    internal class AIAwakeOrSleepComponent : MonoBehaviour
    {
        public class botPlayer
        {
            public Timer timer = new Timer(TimeAfterSpawn * 1000f);

            public int Id { get; set; }

            public float Distance { get; set; }

            public bool eligibleNow { get; set; }

            public botPlayer(int newID)
            {
                Id = newID;
                eligibleNow = false;
                timer.Enabled = false;
                timer.AutoReset = false;
                timer.Elapsed += EligiblePool(this);
                playerMapping[Id].OnPlayerDeadOrUnspawn += delegate (Player deadArgs)
                {
                    botPlayer item = null;
                    if (botMapping.ContainsKey(deadArgs.Id))
                    {
                        item = botMapping[deadArgs.Id];
                        botMapping.Remove(deadArgs.Id);
                    }
                    if (botList.Contains(item))
                    {
                        botList.Remove(item);
                    }
                    if (playerMapping.ContainsKey(deadArgs.Id))
                    {
                        playerMapping.Remove(deadArgs.Id);
                    }
                };
            }
        }

        //public static GameWorld gameWorld = new GameWorld();

        public static Dictionary<int, Player> playerMapping = new Dictionary<int, Player>();

        public static Dictionary<int, botPlayer> botMapping = new Dictionary<int, botPlayer>();

        public static List<botPlayer> botList = new List<botPlayer>();

        public static Player player;

        public static botPlayer bot;

        public static bool PluginEnabled;

        public static int BotLimit;

        public static float BotDistance;

        public static float TimeAfterSpawn;

        public string Location { get; set; }


        internal void Awake()
        {
            PluginEnabled = true;// (Config.Bind<bool>("Main Settings", "Plugin on/off", true, "");
            BotDistance = 200;// (Config.Bind<float>("Main Settings", "Bot Distance", 200f, "Set Max Distance to activate bots");
            BotLimit = 10;// (Config.Bind<int>("Main Settings", "Bot Limit (At Distance)", 10, "Based on your distance selected, limits up to this many # of bots moving at one time");
            TimeAfterSpawn = 10;// (Config.Bind<float>("Main Settings", "Time After Spawn", 10f, "Time (sec) to wait before disabling");
        }

        void Start()
        {
            PatchConstants.Logger.LogDebug($"AIAwakeOrSleepPatch:Start");
        }

        private void Update()
        {
            if (Singleton<GameWorld>.Instantiated)
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                if (!string.IsNullOrEmpty(gameWorld.LocationId) && gameWorld.LocationId != Location)
                {
                    Location = gameWorld.LocationId;
                    PatchConstants.Logger.LogDebug($"AIAwakeOrSleepPatch:{Location}");
                }
                try
                {
                    UpdateBots(gameWorld);
                }
                catch (Exception ex)
                {
                    PatchConstants.Logger.LogInfo((object)ex);
                }
            }
            else
            {
                //gameWorld = null;
            }
        }

        public void UpdateBots(GameWorld gameWorld)
        {
            if (gameWorld == null)
                return;

            if (!gameWorld.RegisteredPlayers.Any())
                return;

            int num = 0;
            for (int i = 0; i < gameWorld.RegisteredPlayers.Count; i++)
            {
                player = gameWorld.RegisteredPlayers[i];
                if (player.IsYourPlayer)
                {
                    continue;
                }
                if (!botMapping.ContainsKey(player.Id) && !playerMapping.ContainsKey(player.Id))
                {
                    playerMapping.Add(player.Id, player);
                    botPlayer value = new botPlayer(player.Id);
                    botMapping.Add(player.Id, value);
                }
                else if (!playerMapping.ContainsKey(player.Id))
                {
                    playerMapping.Add(player.Id, player);
                }
                if (botMapping.ContainsKey(player.Id))
                {
                    bot = botMapping[player.Id];
                    bot.Distance = Vector3.Distance(player.Position, gameWorld.RegisteredPlayers[0].Position);
                    if (bot.eligibleNow && !botList.Contains(bot))
                    {
                        botList.Add(bot);
                    }
                    if (!bot.timer.Enabled && player.CameraPosition != null)
                    {
                        bot.timer.Enabled = true;
                        bot.timer.Start();
                    }
                }
            }
            if (botList.Count > 1)
            {
                for (int j = 1; j < botList.Count; j++)
                {
                    botPlayer botPlayer = botList[j];
                    int num2 = j - 1;
                    while (num2 >= 0 && botList[num2].Distance > botPlayer.Distance)
                    {
                        botList[num2 + 1] = botList[num2];
                        num2--;
                    }
                    botList[num2 + 1] = botPlayer;
                }
            }
            for (int k = 0; k < botList.Count; k++)
            {
                if (num < BotLimit && botList[k].Distance < BotDistance)
                {
                    if (playerMapping.ContainsKey(botList[k].Id))
                    {
                        playerMapping[botList[k].Id].enabled = true;
                        num++;
                    }
                }
                else if (playerMapping.ContainsKey(botList[k].Id))
                {
                    playerMapping[botList[k].Id].enabled = false;
                }
            }
        }

        public static ElapsedEventHandler EligiblePool(botPlayer botplayer)
        {
            botplayer.timer.Stop();
            botplayer.eligibleNow = true;
            return null;
        }
    }
}
