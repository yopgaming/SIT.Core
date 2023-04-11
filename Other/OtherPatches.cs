using BepInEx;
using BepInEx.Logging;
using SIT.Core.Other.AI;

namespace SIT.Core.Other
{
    public class OtherPatches
    {
        private static BepInEx.Configuration.ConfigFile m_Config;
        public static ManualLogSource Logger { get; private set; }

        static AIAwakeOrSleepComponent AIAwakeOrSleepComponent { get; set; }

        public static void Run(BepInEx.Configuration.ConfigFile config, BaseUnityPlugin plugin)
        {
            m_Config = config;

            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("Coop");

            var enabled = config.Bind<bool>("Other Patches", "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all Other Patches stuff.
            {
                Logger.LogInfo("Other patches have been disabled! Ignoring Patches.");
                return;
            }

            if (config.Bind<bool>("Other Patches", "Enable Props AI Bush Patch", true).Value)
                new AIBushPatch().Enable();

            //var enableAIWakeOrSleep = config.Bind<bool>("Other Patches", "Enable AI Wake or Sleep Patch", true);
            //if (enableAIWakeOrSleep.Value)
            //    AIAwakeOrSleepComponent = plugin.GetOrAddComponent<AIAwakeOrSleepComponent>();
        }
    }
}
