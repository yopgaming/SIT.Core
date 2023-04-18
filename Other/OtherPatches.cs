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

        static string ConfigSITOtherCategoryValue { get; } = "SIT.Other";

        public static void Run(BepInEx.Configuration.ConfigFile config, BaseUnityPlugin plugin)
        {
            m_Config = config;

            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("Coop");

            var enabled = config.Bind<bool>(ConfigSITOtherCategoryValue, "Enable", false);
            if (!enabled.Value) // if it is disabled. stop all Other Patches stuff.
            {
                Logger.LogInfo("Other patches have been disabled! Ignoring Patches.");
                return;
            }

            if (config.Bind<bool>(ConfigSITOtherCategoryValue, "EnablePropsAIBushPatch", false).Value)
                new AIBushPatch().Enable();

            var enableAIWakeOrSleep = config.Bind<bool>(ConfigSITOtherCategoryValue, "EnableAIWakeOrSleepPatch", false);
            if (enableAIWakeOrSleep.Value)
                AIAwakeOrSleepComponent = plugin.GetOrAddComponent<AIAwakeOrSleepComponent>();
        }
    }
}
