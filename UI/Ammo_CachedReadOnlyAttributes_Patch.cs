using EFT.InventoryLogic;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Core.Other.UI
{
    internal class Ammo_CachedReadOnlyAttributes_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(AmmoTemplate), "GetCachedReadonlyQualities");
        }

        [PatchPostfix]
        private static void Postfix(ref AmmoTemplate __instance, ref List<ItemAttribute> __result)
        {
            if (!__result.Any((ItemAttribute a) => (Attributes.ENewItemAttributeId)a.Id == Attributes.ENewItemAttributeId.Damage))
            {
                AddNewAttributes(ref __result, __instance);
            }
        }

        public static void AddNewAttributes(ref List<ItemAttribute> attributes, AmmoTemplate template)
        {
            if (template == null)
                return;

            // Damage
            if (template.Damage > 0)
            {
                attributes.Add(
                    new ItemAttribute(Attributes.ENewItemAttributeId.Damage)
                    {
                        Name = Attributes.ENewItemAttributeId.Damage.GetName(),
                        Base = (() => (float)template.Damage),
                        StringValue = (() => template.Damage.ToString()),
                        DisplayType = (() => EItemAttributeDisplayType.Compact)
                    }
                );
            }

            // Armor Damage
            if (template.ArmorDamage > 0)
            {
                attributes.Add(
                    new ItemAttribute(Attributes.ENewItemAttributeId.ArmorDamage)
                    {
                        Name = Attributes.ENewItemAttributeId.ArmorDamage.GetName(),
                        Base = (() => (float)template.ArmorDamage),
                        StringValue = (() => template.ArmorDamage.ToString()),
                        DisplayType = (() => EItemAttributeDisplayType.Compact)
                    }
                );
            }

            // Penetration
            if (template.PenetrationPower > 0)
            {
                attributes.Add(
                    new ItemAttribute(Attributes.ENewItemAttributeId.Penetration)
                    {
                        Name = Attributes.ENewItemAttributeId.Penetration.GetName(),
                        Base = (() => (float)template.PenetrationPower),
                        StringValue = (() => template.PenetrationPower.ToString()),
                        DisplayType = (() => EItemAttributeDisplayType.Compact)
                    }
                );
            }
        }
    }

    public static class Attributes
    {
        public static string GetName(this Attributes.ENewItemAttributeId id)
        {
            switch (id)
            {
                case Attributes.ENewItemAttributeId.Damage:
                    return "DAMAGE";
                case Attributes.ENewItemAttributeId.ArmorDamage:
                    return "ARMOR DAMAGE";
                case Attributes.ENewItemAttributeId.Penetration:
                    return "PENETRATION";
                case Attributes.ENewItemAttributeId.FragmentationChance:
                    return "FRAGMENTATION CHANCE";
                case Attributes.ENewItemAttributeId.RicochetChance:
                    return "RICOCHET CHANCE";
                default:
                    return id.ToString();
            }
        }

        public enum ENewItemAttributeId
        {
            Damage,
            ArmorDamage,
            Penetration,
            FragmentationChance,
            RicochetChance
        }
    }
}
