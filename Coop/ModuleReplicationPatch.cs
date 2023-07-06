using Newtonsoft.Json;
using SIT.Core.Coop.NetworkPacket;
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
                //Logger.LogError($"Attempted to recreate {this.GetType()} Patch");
                return;
            }

            if (!DisablePatch)
                Patches.Add(this);

            LastSent.TryAdd(GetType(), new Dictionary<string, object>());
        }

        public abstract Type InstanceType { get; }
        public Type OverrideInstanceType { get; set; }
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

        public abstract void Replicated(EFT.Player player, Dictionary<string, object> dict);

        protected static ConcurrentDictionary<Type, ConcurrentDictionary<string, ConcurrentBag<long>>> ProcessedCalls = new();

        protected static bool HasProcessed(Type type, EFT.Player player, Dictionary<string, object> dict)
        {
            if (!ProcessedCalls.ContainsKey(type))
                ProcessedCalls.TryAdd(type, new ConcurrentDictionary<string, ConcurrentBag<long>>());

            var playerId = player.ProfileId;
            var timestamp = long.Parse(dict["t"].ToString());
            return HasProcessed(type, playerId, timestamp);

        }

        protected static bool HasProcessed(Type type, EFT.Player player, BasePlayerPacket playerPacket)
        {
            if (!ProcessedCalls.ContainsKey(type))
                ProcessedCalls.TryAdd(type, new ConcurrentDictionary<string, ConcurrentBag<long>>());

            var playerId = player.ProfileId;
            var timestamp = long.Parse(playerPacket.TimeSerializedBetter);
            return HasProcessed(type, playerId, timestamp);
        }

        protected static bool HasProcessed(Type type, string playerId, long timestamp)
        {
            if (!ProcessedCalls.ContainsKey(type))
                ProcessedCalls.TryAdd(type, new ConcurrentDictionary<string, ConcurrentBag<long>>());

            if (!ProcessedCalls[type].ContainsKey(playerId))
            {
                ProcessedCalls[type].TryAdd(playerId, new ConcurrentBag<long>());
            }

            if (!ProcessedCalls[type][playerId].Contains(timestamp))
            {
                ProcessedCalls[type][playerId].Add(timestamp);
                return false;
            }

            return true;
        }

        public static void Replicate(Type type, EFT.Player player, Dictionary<string, object> dict)
        {
            if (!Patches.Any(x => x.GetType().Equals(type)))
                return;

            var p = Patches.Single(x => x.GetType().Equals(type));
            p.Replicated(player, dict);
        }

        public static bool IsHighPingOrAI(EFT.Player player)
        {
            if (CoopGameComponent.GetCoopGameComponent().HighPingMode && player.IsYourPlayer)
                return true;

            return player.IsAI;
        }

        public override void Enable()
        {
            base.Enable();
            if (!Patches.Contains(this))
                Patches.Add(this);
        }

        public override void Disable() { base.Disable(); if (Patches.Contains(this)) Patches.Remove(this); }


        public override bool Equals(object obj)
        {
            if (obj.GetType() == this.GetType())
                return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
