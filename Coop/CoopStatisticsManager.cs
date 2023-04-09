using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop
{
    internal class CoopStatisticsManager : IStatisticsManager
    {
        private EFT.Player player;

        public TimeSpan CurrentSessionLength
        {
            get
            {
                return default(TimeSpan);
            }
        }

        public event Action OnUniqueLoot;

        public void AddDoorExperience(bool breached)
        {
        }

        public void BeginStatisticsSession()
        {
        }

        public void EndStatisticsSession(ExitStatus exitStatus, float pastTime)
        {
        }

        public void Init(EFT.Player player)
        {
            this.player = player;
        }

        public void OnEnemyDamage(DamageInfo damage, EBodyPart bodyPart, EPlayerSide playerSide, string role, string groupId, float fullHealth, bool isHeavyDamage, float distance, int hour, List<string> targetEquipment, BodyPartEffects enemyEffects, List<string> zoneIds)
        {
        }

        public void OnEnemyKill(DamageInfo damage, EDamageType lethalDamageType, EBodyPart bodyPart, EPlayerSide playerSide, WildSpawnType role, string playerAccountId, string playerProfileId, string playerName, string groupId, int level, int killExp, float distance, int hour, List<string> targetEquipment, BodyPartEffects enemyEffects, List<string> zoneIds)
        {
        }

        public void OnGrabLoot(Item item)
        {
        }

        public void OnInteractWithLootContainer(Item item)
        {
        }

        public void OnShot(Weapon weapon, BulletClass ammo)
        {
        }
    }
}
