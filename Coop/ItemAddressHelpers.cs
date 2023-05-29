using EFT.InventoryLogic;
using SIT.Tarkov.Core;
using System.Collections.Generic;

namespace SIT.Core.Coop
{
    internal static class ItemAddressHelpers
    {
        private static string DICTNAMES_SlotItemAddressDescriptor { get; } = "sitad";

        public static void ConvertItemAddressToDescriptor(ItemAddress location, ref Dictionary<string, object> dictionary)
        {
            if (location is GridItemAddress gridItemAddress)
            {
                GridItemAddressDescriptor gridItemAddressDescriptor = new GridItemAddressDescriptor();
                gridItemAddressDescriptor.Container = new ContainerDescriptor();
                gridItemAddressDescriptor.Container.ContainerId = location.Container.ID;
                gridItemAddressDescriptor.Container.ParentId = location.Container.ParentItem != null ? location.Container.ParentItem.Id : null;
                gridItemAddressDescriptor.LocationInGrid = gridItemAddress.LocationInGrid;
                dictionary.Add("grad", gridItemAddressDescriptor);
            }
            else if (location is SlotItemAddress slotItemAddress)
            {
                SlotItemAddressDescriptor slotItemAddressDescriptor = new SlotItemAddressDescriptor();
                slotItemAddressDescriptor.Container = new ContainerDescriptor();
                slotItemAddressDescriptor.Container.ContainerId = location.Container.ID;
                slotItemAddressDescriptor.Container.ParentId = location.Container.ParentItem != null ? location.Container.ParentItem.Id : null;

                dictionary.Add(DICTNAMES_SlotItemAddressDescriptor, slotItemAddressDescriptor);
            }
        }

        public static void ConvertDictionaryToAddress(
            Dictionary<string, object> dict,
            out GridItemAddressDescriptor gridItemAddressDescriptor,
            out SlotItemAddressDescriptor slotItemAddressDescriptor
            )
        {
            gridItemAddressDescriptor = null;
            slotItemAddressDescriptor = null;
            if (dict.ContainsKey("grad"))
            {
                gridItemAddressDescriptor = PatchConstants.SITParseJson<GridItemAddressDescriptor>(dict["grad"].ToString());
            }

            if (dict.ContainsKey(DICTNAMES_SlotItemAddressDescriptor))
            {
                slotItemAddressDescriptor = PatchConstants.SITParseJson<SlotItemAddressDescriptor>(dict[DICTNAMES_SlotItemAddressDescriptor].ToString());
            }
        }
    }
}
