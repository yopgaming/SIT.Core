﻿using Comfort.Common;
using EFT.UI;
using Microsoft.Win32;
using SIT.Tarkov.Core;
using StayInTarkov;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace SIT.Core.Core
{
    public class LegalGameCheck
    {
        public static string IllegalMessage { get; }
            = StayInTarkovPlugin.LanguageDictionaryLoaded && StayInTarkovPlugin.LanguageDictionary.ContainsKey("ILLEGAL_MESSAGE")
            ? StayInTarkovPlugin.LanguageDictionary["ILLEGAL_MESSAGE"]
            : "Illegal game found. Please buy, install and launch the game once.";

        public static bool Checked { get; private set; } = false;
        public static bool LegalGameFound { get; private set; } = false;

        public static bool LegalityCheck(BepInEx.Configuration.ConfigFile config)
        {
            if (Checked || LegalGameFound)
                return LegalGameFound;

            // SIT Legal Game Checker
            var lcRemover = config.Bind<bool>("Debug Settings", "LC Remover", false).Value;
            if (lcRemover)
            {
                LegalGameFound = true;
                Checked = true;
                return LegalGameFound;
            }

            try
            {
                var gamefilePath = RegistryManager.GamePathEXE;
                if (LC1A(gamefilePath))
                {
                    if (LC2B(gamefilePath))
                    {
                        if (LC3C(gamefilePath))
                        {
                            PatchConstants.Logger.LogInfo("Legal Game Found. Thanks for supporting BSG!");
                            Checked = true;
                            LegalGameFound = true;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PatchConstants.Logger.LogError(ex.ToString());
            }
                                 
            Checked = true;
            LegalGameFound = false;
            PatchConstants.Logger.LogError(IllegalMessage);
            return false;
        }

        internal static bool LC1A(string gfp)
        {
            var fiGFP = new FileInfo(gfp);
            return (fiGFP.Exists && fiGFP.Length >= 647 * 1000);
        }

        internal static bool LC2B(string gfp)
        {
            var fiBE = new FileInfo(gfp.Replace(".exe", "_BE.exe"));
            return (fiBE.Exists && fiBE.Length >= 1024000);
        }

        internal static bool LC3C(string gfp)
        {
            var diBattlEye = new DirectoryInfo(gfp.Replace("EscapeFromTarkov.exe", "BattlEye"));
            return (diBattlEye.Exists);
        }

    }
}
