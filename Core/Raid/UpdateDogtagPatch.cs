using EFT;
using EFT.InventoryLogic;
using System;
using System.Linq;
using System.Reflection;

namespace SIT.Tarkov.Core.SP
{
    class UpdateDogtagPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() => PatchConstants.GetMethodForType(typeof(Player), "OnBeenKilledByAggressor");

        [PatchPostfix]
        public static void PatchPostfix(Player __instance, Player aggressor, object damageInfo)
        {
            if (__instance.Profile.Info.Side == EPlayerSide.Savage)
            {
                return;
            }

            var equipment = PatchConstants.GetAllPropertiesForObject(__instance).FirstOrDefault(x => x.Name == "Equipment").GetValue(__instance);
            var dogtagSlot = PatchConstants.GetAllMethodsForType(equipment.GetType()).FirstOrDefault(x => x.Name == "GetSlot").Invoke(equipment, new object[] { EquipmentSlot.Dogtag });
            var dogtagItem = PatchConstants.GetFieldOrPropertyFromInstance<object>(dogtagSlot, "ContainedItem", false) as Item;

            if (dogtagItem == null)
            {
                Logger.LogError("[DogtagPatch] error > DogTag slot item is null somehow.");
                return;
            }

            MethodInfo method = PatchConstants.GetAllMethodsForType(dogtagItem.GetType()).FirstOrDefault(x => x.Name == "GetItemComponent");
            MethodInfo generic = method.MakeGenericMethod(typeof(DogtagComponent));
            var itemComponent = generic.Invoke(dogtagItem, null);

            if (itemComponent == null)
            {
                Logger.LogInfo("[DogtagPatch] error > DogTagComponent on dog tag slot is null");
                return;
            }

            var dogTagComponent = itemComponent as DogtagComponent;
            if (dogTagComponent == null)
            {
                Logger.LogInfo("[DogtagPatch] error > itemComponent is not DogTagComponent");
                return;
            }

            var victimProfileInfo = __instance.Profile.Info;

            dogTagComponent.AccountId = __instance.Profile.AccountId;
            dogTagComponent.ProfileId = __instance.Profile.Id;
            dogTagComponent.Nickname = victimProfileInfo.Nickname;
            dogTagComponent.Side = victimProfileInfo.Side;
            dogTagComponent.KillerName = aggressor.Profile.Info.Nickname;
            dogTagComponent.Time = DateTime.Now;
            dogTagComponent.Status = "Killed by ";
            dogTagComponent.KillerAccountId = aggressor.Profile.AccountId;
            dogTagComponent.KillerProfileId = aggressor.Profile.Id;

            if (__instance.Profile.Info.Experience > 0)
            {
                dogTagComponent.Level = victimProfileInfo.Level;
            }
        }
    }
}
