using Comfort.Common;
using EFT.InventoryLogic;
using EFT;
using Newtonsoft.Json.Linq;
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
using static GClass936;

namespace SIT.Core.Coop.Player
{
    internal class Player_ApplyShot_Patch : ModuleReplicationPatch
    {
        public static Dictionary<string, bool> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "ApplyShot";

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(InstanceType, MethodName);
        }

        [PatchPrefix]
        public static bool PrePatch(EFT.Player __instance)
        {
            var result = false;
            if (CallLocally.TryGetValue(__instance.Profile.AccountId, out var expecting) && expecting)
                result = true;

            return result;
        }

        [PatchPostfix]
        public static void PostPatch(
           EFT.Player __instance,
            DamageInfo damageInfo, EBodyPart bodyPartType, ShotID shotId
            )
        {
            var player = __instance;

            //Logger.LogDebug("Player_ApplyShot_Patch:PostPatch");


            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }

            Dictionary<string, object> packet = new Dictionary<string, object>();
            damageInfo.HitCollider = null;
            damageInfo.HittedBallisticCollider = null;
            Dictionary<string, string> playerDict = new Dictionary<string, string>();
            try
            {
                if (damageInfo.Player != null)
                {
                    playerDict.Add("d.p.aid", damageInfo.Player.Profile.AccountId);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            damageInfo.Player = null;
            Dictionary<string, string> weaponDict = new Dictionary<string, string>();

            if (damageInfo.Weapon != null)
            {
                packet.Add("d.w.tpl", damageInfo.Weapon.TemplateId);
                packet.Add("d.w.id", damageInfo.Weapon.Id);
            }
            damageInfo.Weapon = null;

            var shotammoid_field = ReflectionHelpers.GetFieldFromType(typeof(ShotID), "string_0");
            string shotammoid = null;
            if (shotammoid_field != null)
            {
                shotammoid = shotammoid_field.GetValue(shotId).ToString();
                //Logger.LogDebug(shotammoid);
            }

            packet.Add("t", DateTime.Now.Ticks);
            packet.Add("d", SerializeObject(damageInfo));
            packet.Add("d.p", playerDict);
            packet.Add("d.w", weaponDict);
            packet.Add("bpt", bodyPartType.ToString());
            packet.Add("ammoid", shotammoid);
            packet.Add("m", "ApplyShot");
            ServerCommunication.PostLocalPlayerData(player, packet);
        }

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (player == null)
                return;

            if (dict == null)
                return;

            if (HasProcessed(GetType(), player, dict))
                return;

            if (CallLocally.ContainsKey(player.Profile.AccountId))
                return;

            try
            {
                Enum.TryParse<EBodyPart>(dict["bpt"].ToString(), out var bodyPartType);

                var damageInfo = BuildDamageInfoFromPacket(dict);

                var shotId = new ShotID();
                if(dict.ContainsKey("ammoid") && dict["ammoid"] != null)
                {
                    shotId = new ShotID(dict["ammoid"].ToString(), 1);
                }

                CallLocally.Add(player.Profile.AccountId, true);
                player.ApplyShot(damageInfo, bodyPartType, shotId);
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }
        }

        public static DamageInfo BuildDamageInfoFromPacket(Dictionary<string, object> dict)
        {
            var damageInfo = JObject.Parse(dict["d"].ToString()).ToObject<DamageInfo>();

            EFT.Player aggressorPlayer = null;
            if (dict.ContainsKey("d.p") && dict["d.p"] != null && damageInfo.Player == null)
            {
                Dictionary<string, string> playerDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(dict["d.p"].ToString());
                if (playerDict != null && playerDict.ContainsKey("d.p.aid"))
                {
                    var coopGC = CoopGameComponent.GetCoopGameComponent();
                    if (coopGC != null)
                    {
                        var accountId = playerDict["d.p.aid"];
                        if (coopGC.Players.ContainsKey(accountId))
                        {
                            aggressorPlayer = coopGC.Players[accountId];
                            damageInfo.Player = aggressorPlayer;
                        }
                        else
                        {
                            aggressorPlayer = Singleton<GameWorld>.Instance.RegisteredPlayers.FirstOrDefault(x => x.Profile.AccountId == accountId);
                            if (aggressorPlayer != null)
                                damageInfo.Player = aggressorPlayer;
                        }
                    }
                }
            }

            if (dict.ContainsKey("d.w.tpl") && dict["d.w.tpl"] != null)
            {
                //Logger.LogDebug("Apply Damage: Found d.w.tpl");
                if (aggressorPlayer != null)
                {
                    if (aggressorPlayer.Inventory.GetAllItemByTemplate(dict["d.w.tpl"].ToString()).Any())
                    {
                        //Logger.LogDebug("Apply Damage: Found Template in Player Inventory");
                        var w = aggressorPlayer.Inventory.GetAllItemByTemplate(dict["d.w.tpl"].ToString()).First() as Weapon;
                        if (w != null)
                        {
                            //Logger.LogDebug("Apply Damage: Found Weapon in Player Inventory");
                            damageInfo.Weapon = w;
                        }
                    }
                }
            }

            return damageInfo;
        }
    }

}
