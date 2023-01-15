using EFT;
using SIT.Tarkov.Core;
using System.Reflection;

namespace SIT.Core
{
    internal class IsBossOrFollowerFixPatch : ModulePatch
    {
        public IsBossOrFollowerFixPatch()
        {

            //public static bool IsBossOrFollower(this WildSpawnType role)
            //{
            //    return role.IsBoss() || role.IsFollower();
            //}
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotSettingsRepoClass).GetMethod("IsBossOrFollower", BindingFlags.Static | BindingFlags.Public);
        }
    }
}