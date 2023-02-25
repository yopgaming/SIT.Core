using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Matchmaker.MatchmakerAccept.Grouping
{
    public class SendInvitePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return SIT.Tarkov.Core.PatchConstants.GroupingType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(x => x.Name == "SendInvite");
        }

        [PatchPrefix]
        public static void PatchPrefix(ref string playerId)
        {
            //Logger.LogInfo("SendInvitePatch.PatchPrefix");
        }

        [PatchPostfix]
        public static void PatchPostfix(ref string playerId)
        {
            //Logger.LogInfo("SendInvitePatch.PatchPostfix");
            MatchmakerAcceptPatches.MatchingType = EMatchmakerType.GroupLeader;

            if (MatchmakerAcceptPatches.HostExpectedNumberOfPlayers == 0)
                MatchmakerAcceptPatches.HostExpectedNumberOfPlayers = 1;

            MatchmakerAcceptPatches.HostExpectedNumberOfPlayers++;

            MatchmakerAcceptPatches.SetGroupId(PatchConstants.GetPHPSESSID());

            //_ = ServerCommunication.SendDataDownWebSocket("Start=" + PatchConstants.GetPHPSESSID());

        }
    }
}
