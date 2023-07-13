using DrakiaXYZ.BigBrain.Internal;
using HarmonyLib;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

using AICoreLogicAgentClass = AICoreAgentClass<BotLogicDecision>;
using AICoreNode = GClass103;
using AILogicActionResultStruct = AICoreActionResultStruct<BotLogicDecision>;

namespace DrakiaXYZ.BigBrain.Patches
{
    /**
     * Patch the bot agent update method so we can trigger a Start() method on custom logic actions
     **/
    internal class BotAgentUpdatePatch : ModulePatch
    {
        private static FieldInfo _brainFieldInfo;
        private static FieldInfo _lastResultField;
        private static FieldInfo _logicInstanceDictField;
        private static FieldInfo _lazyGetterField;

        protected override MethodBase GetTargetMethod()
        {
            Type botAgentType = typeof(AICoreLogicAgentClass);

            _brainFieldInfo = AccessTools.Field(botAgentType, "gclass216_0");
            _lastResultField = AccessTools.Field(botAgentType, "gstruct8_0");
            _logicInstanceDictField = AccessTools.Field(botAgentType, "dictionary_0");
            _lazyGetterField = AccessTools.Field(botAgentType, "func_0");

            return AccessTools.Method(botAgentType, "Update");
        }

        [PatchPrefix]
        public static bool PatchPrefix(object __instance)
        {
            try
            {

                // Get values we'll use later
                AbstractBaseBrain brain = _brainFieldInfo.GetValue(__instance) as AbstractBaseBrain;
                Dictionary<BotLogicDecision, AICoreNode> aiCoreNodeDict = _logicInstanceDictField.GetValue(__instance) as Dictionary<BotLogicDecision, AICoreNode>;

                // Update the brain, this is instead of method_10 in the original code
                brain.ManualUpdate();

                // Call the brain update
                AILogicActionResultStruct lastResult = (AILogicActionResultStruct)_lastResultField.GetValue(__instance);
                AILogicActionResultStruct? result = brain.Update(lastResult);
                if (result != null)
                {
                    // If an instance of our action doesn't exist in our dict, add it
                    int action = (int)result.Value.Action;
                    if (!aiCoreNodeDict.TryGetValue((BotLogicDecision)action, out AICoreNode nodeInstance))
                    {
                        Func<BotLogicDecision, AICoreNode> lazyGetter = _lazyGetterField.GetValue(__instance) as Func<BotLogicDecision, AICoreNode>;
                        nodeInstance = lazyGetter((BotLogicDecision)action);

                        if (nodeInstance != null)
                        {
                            aiCoreNodeDict.Add((BotLogicDecision)action, nodeInstance);
                        }
                    }

                    if (nodeInstance != null)
                    {
                        // If we're switching to a new action, call Start() on the new logic
                        if (lastResult.Action != result.Value.Action && nodeInstance is CustomLogicWrapper customLogic)
                        {
                            customLogic.Start();
                        }

                        nodeInstance.Update();
                    }

                    _lastResultField.SetValue(__instance, result);
                }

                return false;

            }
            catch (Exception)
            {
                //Logger.LogError(ex);
                //throw ex;
            }

            // Paulov. If this fails. Just revert.
            return true;
        }
    }
}
