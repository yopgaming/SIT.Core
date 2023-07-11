using BepInEx.Configuration;
using EFT.UI;
using HarmonyLib;
using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Misc
{
    public class VersionLabelPatch : ModulePatch
    {
        private static string _versionLabel;
        private static bool EnableSITVersionLabel { get; set; } = true;

        public VersionLabelPatch(ConfigFile config)
        {
            EnableSITVersionLabel = config.Bind<bool>("SIT.SP", "EnableSITVersionLabel", true).Value;
        }

        protected override MethodBase GetTargetMethod()
        {
            try
            {
                return PatchConstants.EftTypes
                .Single(x => x.GetField("Taxonomy", BindingFlags.Public | BindingFlags.Instance) != null)
                .GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            }
            catch (System.Exception e)
            {
                Logger.LogInfo($"VersionLabelPatch failed {e.Message} {e.StackTrace} {e.InnerException.StackTrace}");
                throw;
            }

        }

        [PatchPostfix]
        internal static void PatchPostfix(
            string major, string minor, string backend, string taxonomy
            , object __result)
        {
            DisplaySITVersionLabel(major, __result);
            Plugin.EFTVersionMajor = major;
            //GetLogger(typeof(VersionLabelPatch)).LogInfo("Postfix");
        }

        private static void DisplaySITVersionLabel(string major, object __result)
        {
            if (!EnableSITVersionLabel)
                return;

            if (string.IsNullOrEmpty(_versionLabel))
            {
                _versionLabel = string.Empty;
                _versionLabel = $"SIT | {Assembly.GetAssembly(typeof(VersionLabelPatch)).GetName().Version} | {major}";
            }

            Traverse.Create(MonoBehaviourSingleton<PreloaderUI>.Instance).Field("_alphaVersionLabel").Property("LocalizationKey").SetValue("{0}");
            Traverse.Create(MonoBehaviourSingleton<PreloaderUI>.Instance).Field("string_2").SetValue(_versionLabel);
            Traverse.Create(__result).Field("Major").SetValue(_versionLabel);
        }
    }
}