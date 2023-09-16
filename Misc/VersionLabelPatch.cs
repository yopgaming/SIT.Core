using BepInEx.Configuration;
using EFT.UI;
using HarmonyLib;
using SIT.Tarkov.Core;
using System;
using System.Diagnostics;
using System.IO;
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
            StayInTarkovPlugin.EFTVersionMajor = major;
            //GetLogger(typeof(VersionLabelPatch)).LogInfo("Postfix");
        }

        private static void DisplaySITVersionLabel(string major, object __result)
        {
            if (!EnableSITVersionLabel)
                return;

            if (string.IsNullOrEmpty(_versionLabel))
            {
                _versionLabel = string.Empty;
                var eftPath = string.Empty;
                var eftProcesses = Process.GetProcessesByName("EscapeFromTarkov");
                foreach (var process in eftProcesses)
                {
                    Logger.LogDebug("Process path found");
                    Logger.LogDebug(process.MainModule.FileName);
                    eftPath = process.MainModule.FileName;
                }
                string exeFileVersion = string.Empty;
                if(!string.IsNullOrEmpty(eftPath)) 
                { 
                    FileInfo fileInfoEft = new FileInfo(eftPath);
                    if (fileInfoEft.Exists)
                    {
                        FileVersionInfo myFileVersionInfo =
                            FileVersionInfo.GetVersionInfo(fileInfoEft.FullName);
                        exeFileVersion = myFileVersionInfo.ProductVersion.Split('-')[0] + "." + myFileVersionInfo.ProductVersion.Split('-')[1];
                    }
                }
                string sitversion = Assembly.GetAssembly(typeof(VersionLabelPatch)).GetName().Version.ToString();
                string assemblyVersion = major;
                if (!string.IsNullOrEmpty(exeFileVersion) && assemblyVersion != exeFileVersion)
                {
                    _versionLabel = $"SIT | ERROR | EXE & Assembly mismatch!";
                    Logger.LogInfo($"Assembly {assemblyVersion} does not match {exeFileVersion}");
                }
                else
                    _versionLabel = $"SIT {sitversion} | ASM {assemblyVersion} | EXE {exeFileVersion}";
            }

            Traverse.Create(MonoBehaviourSingleton<PreloaderUI>.Instance).Field("_alphaVersionLabel").Property("LocalizationKey").SetValue("{0}");
            Traverse.Create(MonoBehaviourSingleton<PreloaderUI>.Instance).Field("string_2").SetValue(_versionLabel);
            Traverse.Create(__result).Field("Major").SetValue(_versionLabel);
        }
    }
}