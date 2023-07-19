using BepInEx.Logging;
using EFT;
using EFT.InventoryLogic;
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
            , bool isYourPlayer = false)
        {
            CoopPlayer localPlayer = EFT.Player.Create<CoopPlayer>(GClass1379.PLAYER_BUNDLE_NAME, playerId, position, updateQueue, armsUpdateMode, bodyUpdateMode, characterControllerMode, getSensitivity, getAimingSensitivity, prefix, aiControl);
            localPlayer.IsYourPlayer = isYourPlayer;
            //SinglePlayerInventoryController inventoryController = new SinglePlayerInventoryController(localPlayer, profile);
            InventoryController inventoryController = isYourPlayer
                ? new SinglePlayerInventoryController(localPlayer, profile)
                : new CoopInventoryController(localPlayer, profile, true);

            if (questController == null && isYourPlayer)
            {
                questController = new QuestController(profile, inventoryController, PatchConstants.BackEndSession, fromServer: true);
                questController.Run();
            }
            await localPlayer.Init(
                rotation
                , layerName
                , pointOfView
                , profile
                , inventoryController
                , new CoopHealthController(profile.Health, localPlayer, inventoryController, profile.Skills, aiControl)
                , isYourPlayer ? new StatisticsManagerForPlayer1() : new NullStatisticsManager()
                , questController
                , filter
                , profile.ProfileId.StartsWith("pmc") ? EVoipState.Available : EVoipState.NotAvailable
                , aiControl
                , async: false);
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
            return localPlayer;
        }

        /// <summary>
        /// A way to block the same Damage Info being run multiple times on this Character
        /// TODO: Fix this at source. Something is replicating the same Damage multiple times!
        /// </summary>
        private List<DamageInfo> PreviousDamageInfos { get; } = new();

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
            base.ApplyDamageInfo(damageInfo, bodyPartType, absorbed, headSegment);
        }

        protected override void OnSkillLevelChanged(AbstractSkill skill)
        {
            base.OnSkillLevelChanged(skill);
            if (!base.IsAI && IsYourPlayer)
            {
                NotificationManagerClass.DisplayNotification(new GClass1990(skill));
            }
        }

        protected override void OnWeaponMastered(MasterSkill masterSkill)
        {
            base.OnWeaponMastered(masterSkill);
            if (!base.IsAI && IsYourPlayer)
            {
                NotificationManagerClass.DisplayMessageNotification(string.Format("MasteringLevelUpMessage".Localized(), masterSkill.MasteringGroup.Id.Localized(), masterSkill.Level.ToString()));
            }
        }

    }
}
