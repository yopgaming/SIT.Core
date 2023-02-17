using HarmonyLib;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop
{
    public abstract class ModuleReplicationPatch : ModulePatch
    {
        public static List<ModuleReplicationPatch> Patches { get; } = new List<ModuleReplicationPatch>();

        public ModuleReplicationPatch() { Patches.Add(this); }

        public abstract Type InstanceType { get; }
        public abstract string MethodName { get; }


        public override List<HarmonyMethod> GetPatchMethods(Type attributeType)
        {
            var methods = base.GetPatchMethods(attributeType);

            foreach (var method in GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (method.GetCustomAttribute(attributeType) != null)
                {
                    methods.Add(new HarmonyMethod(method));
                }
            }

            return methods;
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
