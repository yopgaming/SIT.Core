using SIT.Tarkov.Core;
using SIT.Coop.Core.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EFT;
using System.Collections.Concurrent;

namespace SIT.Coop.Core.Player
{
    internal class PlayerOnInventoryOpenedPatch : ModulePatch
    {

        public static ConcurrentDictionary<EFT.Player, bool> LastOpenedValue = new ConcurrentDictionary<EFT.Player, bool>();

        /// <summary>
		/// public override void Say(EPhraseTrigger @event, bool demand = false, float delay = 0f, ETagStatus mask = (ETagStatus)0, int probability = 100, bool aggressive = false)
        /// 
        /// </summary>
        /// <returns></returns>
        protected override MethodBase GetTargetMethod()
        {
            var t = SIT.Tarkov.Core.PatchConstants.EftTypes.FirstOrDefault(x => x.FullName == "EFT.Player");
            if (t == null)
                Logger.LogInfo($"PlayerOnInventoryOpened:Type is NULL");

            var method = PatchConstants.GetAllMethodsForType(t)
                .FirstOrDefault(x => x.Name == "SetInventoryOpened"
                );

            Logger.LogInfo($"PlayerOnInventoryOpenedPatch:{t.Name}:{method.Name}");
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player __instance,
            bool opened)
        {
            if (
                !LastOpenedValue.ContainsKey(__instance)
                || (LastOpenedValue.ContainsKey(__instance) && LastOpenedValue[__instance] != opened))
            {
                //Logger.LogInfo("PlayerOnInventoryOpenedPatch.PatchPostfix");
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary.Add("opened", opened);
                //if (!opened)
                //    dictionary.Add("p.equip", __instance.Inventory.Equipment.SITToJson());
                dictionary.Add("m", "InventoryOpened");
                ServerCommunication.PostLocalPlayerData(__instance, dictionary);
                LastOpenedValue.TryAdd(__instance, opened);
                LastOpenedValue[__instance] = opened;
            }

            return false;

        }

        internal static void Replicated(LocalPlayer player, Dictionary<string, object> packet)
        {
            //Logger.LogInfo("InventoryOpened");
            var opened = bool.Parse(packet["opened"].ToString());
            //player.SetInventoryOpened(opened);
            player.HandsController.SetInventoryOpened(opened);

            if (!opened && packet.ContainsKey("p.equip"))
            {
                var equip = PatchConstants.SITParseJson<Equipment>(packet["p.equip"].ToString());
                player.Inventory.Equipment = equip;
            }
        }
    }
}
