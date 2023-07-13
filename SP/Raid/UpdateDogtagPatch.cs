using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;

namespace SIT.Core.SP.Raid
{
    /// <summary>
    /// Updates the PMC Dogtag with the Killer / Weapon of the Aggressor
    /// </summary>
    class UpdateDogtagPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => ReflectionHelpers.GetMethodForType(typeof(Player), "OnBeenKilledByAggressor");

        [PatchPostfix]
        public static void PatchPostfix(Player __instance, Player aggressor, DamageInfo damageInfo)
        {
            if (__instance.Profile.Info.Side == EPlayerSide.Savage)
                return;

            if (__instance.ProfileId == Singleton<GameWorld>.Instance.MainPlayer.ProfileId)
                return;

            Item dogtagItem = GetDogtagItem(__instance);

            if (dogtagItem == null)
                return;

            object itemComponent = GetItemComponent(dogtagItem);

            if (itemComponent == null)
                return;

            var dogTagComponent = itemComponent as DogtagComponent;
            if (dogTagComponent == null)
                return;

            var victimProfileInfo = __instance.Profile.Info;

            dogTagComponent.AccountId = __instance.Profile.AccountId;
            dogTagComponent.ProfileId = __instance.ProfileId;
            dogTagComponent.Nickname = victimProfileInfo.Nickname;
            dogTagComponent.Side = victimProfileInfo.Side;
            dogTagComponent.KillerName = aggressor.Profile.Info.Nickname;
            dogTagComponent.Time = DateTime.Now;
            dogTagComponent.Status = "Killed by ";
            dogTagComponent.KillerAccountId = aggressor.Profile.AccountId;
            dogTagComponent.KillerProfileId = aggressor.Profile.Id;
            dogTagComponent.WeaponName = damageInfo.Weapon.Name;

            if (__instance.Profile.Info.Experience > 0)
                dogTagComponent.Level = victimProfileInfo.Level;
        }

        public static object GetItemComponent(Item dogtagItem)
        {
            MethodInfo method = ReflectionHelpers.GetAllMethodsForType(dogtagItem.GetType()).FirstOrDefault(x => x.Name == "GetItemComponent");
            MethodInfo generic = method.MakeGenericMethod(typeof(DogtagComponent));
            var itemComponent = generic.Invoke(dogtagItem, null);
            return itemComponent;
        }

        public static Item GetDogtagItem(Player __instance)
        {
            var equipment = ReflectionHelpers.GetAllPropertiesForObject(__instance).FirstOrDefault(x => x.Name == "Equipment").GetValue(__instance);
            var dogtagSlot = ReflectionHelpers.GetAllMethodsForType(equipment.GetType()).FirstOrDefault(x => x.Name == "GetSlot").Invoke(equipment, new object[] { EquipmentSlot.Dogtag });
            var dogtagItem = ReflectionHelpers.GetFieldOrPropertyFromInstance<object>(dogtagSlot, "ContainedItem", false) as Item;
            return dogtagItem;
        }
    }
}
