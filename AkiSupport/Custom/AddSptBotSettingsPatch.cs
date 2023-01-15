using EFT;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.AkiSupport.Custom
{
    public class AddSptBotSettingsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotSettingsRepoClass).GetMethod("Init");
        }

        public static long sptUsecValue = 0x80000000;
        public static long sptBearValue = 0x100000000;



        [PatchPrefix]
        private static void PatchPrefix(ref Dictionary<WildSpawnType, BotSettingsValuesClass> ___dictionary_0)
        {
            if (___dictionary_0.ContainsKey((WildSpawnType)sptUsecValue))
            {
                return;
            }

            ___dictionary_0.Add((WildSpawnType)sptUsecValue, new BotSettingsValuesClass(false, false, false, EPlayerSide.Savage.ToStringNoBox()));
            ___dictionary_0.Add((WildSpawnType)sptBearValue, new BotSettingsValuesClass(false, false, false, EPlayerSide.Savage.ToStringNoBox()));
        }
    }
}
