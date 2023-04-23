using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using MonoMod.Utils;
using Newtonsoft.Json;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Player
{
    internal sealed class Player_OnItemAddedOrRemoved_Patch : ModuleReplicationPatch
    {
        public static List<string> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "OnItemAddedOrRemoved";

        protected override MethodBase GetTargetMethod()
        {
            var method = ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var result = false;
            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
           Item item, ItemAddress location, bool added
            )
        {
            var player = __instance;

            Logger.LogDebug($"OnItemAddedOrRemoved.PostPatch:{item.TemplateId}:{location.GetType()}:{location.ContainerName}:{added}");
            if (CallLocally.Contains(player.Profile.AccountId))
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }
            SlotItemAddressDescriptor slotItemAddressDescriptor = new SlotItemAddressDescriptor();
            slotItemAddressDescriptor.Container = new ContainerDescriptor();
            slotItemAddressDescriptor.Container.ContainerId = location.Container.ID;
            slotItemAddressDescriptor.Container.ParentId = location.Container.ParentItem != null ? location.Container.ParentItem.Id : null;

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("t", DateTime.Now.Ticks);
            if (location is GridItemAddress gridItemAddress) 
            {
                GridItemAddressDescriptor gridItemAddressDescriptor = new GridItemAddressDescriptor();
                gridItemAddressDescriptor.Container = new ContainerDescriptor();
                gridItemAddressDescriptor.Container.ContainerId = location.Container.ID;
                gridItemAddressDescriptor.Container.ParentId = location.Container.ParentItem != null ? location.Container.ParentItem.Id : null;
                gridItemAddressDescriptor.LocationInGrid = gridItemAddress.LocationInGrid;
                dictionary.Add("grad", gridItemAddressDescriptor);
            }

            dictionary.Add("id", item.Id);
            dictionary.Add("tpl", item.TemplateId);
            dictionary.Add("sitad", slotItemAddressDescriptor);
            dictionary.Add("added", added);
            dictionary.Add("m", "OnItemAddedOrRemoved");
            ServerCommunication.PostLocalPlayerData(player, dictionary);
        }


        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (HasProcessed(GetType(), player, dict))
                return;

            Item item = null;
            //foreach (var l in Singleton<GameWorld>.Instance.AllLoot)
            //{
            //    if (l.Item.TemplateId == dict["tpl"].ToString())
            //    {
            //        item = l.Item;
            //        break;
            //    }
            //}

            var itemFindResult = Singleton<GameWorld>.Instance.FindItemById(dict["id"].ToString());
            if(itemFindResult.Succeeded)
            {
                item = itemFindResult.Value;
                item = item.CloneItem();
            }

            if (item != null) 
            {
                Logger.LogDebug($"OnItemAddedOrRemoved.Replicated:Item[{item}]");
                if (dict.ContainsKey("grad"))
                {
                    Logger.LogDebug($"Has GridItemAddressDescriptor");
                    Logger.LogDebug($"{dict["grad"]}");
                    GridItemAddressDescriptor gridItemAddressDescriptor = PatchConstants.SITParseJson<GridItemAddressDescriptor>(dict["grad"].ToString());
                    var container1 = player.Equipment.FindContainer(gridItemAddressDescriptor.Container.ContainerId, gridItemAddressDescriptor.Container.ParentId);
                    //var container = player.Equipment.GetContainer(gridItemAddressDescriptor.Container.ContainerId);
                    if (container1 != null)
                    {
                        if (bool.Parse(dict["added"].ToString()))
                        {
                            Logger.LogDebug($"OnItemAddedOrRemoved.Replicated:Container[{container1.GetType()}][{container1}]");
                            ((GClass2169)container1).AddItemWithoutRestrictions(item, gridItemAddressDescriptor.LocationInGrid);
                        }
                    }
                }
                else
                {
                    Logger.LogDebug($"Has SlotItemAddressDescriptor");
                    Logger.LogDebug($"{dict["sitad"]}");
                    SlotItemAddressDescriptor slotItemAddressDescriptor = PatchConstants.SITParseJson<SlotItemAddressDescriptor>(dict["sitad"].ToString());
                    var container1 = player.Equipment.FindContainer(slotItemAddressDescriptor.Container.ContainerId, slotItemAddressDescriptor.Container.ParentId);
                    if (container1 != null)
                    {
                        if (bool.Parse(dict["added"].ToString()))
                        {
                            Logger.LogDebug($"OnItemAddedOrRemoved.Replicated:Container[{container1.GetType()}][{container1}]");
                            if (container1 is EFT.InventoryLogic.Slot slot) 
                            {
                                Logger.LogDebug($"OnItemAddedOrRemoved.Replicated:Container[{container1.GetType()}][{container1}]AddWithoutRestrictions");
                                slot.AddWithoutRestrictions(item);
                            }
                        }
                    }
                }

                //player.OnItemAddedOrRemoved(item, )
            }
            //try
            //{
            //    player.CurrentState.Jump();
            //}
            //catch (Exception e)
            //{
            //    Logger.LogInfo(e);
            //}

        }
    }

    //internal static class ItemTransactionHelper
    //{
    //    public static Type ReflectedType { get; }

    //    static ItemTransactionHelper()
    //    {
    //        string[] names = new string[10] { "Sort", "QuickFindAppropriatePlace", "TransferOrMerge", "TransferMax", "Merge", "ApplyItemToRevolverDrum", "ApplySingleItemToAddress", "Fold", "CanRecode", "CanFold" };
    //        ReflectedType = ReflectionHelper.FindClassTypeByMethodNames(names);
    //    }

    //    public static object Move(Item item, ItemAddress to, object itemController, bool simulate = false)
    //    {
    //        return ReflectedType.GetMethod("Move").Invoke(null, new object[4] { item, to, itemController, simulate });
    //    }

    //    public static object TransferOrMerge(Item item, Item targetItem, TraderControllerClass itemController, bool simulate)
    //    {
    //        return ReflectedType.InvokeMethod("TransferOrMerge", new object[4] { item, targetItem, itemController, simulate });
    //    }
    //}

}
