using EFT;
using SIT.Tarkov.Core;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SIT.Core.Other.AI
{
    /**
     * This is an adaptation of "Props" amazing Bush ESP patch! All credit goes to them!
     * https://hub.sp-tarkov.com/files/file/903-no-bush-esp/#overview
     */
    internal class AIBushPatch : ModulePatch
    {
        public static List<string> exclusionList = new List<string> { "filbert", "fibert", "tree", "pine", "plant", "aicollider", "birch", "collider", "timber", "spruce" };

        protected override MethodBase GetTargetMethod()
        {
            try
            {
                return typeof(BotGroupClass).GetMethod("CalcGoalForBot");
            }
            catch
            {
                ModulePatch.Logger.LogInfo("NoBushESP: Failed to get target method.. target dead or unspawned.");
            }
            return null;
        }

        [PatchPostfix]
        public static void PatchPostfix(BotOwner bot)
        {
            try
            {
                object value = bot.Memory.GetType().GetProperty("GoalEnemy").GetValue(bot.Memory);
                if (value == null)
                {
                    return;
                }
                IAIDetails iAIDetails = (IAIDetails)value.GetType().GetProperty("Person").GetValue(value);
                if (!iAIDetails.GetPlayer.IsYourPlayer)
                {
                    return;
                }
                LayerMask highPolyWithTerrainMask = LayerMaskClass.HighPolyWithTerrainMask;
                float maxDistance = Vector3.Distance(bot.Position, iAIDetails.GetPlayer.Position);
                if (!Physics.SphereCast(bot.Position, 1, iAIDetails.GetPlayer.Position, out var hitInfo, maxDistance, highPolyWithTerrainMask))
                {
                    return;
                }
                foreach (string exclusion in exclusionList)
                {
                    if ((hitInfo.collider.transform.parent?.gameObject?.name.ToLower().Contains(exclusion)).Value)
                    {
                        ModulePatch.Logger.LogDebug("NoBushESP: Blocking Excluded Object Name: " + hitInfo.collider.transform.parent?.gameObject?.name);
                        bot.Memory.GetType().GetProperty("GoalEnemy").SetValue(bot.Memory, null);
                        ModulePatch.Logger.LogDebug("NoBushESP: Blocking GoalEnemy for: " + bot.Profile.Info.Settings.Role);
                    }
                }
            }
            catch
            {
                ModulePatch.Logger.LogDebug("NoBushESP: Cannot Assign Brain Because Enemy is Dead or Unspawned");
            }
        }
    }
}
