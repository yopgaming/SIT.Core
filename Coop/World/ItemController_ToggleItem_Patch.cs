using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Coop.World
{
    internal class ItemController_ToggleItem_Patch : ModulePatch, IModuleReplicationWorldPatch
    {
        public Type InstanceType => typeof(ItemController);
        public string MethodName => "ToggleItem";

        public bool DisablePatch => false;

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        public static Dictionary<string, bool> CallLocally
            = new();


        [PatchPrefix]
        public static bool PrePatch(
            ItemController __instance)
        {
            var result = false;
            if (CallLocally.TryGetValue(__instance.RootItem.Id, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(ItemController __instance)
        {
            //var player = ReflectionHelpers.GetAllFieldsForObject(__instance).First(x => x.Name == "_player").GetValue(__instance) as EFT.Player;
            //if (player == null)
            //    return;

            if (CallLocally.TryGetValue(__instance.RootItem.Id, out var expecting) && expecting)
            {
                CallLocally.Remove(__instance.RootItem.Id);
                return;
            }


            Logger.LogInfo("ItemController_ToggleItem_Patch.PostPatch");
            Logger.LogInfo(__instance.RootItem.Id);

            //Dictionary<string, object> dictionary = new Dictionary<string, object>();
            //dictionary.Add("t", DateTime.Now.Ticks);
            //dictionary.Add("m", "ToggleLauncher");
            //Request.Instance.SendDataToPool(dictionary.ToJson());
        }

        public void Replicated(Dictionary<string, object> packet)
        {
            if (HasProcessed(packet))
                return;

            Logger.LogInfo("ItemController_ToggleItem_Patch.Replicated");

        }


        static ConcurrentBag<long> ProcessedCalls = new();

        protected static bool HasProcessed(Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());

            if (!ProcessedCalls.Contains(timestamp))
            {
                ProcessedCalls.Add(timestamp);
                return false;
            }

            return true;
        }
    }
}
