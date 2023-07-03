using SIT.Coop.Core.Matchmaker;
using SIT.Coop.Core.Player;
using SIT.Core.Core;
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
                AkiBackendCommunication.Instance.PostDownWebSocketImmediately(new System.Collections.Generic.Dictionary<string, object>() {

                    { "m", "PlayerLeft" },
                    { "accountId", component.Players.FirstOrDefault(x=>x.Value.ProfileId == profileId).Value.Profile.AccountId },
                    { "serverId", CoopGameComponent.GetServerId() }

                });

                // If I am the Host/Server, then ensure all the bots have left too
                if (MatchmakerAcceptPatches.IsServer)
                {
                    foreach (var p in component.Players)
                    {
                        AkiBackendCommunication.Instance.PostDownWebSocketImmediately(new System.Collections.Generic.Dictionary<string, object>() {

                            { "m", "PlayerLeft" },
                            { "accountId", p.Value.Profile.AccountId },
                            { "serverId", CoopGameComponent.GetServerId() }

                        });
                    }
                }

                Logger.LogDebug("PlayerLeavingGame.Postfix.Destroying CoopGameComponent component");
                foreach (var p in component.Players)
                {
                    if (p.Value == null)
                        continue;

                    if (p.Value.TryGetComponent<PlayerReplicatedComponent>(out var prc))
                    {
                        GameObject.Destroy(prc);
                    }
                }

                if (component != null)
                {
                    foreach (var prc in GameObject.FindObjectsOfType<PlayerReplicatedComponent>())
                    {
                        GameObject.DestroyImmediate(prc);
                    }
                    GameObject.DestroyImmediate(component);
                }
            }
        }
    }
}
