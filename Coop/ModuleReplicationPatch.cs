using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SIT.Core.Coop
{
    public abstract class ModuleReplicationPatch : ModulePatch, IModuleReplicationPatch
    {
        public static List<ModuleReplicationPatch> Patches { get; } = new List<ModuleReplicationPatch>();

        public ModuleReplicationPatch()
        {
            if (Patches.Any(x => x.GetType() == this.GetType()))
            {
                Logger.LogError($"Attempted to recreate {this.GetType()} Patch");
                return;
            }

            Patches.Add(this);
            LastSent.TryAdd(GetType(), new Dictionary<string, object>());
        }

        public abstract Type InstanceType { get; }
        public abstract string MethodName { get; }

        public virtual bool DisablePatch { get; } = false;

        protected static ConcurrentDictionary<Type, Dictionary<string, object>> LastSent = new();


        public static string SerializeObject(object o)
        {
            try
            {
                return o.SITToJson();
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
            return string.Empty;
        }

        public static T DeserializeObject<T>(string s)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(s, PatchConstants.GetJsonSerializerSettings());
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
            }
            return default(T);
        }

        public virtual void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {

        }

        public static void Replicate(Type type, EFT.Player player, Dictionary<string, object> dict)
        {
            if (!Patches.Any(x => x.GetType().Equals(type)))
                return;

            var p = Patches.Single(x => x.GetType().Equals(type));
            p.Replicated(player, dict);
        }
    }
}
