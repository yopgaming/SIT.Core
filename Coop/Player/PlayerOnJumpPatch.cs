using SIT.Coop.Core.Web;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SIT.Coop.Core.Player
{
    internal class PlayerOnJumpPatch : ModulePatch
    {

        public static ConcurrentBag<(string, long)> LastJumps = new ConcurrentBag<(string, long)>();

        protected override MethodBase GetTargetMethod()
        {
            var t = typeof(EFT.Player);
            if (t == null)
                Logger.LogInfo($"PlayerOnJumpPatch:Type is NULL");

            var method = ReflectionHelpers.GetMethodForType(t, "Jump");

            Logger.LogInfo($"PlayerOnJumpPatch:{t.Name}:{method.Name}");
            return method;
        }

        [PatchPrefix]
        public static bool PrePatch(
            EFT.Player __instance)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("m", "Jump");
            ServerCommunication.PostLocalPlayerData(__instance, dictionary, out var rD, out var genD);

            return false;// Matchmaker.MatchmakerAcceptPatches.IsSinglePlayer;
        }

        //[PatchPostfix]
        //public static void PatchPostfix(
        //    EFT.Player __instance)
        //{
        //    Dictionary<string, object> dictionary = new Dictionary<string, object>();
        //    dictionary.Add("m", "Jump");
        //    ServerCommunication.PostLocalPlayerData(__instance, dictionary, out var rD, out var genD);


        //    //LastJumps.Add((__instance.Profile.AccountId, DateTime.Now.Ticks));
        //}

        public static void Replicated(EFT.Player player, Dictionary<string, object> packet)
        {
            if (player == null)
                return;

            var timeStamp = long.Parse(packet["t"].ToString());
            if (LastJumps.Any(x => x.Item1 == player.Profile.AccountId && x.Item2 == timeStamp))
                return;

            LastJumps.Add((player.Profile.AccountId, timeStamp));

            //Logger.LogInfo("Jumping BIIIITCH!");

            player.CurrentState.Jump();
        }
    }
}
