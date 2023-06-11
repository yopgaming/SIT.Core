using Comfort.Common;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop
{
    public static class ItemFinder
    {
        public static bool TryFindItemOnPlayer(EFT.Player player, string templateId, string itemId, out EFT.InventoryLogic.Item item)
        {
            item = null;

            var allItemsOfTemplate = player.Profile.Inventory.GetAllItemByTemplate(templateId);

            if (!allItemsOfTemplate.Any())
                return false;

            item = allItemsOfTemplate.FirstOrDefault(x => x.Id == itemId);
            return item != null;
        }

        public static bool TryFindItemInWorld(string itemId, out EFT.InventoryLogic.Item item)
        {
            item = null;

            var itemFindResult = Singleton<GameWorld>.Instance.FindItemById(itemId);
            if (itemFindResult.Succeeded)
            {
                item = itemFindResult.Value;
            }

            return item != null;

        }
    }
}
