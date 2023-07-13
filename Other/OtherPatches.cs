using BepInEx;
using BepInEx.Logging;
using SIT.Core.Other.AI.DrakiaXYZ.BigBrain;
using SIT.Core.Other.UI;

namespace SIT.Core.Other
{
    public class OtherPatches
    {
        private static BepInEx.Configuration.ConfigFile m_Config;
        public static ManualLogSource Logger { get; private set; }

        static string ConfigSITOtherCategoryValue { get; } = "SIT.Other";

        public static void Run(BepInEx.Configuration.ConfigFile config, BaseUnityPlugin plugin)
        {
            m_Config = config;

            if (Logger == null)
                Logger = BepInEx.Logging.Logger.CreateLogSource("Coop");

            var enabled = config.Bind<bool>(ConfigSITOtherCategoryValue, "Enable", true);
            if (!enabled.Value) // if it is disabled. stop all Other Patches stuff.
            {
                Logger.LogInfo("Other patches have been disabled! Ignoring Patches.");
                return;
            }

            if (config.Bind<bool>(ConfigSITOtherCategoryValue, "EnableAdditionalAmmoUIDescriptions", true).Value)
                new Ammo_CachedReadOnlyAttributes_Patch().Enable();

            if (config.Bind<bool>(ConfigSITOtherCategoryValue, "EnableBigBrain", true).Value)
                new BigBrainPatch();

        }
    }
}
