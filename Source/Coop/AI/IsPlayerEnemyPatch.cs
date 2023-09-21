using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Core.Coop.AI
{
    internal class IsPlayerEnemyPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(BotGroupClass), "IsPlayerEnemy");
        }

        //[PatchPrefix]
        //public static bool Prefix(
        //    bool __result
        //    )
        //{
        //    __result = true;
        //    return true;
        //}

        [PatchPostfix]
        public static void Postfix(
            bool __result
            //BotGroupClass __instance,
            //IAIDetails player,
            //BotGlobalsMindSettings ___botGlobalsMindSettings_0
            )
        {
            __result = true;

            //var mySide = __instance.Side;
            //var flag = true;
            //WildSpawnType role = player.AIData.BotOwner.Profile.Info.Settings.Role;
            //var botGlobalsMindSettings_0 = ___botGlobalsMindSettings_0;
            //if (botGlobalsMindSettings_0.ENEMY_BOT_TYPES.Contains(role))
            //{
            //    flag = true;
            //}
            //switch (player.Side)
            //{
            //    case EPlayerSide.Usec:
            //        flag = flag || botGlobalsMindSettings_0.DEFAULT_USEC_BEHAVIOUR.HasFlag(EWarnBehaviour.Attack)
            //        || mySide != player.Side;
            //        break;
            //    case EPlayerSide.Bear:
            //        flag = flag || botGlobalsMindSettings_0.DEFAULT_BEAR_BEHAVIOUR.HasFlag(EWarnBehaviour.Attack)
            //        || mySide != player.Side;
            //        break;
            //    case EPlayerSide.Savage:
            //        flag = flag || player.Loyalty.HostileScavs;
            //        if (botGlobalsMindSettings_0.DEFAULT_SAVAGE_BEHAVIOUR.HasFlag(EWarnBehaviour.Attack))
            //        {
            //            flag = player.Loyalty == null || flag || !player.Loyalty.BossNoAttack || botGlobalsMindSettings_0.BOSS_IGNORE_LOYALTY;
            //        }
            //        break;
            //}

            //if(flag)
            //{
            //    var value = new BotSettingsClass(player.GetPlayer, __instance);
            //    __instance.Enemies.Add(player, value);
            //}
        }
    }
}
