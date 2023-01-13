using EFT;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.AkiSupport
{
    //public class AddSptBotSettingsPatch : ModulePatch
    //{
    //    protected override MethodBase GetTargetMethod()
    //    {
    //        return typeof(BotSettingsRepoClass).GetMethod("Init");
    //    }

    //    [PatchPrefix]
    //    private static void PatchPrefix(ref Dictionary<WildSpawnType, BotSettingsValuesClass> ___dictionary_0)
    //    {
    //        if (___dictionary_0.ContainsKey((WildSpawnType)AkiBotsPrePatcher.sptUsecValue))
    //        {
    //            return;
    //        }

    //        ___dictionary_0.Add((WildSpawnType)AkiBotsPrePatcher.sptUsecValue, new BotSettingsValuesClass(false, false, false, EPlayerSide.Savage.ToStringNoBox()));
    //        ___dictionary_0.Add((WildSpawnType)AkiBotsPrePatcher.sptBearValue, new BotSettingsValuesClass(false, false, false, EPlayerSide.Savage.ToStringNoBox()));
    //    }
    //}
}
