using BepInEx.Logging;
using SIT.Core.Other.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Other
{
    public class OtherPatches
    {
        private static BepInEx.Configuration.ConfigFile m_Config;
        public static ManualLogSource Logger { get; private set; }

        public static void Run(BepInEx.Configuration.ConfigFile config)
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

            if(config.Bind<bool>("Other Patches", "Enable Props AI Bush Patch", true).Value)
                new AIBushPatch().Enable();
        }
    }
}
