using SIT.Coop.Core.Matchmaker;
using SIT.Core.SP.PlayerPatches;
using SIT.Tarkov.Core;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SIT.Core.Coop.LocalGame
{
    internal class Player_LeavingGame_Patch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return OfflineSaveProfile.GetMethod();
        }

        [PatchPostfix]
        public static void Postfix(string profileId)
        {
            Logger.LogDebug("PlayerLeavingGame.Postfix");

            

            if (CoopGameComponent.TryGetCoopGameComponent(out var component))
            {
                // Notify that I have left the Server
                Request.Instance.PostDownWebSocketImmediately(new System.Collections.Generic.Dictionary<string, object>() {

                    { "m", "PlayerLeft" },
                    { "accountId", component.Players.FirstOrDefault(x=>x.Value.ProfileId == profileId).Value.Profile.AccountId },
                    { "serverId", CoopGameComponent.GetServerId() }

                });

                // If I am the Host/Server, then ensure all the bots have left too
                if (MatchmakerAcceptPatches.IsServer)
                {
                    foreach(var p in component.Players)
                    {
                        Request.Instance.PostDownWebSocketImmediately(new System.Collections.Generic.Dictionary<string, object>() {

                            { "m", "PlayerLeft" },
                            { "accountId", p.Value.Profile.AccountId },
                            { "serverId", CoopGameComponent.GetServerId() }

                        });
                    }
                }

                Logger.LogDebug("PlayerLeavingGame.Postfix.Destroying CoopGameComponent component");
                GameObject.Destroy(component);
            }
        }
    }
}
