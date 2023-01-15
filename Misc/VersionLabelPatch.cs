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
        internal static void PatchPostfix(object __result)
        {
            if (string.IsNullOrEmpty(_versionLabel))
            {
                _versionLabel = string.Empty;
                _versionLabel = "SIT Powered by Aki";
                //var json = new Request().GetJson("/singleplayer/settings/version");
                //_versionLabel = Json.Deserialize<VersionResponse>(json).Version;
                //Logger.LogInfo($"Server version: {_versionLabel}");
            }

            Traverse.Create(MonoBehaviourSingleton<PreloaderUI>.Instance).Field("_alphaVersionLabel").Property("LocalizationKey").SetValue("{0}");
            Traverse.Create(MonoBehaviourSingleton<PreloaderUI>.Instance).Field("string_2").SetValue(_versionLabel);
            Traverse.Create(__result).Field("Major").SetValue(_versionLabel);
        }
    }
}