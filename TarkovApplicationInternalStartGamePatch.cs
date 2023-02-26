using EFT;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core
{
    internal class TarkovApplicationInternalStartGamePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(TarkovApplication).GetMethod("InternalStartGame", BindingFlags.Instance | BindingFlags.Public);
        }

        [PatchPrefix]
        public static async void Prefix(TarkovApplication __instance)
        {
            Logger.LogInfo("TarkovApplicationInternalStartGamePatch.Prefix");
        }
    }

    //internal class TarkovApplicationOtherStartGamePatch : ModulePatch
    //{
    //    protected override MethodBase GetTargetMethod()
    //    {

    //        return typeof(TarkovApplication).GetMethod("method_29", BindingFlags.Instance | BindingFlags.NonPublic);
    //    }

    //    [PatchPrefix]
    //    public static async void Prefix(TarkovApplication __instance, RaidSettings ____raidSettings)
    //    {
    //        Logger.LogInfo("TarkovApplicationOtherStartGamePatch.Prefix");
    //        ____raidSettings.RaidMode = ERaidMode.Local;
    //    }
    //}
}
