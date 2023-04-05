using Comfort.Common;
using EFT;
using SIT.Core.Coop;
using SIT.Core.Misc;
using SIT.Core.SP.PlayerPatches.Health;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;

namespace SIT.Core.SP.PlayerPatches
{
    public class OfflineSaveProfile : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            foreach (var method in ReflectionHelpers.GetAllMethodsForType(typeof(TarkovApplication)))
            {
                if (method.Name.StartsWith("method") &&
                    method.GetParameters().Length >= 3 &&
                    method.GetParameters()[0].Name == "profileId" &&
                    method.GetParameters()[1].Name == "savageProfile" &&
                    method.GetParameters()[2].Name == "location" &&
                    method.GetParameters().Any(x => x.Name == "result") &&
                    method.GetParameters()[method.GetParameters().Length - 1].Name == "timeHasComeScreenController"
                    )
                {
                    //Logger.Log(BepInEx.Logging.LogLevel.Info, method.Name);
                    return method;
                }
            }
            Logger.Log(BepInEx.Logging.LogLevel.Error, "OfflineSaveProfile::Method is not found!");

            return null;
        }

        [PatchPrefix]
        public static bool PatchPrefix(string profileId, RaidSettings ____raidSettings, TarkovApplication __instance, Result<ExitStatus, TimeSpan, object> result)
        {
            // Get scav or pmc profile based on IsScav value
            var profile = ____raidSettings.IsScav
                ? __instance.GetClientBackEndSession().ProfileOfPet
                : __instance.GetClientBackEndSession().Profile;

            var currentHealth = HealthListener.Instance.CurrentHealth;
            SaveProfileProgress(result.Value0, profile, currentHealth, ____raidSettings.IsScav);


            var coopGC = CoopGameComponent.GetCoopGameComponent();
            if (coopGC != null)
            {
                UnityEngine.Object.Destroy(coopGC);
            }

            return true;
        }

        public static void SaveProfileProgress(ExitStatus exitStatus, Profile profileData, PlayerHealth currentHealth, bool isPlayerScav)
        {
            SaveProfileRequest request = new SaveProfileRequest
            {
                exit = exitStatus.ToString().ToLower(),
                profile = profileData,
                health = currentHealth,
                //health = profileData.Health,
                isPlayerScav = isPlayerScav
            };

            var convertedJson = request.SITToJson();
            Request.Instance.PostJson("/raid/profile/save", convertedJson);

        }

        public class SaveProfileRequest
        {
            public string exit { get; set; }
            public Profile profile { get; set; }
            public bool isPlayerScav { get; set; }
            public object health { get; set; }
        }
    }
}
