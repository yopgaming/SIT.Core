using ChartAndGraph;
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

            if (!string.IsNullOrEmpty(templateId))
            {
                var allItemsOfTemplate = player.Profile.Inventory.GetAllItemByTemplate(templateId);

                if (!allItemsOfTemplate.Any())
                    return false;

                item = allItemsOfTemplate.FirstOrDefault(x => x.Id == itemId);
            }
            else
            {
                item = player.Profile.Inventory.AllPlayerItems.FirstOrDefault(x => x.Id == itemId);
            }
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

        public static bool TryFindItem(string itemId, out EFT.InventoryLogic.Item item)
        {
            if (TryFindItemInWorld(itemId, out item))
                return item != null;
            else
            {
                var coopGC = CoopGameComponent.GetCoopGameComponent();
                var players = coopGC.Players;
                if(players == null)
                    return false;

                foreach( var player in players )
                {
                    if(TryFindItemOnPlayer(player.Value, null, itemId, out item))
                        return true;
                }
            }

            return false;
        }
    }
}
