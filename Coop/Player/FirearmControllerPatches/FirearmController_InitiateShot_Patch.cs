using EFT;
using EFT.InventoryLogic;
using SIT.Coop.Core.Web;
using SIT.Tarkov.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.Player.FirearmControllerPatches
{
    /// <summary>
    ///  LightAndSoundShot(Vector3 point, Vector3 direction, AmmoTemplate ammoTemplate);
    /// </summary>
    public class FirearmController_InitiateShot_Patch : ModuleReplicationPatch
    {
        public override Type InstanceType => typeof(EFT.Player.FirearmController);
        public override string MethodName => "InitiateShot";
        public MethodInfo Method { get; set; } = null;

        public override bool DisablePatch => true;

        public FirearmController_InitiateShot_Patch()
        {
            dictionary = new Dictionary<string, object>
            {
                { "IsPrimaryActive", string.Empty },
                { "WeaponState", string.Empty },
                { "AmmoAfterShot", string.Empty },
                { "ShotPosition", string.Empty },
                { "ShotDirection", string.Empty },
                { "FireportPosition", string.Empty },
                { "ChamberIndex", string.Empty },
                { "Overheat", string.Empty },
                { "UnderbarrelShot", string.Empty },
                { "Ammo", string.Empty },
                { "m", "InitiateShot" },
                { "t", DateTime.Now.Ticks }
            };
        }

        protected override MethodBase GetTargetMethod()
        {
            Method = PatchConstants.GetMethodForType(InstanceType, MethodName);
            return Method;
        }

        public static Dictionary<string, bool> CallLocally
            = new Dictionary<string, bool>();


        public static Dictionary<string, object> dictionary;

        [PatchPostfix]
        public static void PostPatch(
            ref EFT.Player.FirearmController __instance
            , ref IWeapon weapon, ref BulletClass ammo, ref Vector3 shotPosition, ref Vector3 shotDirection, ref Vector3 fireportPosition, ref int chamberIndex, ref float overheat
            , ref EFT.Player ____player)
        {
            var player = ____player;
            if (player == null)
                return;

            if (CallLocally.TryGetValue(player.Profile.AccountId, out var expecting) && expecting)
            {
                CallLocally.Remove(player.Profile.AccountId);
                return;
            }
        
            if (dictionary == null)
                return;

            // TODO: this dictionary is available to all since its static, need to make this player specific
            dictionary["IsPrimaryActive"] = (weapon == __instance.Item).ToString();
            dictionary["WeaponState"] = weapon.MalfState.State.ToString();
            dictionary["AmmoAfterShot"] = weapon.GetCurrentMagazineCount();
            dictionary["ShotPosition"] = shotPosition.ToJson();
            dictionary["ShotDirection"] = shotDirection.ToJson();
            dictionary["FireportPosition"] = fireportPosition.ToJson();
            dictionary["ChamberIndex"] = chamberIndex.ToString();
            dictionary["Overheat"] = overheat;
            dictionary["UnderbarrelShot"] = weapon.IsUnderbarrelWeapon;
            dictionary["Ammo"] = ammo.TemplateId.ToString();
            dictionary["Weapon"] = weapon.Item.TemplateId.ToString();
            dictionary["t"] = DateTime.Now.Ticks;
         
            ServerCommunication.PostLocalPlayerData(player, dictionary);
        }

        private static List<long> ProcessedCalls = new List<long>();

        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            var timestamp = long.Parse(dict["t"].ToString());
            if (!ProcessedCalls.Contains(timestamp))
                ProcessedCalls.Add(timestamp);
            else
            {
                ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddHours(-1).Ticks);
                return;
            }

            if (CallLocally.ContainsKey(player.Profile.AccountId))
                return;

            if (player.HandsController is EFT.Player.FirearmController firearmCont)
            {
                try
                {
                    CallLocally.Add(player.Profile.AccountId, true);
                    Logger.LogInfo("Replicated: Calling InitiateShot");
                    var ammos = player.Inventory.GetAllItemByTemplate(dict["Ammo"].ToString());
                    if (!ammos.Any())
                        return;

                    var ammo = player.Inventory.GetAllItemByTemplate(dict["Ammo"].ToString()).FirstOrDefault();
                    if (ammo == null)
                        return;

                    //Logger.LogInfo("Replicated: Calling InitiateShot - Found Ammo");
                    var bullet = ammo as BulletClass;
                    if (bullet == null)
                        return;

                    var weaponItem = player.Inventory.GetAllItemByTemplate(dict["Weapon"].ToString()).FirstOrDefault();
                    if (weaponItem == null)
                        return;

                    var weapon = weaponItem as IWeapon;
                    if (weapon == null)
                        return;

                    //Logger.LogInfo("Replicated: Calling InitiateShot - Found Weapon");

                    //dictionary["WeaponState"] = weapon.MalfState.State.ToString();
                    //dictionary["AmmoAfterShot"] = weapon.GetCurrentMagazineCount();
                    var shotPosition = dictionary["ShotPosition"].ToString().ParseJsonTo<Vector3>();
                    //dictionary["ShotDirection"] = shotDirection.ToJson();
                    //dictionary["FireportPosition"] = fireportPosition.ToJson();
                    //dictionary["ChamberIndex"] = chamberIndex.ToString();
                    //dictionary["Overheat"] = overheat;
                    //dictionary["UnderbarrelShot"] = weapon.IsUnderbarrelWeapon;

                    //GetTargetMethod().Invoke(firearmCont,
                    //    new object[] { weapon, ammo, shotPosition, shotDirection, fireportPosition, chamberIndex, overheat);

                }
                catch (Exception e)
                {
                    Logger.LogInfo(e);
                }
            }
        }
    }
}
