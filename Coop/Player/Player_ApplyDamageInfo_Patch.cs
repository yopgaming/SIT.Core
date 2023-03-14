#region OLD
//using EFT;
//using Newtonsoft.Json;
////using SIT.Coop.Core.HelpfulStructs;
//using SIT.Tarkov.Core;
//using SIT.Coop.Core.Web;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using SIT.Tarkov.Core.PlayerPatches.Health;
//using EFT.InventoryLogic;
//using System.Collections.Concurrent;
//using System.Runtime.Serialization.Formatters.Binary;
//using UnityEngine.Networking;
//using Comfort.Common;
//using SIT.Core.PlayerPatches.Health;

//namespace SIT.Coop.Core.Player
//{
//    internal class PlayerOnDamagePatch : ModulePatch
//    {
//        private static Type BeingHitType;

//        public PlayerOnDamagePatch()
//        {
//            BeingHitType = PatchConstants.EftTypes.Single(x => ReflectionHelpers.GetMethodForType(x, "BeingHitAction") != null);
//        }

//        protected override MethodBase GetTargetMethod()
//        {
//            var t = typeof(EFT.Player);
//            if (t == null)
//                Logger.LogInfo($"PlayerOnDamagePatch:Type is NULL");

//            var method = ReflectionHelpers.GetMethodForType(t, "ApplyDamageInfo");

//            Logger.LogInfo($"PlayerOnDamagePatch:{t.Name}:{method.Name}");
//            return method;
//        }

//        //[PatchPrefix]
//        //public static void PatchPrefix(object gesture)
//        //{
//        //    Logger.LogInfo("OnGesturePatch.PatchPrefix");
//        //}

//        [PatchPrefix]
//        public static bool PrePatch(
//            )
//        {
//            return false;// Matchmaker.MatchmakerAcceptPatches.IsSinglePlayer;
//        }

//        [PatchPostfix]
//        public static void PatchPostfix(
//            EFT.Player __instance
//            , DamageInfo damageInfo
//            , EBodyPart bodyPartType
//            , float absorbed
//            , object headSegment
//            )
//        {
//            //Logger.LogInfo("PlayerOnDamagePatch.PatchPostfix");
//            Dictionary<string, object> dictionary = new Dictionary<string, object>();
//            //DamageInfo damageI = JsonConvert.DeserializeObject<DamageInfo>(JsonConvert.SerializeObject(damageInfo, settings: new JsonSerializerSettings() { MaxDepth = 0, ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
//            //dictionary.Add("armorDamage", ReflectionHelpers.GetFieldFromType(damageInfo.GetType(), "ArmorDamage").GetValue(damageInfo));
//            dictionary.Add("armorDamage", damageInfo.ArmorDamage);
//            dictionary.Add("bodyPart", bodyPartType);
//            ////dictionary.Add("bodyPartColliderType", damageI.BodyPartColliderType);
//            ////dictionary.Add("damage", ReflectionHelpers.GetFieldFromType(damageInfo.GetType(), "Damage").GetValue(damageInfo));
//            dictionary.Add("damage", damageInfo.Damage);
//            ////dictionary.Add("damageType", ReflectionHelpers.GetFieldFromType(damageInfo.GetType(), "DamageType").GetValue(damageInfo));
//            dictionary.Add("damageType", damageInfo.DamageType);
//            ////dictionary.Add("damageType", damageI.DamageType);
//            //dictionary.Add("deflectedBy", damageInfo.DeflectedBy);
//            dictionary.Add("didArmorDamage", damageInfo.DidArmorDamage);
//            dictionary.Add("didBodyDamage", damageInfo.DidBodyDamage);
//            dictionary.Add("direction", damageInfo.Direction);
//            dictionary.Add("hitNormal", damageInfo.HitNormal);
//            dictionary.Add("hitPoint", damageInfo.HitPoint);
//            dictionary.Add("lightBleedingDelta", damageInfo.LightBleedingDelta);
//            dictionary.Add("masterOrigin", damageInfo.MasterOrigin);
//            ////dictionary.Add("didArmorDamage", damageI.DidArmorDamage);
//            ////dictionary.Add("didBodyDamage", damageI.DidBodyDamage);
//            ////dictionary.Add("direction", damageI.Direction);
//            ////dictionary.Add("heavyBleedingDelta", damageI.HeavyBleedingDelta);
//            //////dictionary.Add("hitCollider", damageInfo.HitCollider);
//            ////dictionary.Add("hitNormal", damageI.HitNormal);
//            ////dictionary.Add("hitPoint", damageI.HitPoint);
//            ////dictionary.Add("lightBleedingDelta", damageI.LightBleedingDelta);
//            ////dictionary.Add("masterOrigin", damageI.MasterOrigin);
//            //if (damageInfo.OverDamageFrom.HasValue)
//            //    dictionary.Add("overDamageFrom", damageInfo.OverDamageFrom);
//            //dictionary.Add("penetrationPower", damageI.PenetrationPower);
//            ////if (damageI.Player != null && damageI.Player.Profile != null)
//            ////    dictionary.Add("playerId", damageI.Player.Profile.AccountId);
//            ////dictionary.Add("sourceId", damageI.SourceId);
//            ////if (damageI.Weapon != null)
//            ////    dictionary.Add("weapon", damageI.Weapon.Id);

//            dictionary.Add("absorbed", absorbed);
//            //dictionary.Add("headSegment", headSegment);
//            //foreach(var r in ReflectionHelpers.GetAllPropertiesForObject(damageInfo))
//            //{
//            //    dictionary.Add(r.Name, r.GetValue(damageInfo));
//            //}
//            //foreach (var r in ReflectionHelpers.GetAllFieldsForObject(damageInfo))
//            //{
//            //    dictionary.Add(r.Name, r.GetValue(damageInfo));
//            //}
//            dictionary.Add("m", "Damage");
//            dictionary.Add("t", DateTime.Now.Ticks);
//            ServerCommunication.PostLocalPlayerData(__instance, dictionary, out string returnedData, out var generatedDict);


//            //Logger.LogInfo("PlayerOnDamagePatch.PatchPostfix:Sent");

//        }

//        private static ConcurrentBag<long> ProcessedDamages = new ConcurrentBag<long>();

//        public static void DamageReplicated(EFT.Player player, Dictionary<string, object> dict)
//        {

//            if (player == null)
//            {
//                Logger.LogError("PlayerOnDamagePatch.DamageReplicated() - ERROR, no player instance");
//                return;
//            }
//            AbstractActiveHealthController ActiveHealthController = player.ActiveHealthController;
//            if (ActiveHealthController == null)
//            {
//                Logger.LogError("PlayerOnDamagePatch.DamageReplicated() - ERROR, no ActiveHealthController instance");
//                return;
//            }

//            //Logger.LogInfo("PlayerOnDamagePatch.DamageReplicated() - Get IsAlive ");

//            bool isAlive = ReflectionHelpers.GetFieldOrPropertyFromInstance<bool>(ActiveHealthController, "IsAlive", false);
//            //DamageInfo damageInfo = new DamageInfo();
//            //Logger.LogInfo("PlayerOnDamagePatch.DamageReplicated() - Get DamageInfo ");

//            //DamageInfo dmI = Json.Deserialize<DamageInfo>(dict["data"].ToString()); // HealthControllerHelpers.CreateDamageInfoTypeFromDict(dict);
//            DamageInfo dmI = new DamageInfo();
//            if (!dict.ContainsKey("damage"))
//                throw new ArgumentNullException("Damage", $"Damage has not been provided!");
//            dmI.Damage = float.Parse(dict["damage"].ToString());
//            if (dmI.Damage <= 0)
//                throw new ArgumentOutOfRangeException("Damage", $"Damage needs to be over 0! Value provided: {dmI.Damage}");
//            //foreach (var r in ReflectionHelpers.GetAllPropertiesForObject(dmI))
//            //{
//            //    if (dict.ContainsKey(r.Name))
//            //    {
//            //        Logger.LogInfo($"PlayerOnDamagePatch.DamageReplicated() - Set DamageInfo.{r.Name}={dict[r.Name]}");
//            //        r.SetValue(dmI, dict[r.Name]);
//            //    }
//            //}
//            //foreach (var r in ReflectionHelpers.GetAllFieldsForObject(dmI))
//            //{
//            //    if (dict.ContainsKey(r.Name))
//            //    {
//            //        Logger.LogInfo($"PlayerOnDamagePatch.DamageReplicated() - Set DamageInfo.{r.Name}={dict[r.Name]}");
//            //        r.SetValue(dmI, dict[r.Name]);
//            //    }
//            //}

//            //Logger.LogInfo("PlayerOnDamagePatch.DamageReplicated() - Get DamageInfo Damage ");

//            var damage = dmI.Damage;
//            //damageInfo.Damage = float.Parse(dict["damage"].ToString());

//            //Logger.LogInfo("PlayerOnDamagePatch.DamageReplicated() - Get bodyPart ");

//            Enum.TryParse<EBodyPart>(dict["bodyPart"].ToString(), out EBodyPart bodyPart);

//            bool autoKillThisDude = (dict.ContainsKey("killThisCunt") ? bool.Parse(dict["killThisCunt"].ToString()) : false);


//            //Logger.LogInfo("PlayerOnDamagePatch.DamageReplicated() - Check processed damages ");

//            var timestampOfDamage = long.Parse(dict["t"].ToString());
//            if (!ProcessedDamages.Contains(timestampOfDamage))
//                ProcessedDamages.Add(timestampOfDamage);
//            else
//            {
//                //Logger.LogInfo("PlayerOnDamagePatch.DamageReplicated() - Ignoring already processed damage ");
//                return;
//            }

//            //EDamageType damageType = damageInfo.DamageType;
//            EDamageType damageType = dmI.DamageType;
//            if (damageType == EDamageType.Undefined)
//            {
//                Logger.LogInfo("PlayerOnDamagePatch.DamageReplicated() - Ignoring undefined damage ");

//                return;
//            }

//            if (ActiveHealthController == null)
//            {
//                Logger.LogInfo($"PlayerOnDamagePatch.DamageReplicated() - Attempting to Apply Damage a person with no Health Controller");
//                return;
//            }

//            if (!isAlive)
//            {
//                //Logger.LogInfo($"PlayerOnDamagePatch.DamageReplicated() - Attempting to Apply Damage to a Dead Guy");
//                return;
//            }

//            if (autoKillThisDude)
//            {
//                Kill(ActiveHealthController, damageType);
//                return;
//            }

//            //if (ClientHandledDamages.ContainsKey(timeStamp))
//            //    return;
//            //ClientHandledDamages.Add(timeStamp, damageInfo);

//            float currentBodyPartHealth = HealthControllerHelpers.GetBodyPartHealth(ActiveHealthController, bodyPart).Current;

//            //Logger.LogInfo($"ClientApplyDamageInfo::Damage = {damage}");
//            //Logger.LogInfo($"ClientApplyDamageInfo::{bodyPart} current health [before] = {currentBodyPartHealth}");

//            try
//            {
//                if (damage > 0f)
//                {
//                    HealthControllerHelpers.ChangeHealth(ActiveHealthController, bodyPart, -damage, dmI);

//                    //if (Singleton<PlayerGameAction>.Instantiated)
//                    //{
//                    //    //Singleton<PlayerGameAction>.Instance.BeingHitAction(dmI, player);
//                    //}
//                    //if (Singleton<GClass558>.Instantiated)
//                    //{
//                    //    Singleton<GClass558>.Instance.BeingHitAction(damageInfo, this);
//                    //}
//                    //ActiveHealthController.TryApplySideEffects(dmI, bodyPart, out var sideEffectComponent);
//                    if (ActiveHealthController is PlayerHealthController)
//                    {
//                        //Logger.LogInfo("Attempting to Kill!");
//                        ((PlayerHealthController)ActiveHealthController).TryApplySideEffects(dmI, bodyPart, out _);
//                    }
//                }
//            }
//            catch
//            {
//            }

//            ////// get the health again
//            currentBodyPartHealth = HealthControllerHelpers.GetBodyPartHealth(ActiveHealthController, bodyPart).Current;
//            //Logger.LogInfo($"ClientApplyDamageInfo::{bodyPart} current health [after] = {currentBodyPartHealth}");

//            if (currentBodyPartHealth <= 0)
//            {
//                if (!damageType.IsBleeding() && (bodyPart == EBodyPart.Head || bodyPart == EBodyPart.Chest))
//                {
//                    //UnityEngine.Debug.LogError($"ClientApplyDamageInfo::No BodyPart Health on Head/Chest, killing");

//                    Kill(ActiveHealthController, damageType);
//                }
//            }

//            var currentOVRHealth = HealthControllerHelpers.GetBodyPartHealth(ActiveHealthController, EBodyPart.Common).Current;
//            if (currentOVRHealth <= 0)
//            {
//                //UnityEngine.Debug.LogError($"ClientApplyDamageInfo::Common Health, killing");

//                Kill(ActiveHealthController, damageType);
//            }

//            isAlive = ReflectionHelpers.GetFieldOrPropertyFromInstance<bool>(ActiveHealthController, "IsAlive", false);
//            if (!isAlive)
//                return;

//            ActiveHealthController.DoWoundRelapse(damage, bodyPart);
//        }

//        private static void Kill(AbstractActiveHealthController activeHealthController, EDamageType damageType)
//        {
//            if (activeHealthController == null)
//                return;

//            if (activeHealthController is PlayerHealthController)
//            {
//                //Logger.LogInfo("Attempting to Kill!");
//                ((PlayerHealthController)activeHealthController).Kill(damageType);
//            }

//            if (activeHealthController.Player == LocalGame.LocalGamePatches.MyPlayer && Matchmaker.MatchmakerAcceptPatches.IsServer)
//            {
//                Dictionary<string, object> dict = new Dictionary<string, object>();
//                dict.Add("m", "HostDied");
//                ServerCommunication.PostLocalPlayerData(activeHealthController.Player, dict);
//            }
//            //activeHealthController.Kill(damageType);
//            //ReflectionHelpers.GetMethodForType(activeHealthController.GetType(), "Kill").Invoke(activeHealthController, new object[] { damageType });
//        }

//        private static void BeingHitAction(object damageInfo, EFT.Player player)
//        {
//            var singletonBeingHit = PatchConstants.GetSingletonInstance(BeingHitType);
//            if (singletonBeingHit == null)
//                return;

//            ReflectionHelpers.GetMethodForType(singletonBeingHit.GetType(), "BeingHitAction")
//                .Invoke(singletonBeingHit, new object[]
//                {
//                    damageInfo
//                    , player
//                });

//        }

//        private static void TryApplySideEffects(object activeHealthController, EDamageType damageType, EBodyPart bodyPart, out SideEffectComponent sideEffectComponent)
//        {
//            sideEffectComponent = null;
//            if (activeHealthController == null)
//                return;

//            ReflectionHelpers.GetMethodForType(activeHealthController.GetType(), "TryApplySideEffects")
//                .Invoke(activeHealthController, new object[]
//                {
//                    damageType
//                    , bodyPart
//                    , sideEffectComponent
//                });

//        }
//    }
//}

#endregion
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;

namespace SIT.Core.Coop.Player
{
    public class Player_ApplyDamageInfo_Patch : ModuleReplicationPatch
    {
        private static List<long> ProcessedCalls = new();
        public static Dictionary<string, bool> CallLocally = new();
        public override Type InstanceType => typeof(EFT.Player);
        public override string MethodName => "ApplyDamageInfo";
        //public override bool DisablePatch => true;

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
            DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null
            )
        {
            var player = __instance;

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
                    //playerDict.Add("d.p.profile", damageInfo.Player.Profile.SITToJson());
                    //playerDict.Add("d.p.pos.x", damageInfo.Player.Transform.position.x.ToString());
                    //playerDict.Add("d.p.pos.y", damageInfo.Player.Transform.position.y.ToString());
                    //playerDict.Add("d.p.pos.z", damageInfo.Player.Transform.position.z.ToString());
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            damageInfo.Player = null;
            Dictionary<string, string> weaponDict = new Dictionary<string, string>();
            try
            {
                if (damageInfo.Weapon != null)
                {
                    foreach (var wProp in damageInfo.Weapon.GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(x => x.CanWrite && x.CanRead)
                        )
                    {
                        var pw = JsonConvert.SerializeObject(wProp.GetValue(damageInfo.Weapon), PatchConstants.GetJsonSerializerSettingsWithoutBSG());
                        weaponDict.Add(wProp.Name, pw);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
            if (damageInfo.Weapon != null)
            {
                packet.Add("d.w.tpl", damageInfo.Weapon.TemplateId);
                packet.Add("d.w.id", damageInfo.Weapon.Id);
            }
            damageInfo.Weapon = null;

            packet.Add("t", DateTime.Now.Ticks);
            packet.Add("d", SerializeObject(damageInfo));
            packet.Add("d.p", playerDict);
            packet.Add("d.w", weaponDict);
            packet.Add("bpt", bodyPartType.ToString());
            packet.Add("ab", absorbed.ToString());
            packet.Add("m", "ApplyDamageInfo");
            ServerCommunication.PostLocalPlayerData(player, packet);
        }



        public override void Replicated(EFT.Player player, Dictionary<string, object> dict)
        {
            if (player == null)
                return;

            if (dict == null)
                return;

            var timestamp = long.Parse(dict["t"].ToString());
            if (!ProcessedCalls.Contains(timestamp))
                ProcessedCalls.Add(timestamp);
            else
            {
                ProcessedCalls.RemoveAll(x => x <= DateTime.Now.AddHours(-1).Ticks);
                return;
            }

            try
            {
                //DamageInfo damageInfo = new DamageInfo();
                var damageInfo = JObject.Parse(dict["d"].ToString()).ToObject<DamageInfo>();
                Enum.TryParse<EBodyPart>(dict["bpt"].ToString(), out var bodyPartType);
                var absorbed = float.Parse(dict["ab"].ToString());

                EFT.Player aggressorPlayer = null;
                if (dict.ContainsKey("d.p") && damageInfo.Player == null)
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
                            aggressorPlayer = Singleton<GameWorld>.Instance.RegisteredPlayers.FirstOrDefault(x => x.Profile.AccountId == accountId);
                            if (aggressorPlayer != null)
                                damageInfo.Player = aggressorPlayer;
                        }
                    }
                }

                if (dict.ContainsKey("d.w") && damageInfo.Weapon == null)
                {
                    Dictionary<string, string> weaponDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(dict["d.w"].ToString());
                }

                if(dict.ContainsKey("d.w.tpl"))
                {
                    Logger.LogDebug("Apply Damage: Found d.w.tpl");
                    if (aggressorPlayer != null)
                    {
                        if (aggressorPlayer.Inventory.GetAllItemByTemplate(dict["d.w.tpl"].ToString()).Any())
                        {
                            Logger.LogDebug("Apply Damage: Found Template in Player Inventory");
                            var w = aggressorPlayer.Inventory.GetAllItemByTemplate(dict["d.w.tpl"].ToString()).First() as Weapon;
                            if (w != null)
                            {
                                Logger.LogDebug("Apply Damage: Found Weapon in Player Inventory");
                                damageInfo.Weapon = w;
                            }
                        }
                    }
                }


                CallLocally.Add(player.Profile.AccountId, true);
                player.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, EHeadSegment.Eyes);
                //damageInfo.Player = null;
            }
            catch (Exception e)
            {
                Logger.LogInfo(e);
            }
        }
    }
}

