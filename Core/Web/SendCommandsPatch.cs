using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Core.Web
{
    /// <summary>
    /// This patch removes the wait to push changes from Inventory
    /// </summary>
    internal class SendCommandsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BackEnd0.BackEndSession2), "TrySendCommands");
        }

        [PatchPrefix]
        public static bool Prefix(
         
            ref float ___float_0
            )
        {
            ___float_0 = 0;
            return true;
        }
     
    }
}
