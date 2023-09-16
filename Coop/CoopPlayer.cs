using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using EFT.Interactive;
using EFT.InventoryLogic;
using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Player;
using SIT.Coop.Core.Web;
using SIT.Core.Coop.Player;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop
{
    internal class CoopPlayer : LocalPlayer
    {
        ManualLogSource BepInLogger { get; set; }

        public static async Task<LocalPlayer>
            Create(int playerId
            , Vector3 position
            , Quaternion rotation
            , string layerName
            , string prefix
            , EPointOfView pointOfView
            , Profile profile
            , bool aiControl
            , EUpdateQueue updateQueue
            , EUpdateMode armsUpdateMode
            , EUpdateMode bodyUpdateMode
            , CharacterControllerSpawner.Mode characterControllerMode
            , Func<float> getSensitivity, Func<float> getAimingSensitivity
            , IFilterCustomization filter
            , QuestControllerClass questController = null
            , bool isYourPlayer = false
            , bool isClientDrone = false)
        {
            CoopPlayer localPlayer = EFT.Player.Create<CoopPlayer>(ResourceBundleConstants.PLAYER_BUNDLE_NAME, playerId, position, updateQueue, armsUpdateMode, bodyUpdateMode, characterControllerMode, getSensitivity, getAimingSensitivity, prefix, aiControl);
            localPlayer.IsYourPlayer = isYourPlayer;

            InventoryController inventoryController = isYourPlayer && !isClientDrone 
                ? new CoopInventoryController(localPlayer, profile, true) 
                : new CoopInventoryControllerForClientDrone(localPlayer, profile, true);

            if (questController == null && isYourPlayer)
            {
                questController = new QuestController(profile, inventoryController, PatchConstants.BackEndSession, fromServer: true);
                questController.Run();
            }
            //await localPlayer.Init(
            //    rotation
            //    , layerName
            //    , pointOfView
            //    , profile
            //    , inventoryController
            //    , new CoopHealthController(profile.Health, localPlayer, inventoryController, profile.Skills, aiControl)
            //    , isYourPlayer ? new StatisticsManagerForPlayer1() : new NullStatisticsManager()
            //    , questController
            //    , filter
            //    , profile.ProfileId.StartsWith("pmc") ? EVoipState.Available : EVoipState.NotAvailable
            //    , aiControl
            //    , async: false);
            await localPlayer.Init(rotation, layerName, pointOfView, profile, inventoryController, new PlayerHealthController(profile.Health, localPlayer, inventoryController, profile.Skills, aiControl), new NullStatisticsManager(), questController, filter, EVoipState.NotAvailable, aiControl, async: false);
            //foreach (MagazineClass item in localPlayer.Inventory.NonQuestItems.OfType<MagazineClass>())
            //{
            //    localPlayer.GClass2656_0.StrictCheckMagazine(item, status: true, localPlayer.Profile.MagDrillsMastering, notify: false, useOperation: false);
            //}
            //localPlayer._handsController = (EFT.Player.EmptyHandsController)EFT.Player.EmptyHandsController.smethod_5<EFT.Player.EmptyHandsController>(localPlayer);
            localPlayer._handsController = EmptyHandsController.smethod_5<EmptyHandsController>(localPlayer);
            localPlayer._handsController.Spawn(1f, delegate
            {
            });
            localPlayer.AIData = new AiDataClass(null, localPlayer);
            localPlayer.AggressorFound = false;
            localPlayer._animators[0].enabled = true;
            localPlayer.BepInLogger = BepInEx.Logging.Logger.CreateLogSource("CoopPlayer");

            // If this is a Client Drone add Player Replicated Component
            if (isClientDrone)
            {
                var prc = localPlayer.GetOrAddComponent<PlayerReplicatedComponent>();
                prc.IsClientDrone = true;
            }

            return localPlayer;
        }

        /// <summary>
        /// A way to block the same Damage Info being run multiple times on this Character
        /// TODO: Fix this at source. Something is replicating the same Damage multiple times!
        /// </summary>
        private HashSet<DamageInfo> PreviousDamageInfos { get; } = new();
        private HashSet<string> PreviousSentDamageInfoPackets { get; } = new();
        private HashSet<string> PreviousReceivedDamageInfoPackets { get; } = new();

        public override void ApplyDamageInfo(DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null)
        {
            // Quick check?
            if (PreviousDamageInfos.Any(x =>
                x.Damage == damageInfo.Damage
                && x.SourceId == damageInfo.SourceId
                && x.Weapon != null && damageInfo.Weapon != null && x.Weapon.Id == damageInfo.Weapon.Id
                && x.Player != null && damageInfo.Player != null && x.Player == damageInfo.Player
                ))
                return;

            PreviousDamageInfos.Add(damageInfo);

            BepInLogger.LogInfo($"{nameof(ApplyDamageInfo)}:{this.ProfileId}:{DateTime.Now.ToString("T")}");
            //base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, headSegment);

            if (CoopGameComponent.TryGetCoopGameComponent(out var coopGameComponent))
            {
                // If we are not using the Client Side Damage, then only run this on the server
                if(MatchmakerAcceptPatches.IsServer && !coopGameComponent.SITConfig.useClientSideDamageModel)               
                    SendDamageToAllClients(damageInfo, bodyPartType, absorbed, headSegment);
                else
                    SendDamageToAllClients(damageInfo, bodyPartType, absorbed, headSegment);
            }
        }

        private void SendDamageToAllClients(DamageInfo damageInfo, EBodyPart bodyPartType, float absorbed, EHeadSegment? headSegment = null)
        {
            //await Task.Run(async () =>
            //{
                Dictionary<string, object> packet = new();
                var bodyPartColliderType = ((BodyPartCollider)damageInfo.HittedBallisticCollider).BodyPartColliderType;
                damageInfo.HitCollider = null;
                damageInfo.HittedBallisticCollider = null;
                Dictionary<string, string> playerDict = new();
                if (damageInfo.Player != null)
                {
                    playerDict.Add("d.p.aid", damageInfo.Player.iPlayer.Profile.AccountId);
                    playerDict.Add("d.p.id", damageInfo.Player.iPlayer.ProfileId);
                }

                damageInfo.Player = null;
                Dictionary<string, string> weaponDict = new();

                if (damageInfo.Weapon != null)
                {
                    packet.Add("d.w.tpl", damageInfo.Weapon.TemplateId);
                    packet.Add("d.w.id", damageInfo.Weapon.Id);
                }
                damageInfo.Weapon = null;

                packet.Add("d", damageInfo.SITToJson());
                //PatchConstants.Logger.LogDebug(packet["d"]);

                packet.Add("d.p", playerDict);
                packet.Add("d.w", weaponDict);
                packet.Add("bpt", bodyPartType.ToString());
                packet.Add("bpct", bodyPartColliderType.ToString());
                packet.Add("ab", absorbed.ToString());
                packet.Add("hs", headSegment.ToString());
                packet.Add("m", "ApplyDamageInfo");

                // -----------------------------------------------------------
                // An attempt to stop the same packet being sent multiple times
                if (PreviousSentDamageInfoPackets.Contains(packet.ToJson()))
                    return;

                PreviousSentDamageInfoPackets.Add(packet.ToJson());
                // -----------------------------------------------------------

                AkiBackendCommunicationCoop.PostLocalPlayerData(this, packet, true);
            //});
        }

        public void ReceiveDamageFromServer(Dictionary<string, object> dict)
        {
            if(PreviousReceivedDamageInfoPackets.Contains(dict.ToJson())) 
                return;

            PreviousReceivedDamageInfoPackets.Add(dict.ToJson());

            BepInLogger.LogDebug("ReceiveDamageFromServer");
            BepInLogger.LogDebug(dict.ToJson());

            Enum.TryParse<EBodyPart>(dict["bpt"].ToString(), out var bodyPartType);
            Enum.TryParse<EHeadSegment>(dict["hs"].ToString(), out var headSegment);
            var absorbed = float.Parse(dict["ab"].ToString());

            var damageInfo = Player_ApplyShot_Patch.BuildDamageInfoFromPacket(dict);
            damageInfo.HitCollider = Player_ApplyShot_Patch.GetCollider(this, damageInfo.BodyPartColliderType);

            base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, headSegment);
            //base.ShotReactions(damageInfo, bodyPartType);

        }

        protected override void OnSkillLevelChanged(AbstractSkill skill)
        {
            //base.OnSkillLevelChanged(skill);
        }

        protected override void OnWeaponMastered(MasterSkill masterSkill)
        {
            //base.OnWeaponMastered(masterSkill);
        }

        public override void Heal(EBodyPart bodyPart, float value)
        {
            base.Heal(bodyPart, value);
        }

        public override PlayerHitInfo ApplyShot(DamageInfo damageInfo, EBodyPart bodyPartType, ShotId shotId)
        {
            return base.ApplyShot(damageInfo, bodyPartType, shotId);
        }

        public void ReceiveApplyShotFromServer(Dictionary<string, object> dict)
        {
            Logger.LogDebug("ReceiveApplyShotFromServer");
            Enum.TryParse<EBodyPart>(dict["bpt"].ToString(), out var bodyPartType);
            Enum.TryParse<EHeadSegment>(dict["hs"].ToString(), out var headSegment);
            var absorbed = float.Parse(dict["ab"].ToString());

            var damageInfo = Player_ApplyShot_Patch.BuildDamageInfoFromPacket(dict);
            damageInfo.HitCollider = Player_ApplyShot_Patch.GetCollider(this, damageInfo.BodyPartColliderType);

            var shotId = new ShotId();
            if (dict.ContainsKey("ammoid") && dict["ammoid"] != null)
            {
                shotId = new ShotId(dict["ammoid"].ToString(), 1);
            }

            base.ApplyShot(damageInfo, bodyPartType, shotId);
        }

        public override Corpse CreateCorpse()
        {
            return base.CreateCorpse();
        }

        public override void OnItemAddedOrRemoved(Item item, ItemAddress location, bool added)
        {
            base.OnItemAddedOrRemoved(item, location, added);
        }
    }
}
