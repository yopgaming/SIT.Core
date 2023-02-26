using EFT.InventoryLogic;
using SIT.Coop.Core.Web;
using SIT.Core.Coop;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class PlayerOnTryProceedPatch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player);

        public override string MethodName => "TryProceed";

        public static Dictionary<string, bool> CallLocally
            = new Dictionary<string, bool>();

        private static List<long> ProcessedCalls
            = new List<long>();

        //public override bool DisablePatch => true;

        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player);
            if (t == null)
                Logger.LogInfo($"PlayerOnTryProceedPatch:Type is NULL");

            var method = PatchConstants.GetMethodForType(t, MethodName);

            //Logger.LogInfo($"PlayerOnTryProceedPatch:{t.Name}:{method.Name}");
            return method;
        }


        [PatchPrefix]
        public static bool PrePatch(
           EFT.Player __instance
            )
        {
            if (__instance.IsAI)
                return true;

            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(EFT.Player __instance
            , Item item
            , bool scheduled)
        {
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(__instance.Profile.AccountId);
                return;
            }

            Logger.LogInfo($"PlayerOnTryProceedPatch:Patch");
            Dictionary<string, object> args = new Dictionary<string, object>();
            args.Add("m", "TryProceed");
            args.Add("t", DateTime.Now.Ticks);
            args.Add("item.id", item.Id);
            args.Add("item.tpl", item.TemplateId);
            args.Add("s", scheduled.ToString());
            ServerCommunication.PostLocalPlayerData(__instance, args);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var t = long.Parse(dict["t"].ToString());
            if (ProcessedCalls.Contains(t))
                return;

            ProcessedCalls.Add(t);
            Logger.LogInfo($"PlayerOnTryProceedPatch:Replicated");

            var item = player.Profile.Inventory.GetAllItemByTemplate(dict["item.tpl"].ToString()).FirstOrDefault();
            if (item != null)
            {
                Logger.LogInfo($"PlayerOnTryProceedPatch:Replicated:Found Item");
                CallLocally.Add(player.Profile.AccountId, true);
                player.TryProceed(item, (IResult) =>
                {
                    Logger.LogInfo($"PlayerOnTryProceedPatch:Replicated:Try Proceed:{IResult.Succeed}");
                }, bool.Parse(dict["s"].ToString()));
            }
        }
    }
}

//        [PatchPrefix]
//        public static bool PrePatch()
//        {
//            return Matchmaker.MatchmakerAcceptPatches.IsSinglePlayer;
//        }

//        [PatchPostfix]
//        public static void Patch(EFT.Player __instance, Item item)
//        {
//            if (Matchmaker.MatchmakerAcceptPatches.IsSinglePlayer)
//                return;

//            if (item == null)
//                return;

//            Logger.LogInfo($"PlayerOnTryProceedPatch:Patch");

//            Dictionary<string, object> args = new Dictionary<string, object>();
//            args.Add("m", "TryProceed");
//            args.Add("item.id", item.Id);
//            args.Add("item.tpl", item.TemplateId);

//            var dictProcessed = ServerCommunication.PostLocalPlayerData(__instance, args);
//            Replicated(__instance, args);

//        }

//        private static ConcurrentBag<string> Processed = new ConcurrentBag<string>();

//        public static void Replicated(EFT.Player player, Dictionary<string, object> packet)
//        {
//            var t = packet["t"].ToString();
//            if (Processed.Contains(t))
//                return;

//            Processed.Add(t);
//            Logger.LogInfo($"PlayerOnTryProceedPatch:Replicated");

//            var item = player.Profile.Inventory.GetAllItemByTemplate(packet["item.tpl"].ToString()).FirstOrDefault();
//            if (item != null)
//            {
//                //player.TryProceed(item, null, false);
//                    if (item is EFT.InventoryLogic.Weapon weapon)
//                    {
//                        player.Proceed(weapon, delegate (Result<IShootController> result)
//                        {
//                            EFT.Player.smethod_0(result, null);
//                        }, false);
//                        return;
//                    }
//                    if (item is ThrowWeap potentialGrenade)
//                    {
//                    ThrowWeap throwWeap = potentialGrenade;
//                        player.Proceed(throwWeap, delegate (Result<GInterface104> result)
//                        {
//                            EFT.Player.smethod_0(result, null);
//                        }, false);
//                        return;
//                    }
//                    if (item is Meds meds)
//                    {
//                        player.Proceed(meds, EBodyPart.Common, null, item.GetRandomAnimationVariant(), false);
//                        return;
//                    }
//                if (item is FoodDrink foodDrink)
//                {
//                    FoodDrink foodDrink2 = foodDrink;
//                    player.Proceed(foodDrink2, 1f, null, item.GetRandomAnimationVariant(), false);
//                    return;
//                }

//                PlayerOnProceedKnifePatch.ProceedWeaponReplicated(player, packet);
//                //KnifeComponent itemComponent = item.GetItemComponent<KnifeComponent>();
//                //if (itemComponent != null)
//                //{
//                //    this.Proceed(itemComponent, delegate (Result<IKnifeHandsController> result)
//                //    {
//                //        Player.smethod_0(result, CS$<> 8__locals0.completeCallback);
//                //    }, scheduled);
//                //}
//            }

//        }
//    }
//}
