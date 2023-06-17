using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GClass1648;

namespace SIT.Core.Configuration
{
    public class PluginConfigSettings
    {
        public ConfigFile Config { get; }
        public ManualLogSource Logger { get; }

        public static PluginConfigSettings Instance { get; private set; }

        public CoopConfigSettings CoopSettings { get; }

        public PluginConfigSettings(ManualLogSource logger, ConfigFile config)
        {
            Logger = logger;
            Config = config;
            CoopSettings = new CoopConfigSettings(logger, config);
            Instance = this;
        }

        public void GetSettings()
        {

        }

        public class CoopConfigSettings
        {
            public ConfigFile Config { get; }
            public ManualLogSource Logger { get; }

            public CoopConfigSettings(ManualLogSource logger, ConfigFile config)
            {
                Logger = logger;
                Config = config;
                GetSettings();
            }

            public bool SETTING_DEBUGSpawnDronesOnServer { get; set; } = false;
            public bool SETTING_DEBUGShowPlayerList { get; set; } = false;
            public int SETTING_Actions_TickRateInMS { get; private set; } = 999;
            public int SETTING_PlayerStateTickRateInMS { get; set; } = -100;
            public bool SETTING_HeadshotsAlwaysKill { get; set; } = true;
            public int SITWebSocketPort { get; set; } = 6970;

            public bool AllPlayersSpawnTogether { get; set; } = true;
            public bool ArenaMode { get; set; } = false;
            public bool EnableAISpawnWaveSystem { get; set; } = true;

            public bool ForceHighPingMode { get; set; } = false;

            public void GetSettings()
            {
                SETTING_DEBUGSpawnDronesOnServer = Plugin.Instance.Config.Bind
                ("Coop", "ShowDronesOnServer", false, new ConfigDescription("Whether to spawn the client drones on the server -- for debugging")).Value;

                SETTING_DEBUGShowPlayerList = Plugin.Instance.Config.Bind
                   ("Coop", "ShowPlayerList", false, new ConfigDescription("Whether to show the player list on the GUI -- for debugging")).Value;

                SETTING_PlayerStateTickRateInMS = Plugin.Instance.Config.Bind
                  ("Coop", "PlayerStateTickRateInMS", 100, new ConfigDescription("The rate at which Player States will be sent to the Server")).Value;
                if (SETTING_PlayerStateTickRateInMS > 0)
                    SETTING_PlayerStateTickRateInMS = SETTING_PlayerStateTickRateInMS * -1;
                else if (SETTING_PlayerStateTickRateInMS == 0)
                    SETTING_PlayerStateTickRateInMS = -100;

                SETTING_Actions_TickRateInMS = Plugin.Instance.Config.Bind
                ("Coop", "LastActionTickRateInMS", SETTING_Actions_TickRateInMS, new ConfigDescription("The tick rate at which actions acquired from Server. MIN = 250ms. MAX = 999ms")).Value;
                SETTING_Actions_TickRateInMS = Math.Max(250, SETTING_Actions_TickRateInMS);
                SETTING_Actions_TickRateInMS = Math.Min(999, SETTING_Actions_TickRateInMS);

                SETTING_HeadshotsAlwaysKill = Plugin.Instance.Config.Bind
                  ("Coop", "HeadshotsAlwaysKill", true, new ConfigDescription("Enable to make headshots actually work, no more tanking definite kills!")).Value;

                SITWebSocketPort = Plugin.Instance.Config.Bind("Coop", "SITPort", 6970, new ConfigDescription("SIT.Core Websocket Port DEFAULT = 6970")).Value;

                AllPlayersSpawnTogether = Plugin.Instance.Config.Bind
               ("Coop", "AllPlayersSpawnTogether", true, new ConfigDescription("Whether to spawn all players in the same place")).Value;

                ArenaMode = Plugin.Instance.Config.Bind
                ("Coop", "ArenaMode", false, new ConfigDescription("Arena Mode - For the meme's (DEBUG). Can SIT be less laggy than Live Tarkov in PvP?")).Value;

                EnableAISpawnWaveSystem = Plugin.Instance.Config.Bind("Coop", "EnableAISpawnWaveSystem", true
                        , new ConfigDescription("Whether to run the Wave Spawner System. Useful for testing.")).Value;

                ForceHighPingMode = Plugin.Instance.Config.Bind("Coop", "ForceHighPingMode", false
                        , new ConfigDescription("Forces the High Ping Mode which allows some actions to not round-trip. This may be useful if you have large input lag")).Value;


            Logger.LogDebug($"SETTING_DEBUGSpawnDronesOnServer: {SETTING_DEBUGSpawnDronesOnServer}");
                Logger.LogDebug($"SETTING_DEBUGShowPlayerList: {SETTING_DEBUGShowPlayerList}");
                Logger.LogDebug($"SETTING_PlayerStateTickRateInMS: {SETTING_PlayerStateTickRateInMS}");
                Logger.LogDebug($"SETTING_Actions_TickRateInMS: {SETTING_Actions_TickRateInMS}");
                Logger.LogDebug($"SETTING_HeadshotsAlwaysKill: {SETTING_HeadshotsAlwaysKill}");
                Logger.LogDebug($"SITWebSocketPort: {SITWebSocketPort}");
                Logger.LogDebug($"AllPlayersSpawnTogether: {AllPlayersSpawnTogether}");
                Logger.LogDebug($"ArenaMode: {ArenaMode}");
                Logger.LogDebug($"ForceHighPingMode: {ForceHighPingMode}");

                if (ArenaMode)
                {
                    Logger.LogInfo($"x!Arena Mode Activated!x");
                    AllPlayersSpawnTogether = false;
                    EnableAISpawnWaveSystem = false;
                }
            }
        }

    }
}
