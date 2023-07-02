using DrakiaXYZ.BigBrain.Patches;

namespace SIT.Core.Other.AI.DrakiaXYZ.BigBrain
{
    /// <summary>
    /// Integrated DrakiaXYZ BigBrain Patch. Found here. https://github.com/DrakiaXYZ/SPT-BigBrain
    /// Full Credit of this Module goes to DrakiaXYZ https://github.com/DrakiaXYZ
    /// </summary>
    internal class BigBrainPatch
    {
        public BigBrainPatch()
        {

            new BotBaseBrainActivatePatch().Enable();
            new BotBrainCreateLogicNodePatch().Enable();

            new BotBaseBrainUpdatePatch().Enable();
            new BotAgentUpdatePatch().Enable();

            new BotBaseBrainActivateLayerPatch().Enable();
            new BotBaseBrainAddLayerPatch().Enable();

        }
    }
}
