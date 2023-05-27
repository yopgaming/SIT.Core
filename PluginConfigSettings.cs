using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core
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
            }

            public bool SETTING_DEBUGSpawnDronesOnServer { get; set; } = false;
            public bool SETTING_DEBUGShowPlayerList { get; set; } = false;
            public int SETTING_Actions_TickRateInMS { get; private set; } = 999;
            public bool SETTING_Actions_AlwaysProcessAllActions { get; private set; }
            public int SETTING_Actions_CutoffTimeInSeconds { get; private set; }
            public int SETTING_PlayerStateTickRateInMS { get; set; } = -1000;
            public bool SETTING_HeadshotsAlwaysKill { get; set; } = true;
            public int SETTING_SIT_Port { get; set; } = 6970;

            public void GetSettings()
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

                SETTING_Actions_TickRateInMS = Plugin.Instance.Config.Bind<int>
                ("Coop", "LastActionTickRateInMS", SETTING_Actions_TickRateInMS, new BepInEx.Configuration.ConfigDescription("The tick rate at which actions acquired from Server. MIN = 250ms. MAX = 999ms")).Value;
                SETTING_Actions_TickRateInMS = Math.Max(250, SETTING_Actions_TickRateInMS);
                SETTING_Actions_TickRateInMS = Math.Min(999, SETTING_Actions_TickRateInMS);

                SETTING_HeadshotsAlwaysKill = Plugin.Instance.Config.Bind<bool>
                  ("Coop", "HeadshotsAlwaysKill", true, new BepInEx.Configuration.ConfigDescription("Enable to make headshots actually work, no more tanking definite kills!")).Value;

                SETTING_SIT_Port = Plugin.Instance.Config.Bind<int>("Coop", "SITPort", 6970, new ConfigDescription("SIT.Core Websocket Port DEFAULT = 6970")).Value;


                Logger.LogDebug($"SETTING_DEBUGSpawnDronesOnServer: {SETTING_DEBUGSpawnDronesOnServer}");
                Logger.LogDebug($"SETTING_DEBUGShowPlayerList: {SETTING_DEBUGShowPlayerList}");
                Logger.LogDebug($"SETTING_PlayerStateTickRateInMS: {SETTING_PlayerStateTickRateInMS}");
                Logger.LogDebug($"SETTING_Actions_AlwaysProcessAllActions: {SETTING_Actions_AlwaysProcessAllActions}");
                Logger.LogDebug($"SETTING_Actions_CutoffTimeInSeconds: {SETTING_Actions_CutoffTimeInSeconds}");
                Logger.LogDebug($"SETTING_Actions_TickRateInMS: {SETTING_Actions_TickRateInMS}");
                Logger.LogDebug($"SETTING_HeadshotsAlwaysKill: {SETTING_HeadshotsAlwaysKill}");
                Logger.LogDebug($"SETTING_SIT_Port: {SETTING_SIT_Port}");
            }
        }

    }
}
