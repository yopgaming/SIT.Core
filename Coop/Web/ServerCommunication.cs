using Newtonsoft.Json;
using SIT.Core.Coop;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;

namespace SIT.Coop.Core.Web
{
    public static class ServerCommunication
    {

        public static void PostLocalPlayerData(
            EFT.Player player
            , Dictionary<string, object> data
            , bool useWebSocket = false
            )
        {
            PostLocalPlayerData(player, data, useWebSocket, out _, out _);
        }

        /// <summary>
        /// Posts the data to the Udp Socket and returns the changed Dictionary for any extra use
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <param name="useWebSocket">Use the Web Socket (faster than HTTP)</param>
        /// <returns></returns>
        public static void PostLocalPlayerData(
            EFT.Player player
            , Dictionary<string, object> data
            , bool useWebSocket
            , out string returnedData
            , out Dictionary<string, object> generatedData)
        {
            returnedData = string.Empty;

            if (!data.ContainsKey("t"))
            {
                data.Add("t", DateTime.Now.Ticks);
            }
            if (!data.ContainsKey("accountId"))
            {
                var profile = player.Profile; //  PatchConstants.GetPlayerProfile(player);
                data.Add("accountId", profile.AccountId); // PatchConstants.GetPlayerProfileAccountId(profile));
            }
            if (!data.ContainsKey("serverId"))
            {
                data.Add("serverId", CoopGameComponent.GetServerId());
            }
            if (!data.ContainsKey("profileId"))
            {
                data.Add("profileId", player.ProfileId); // PatchConstants.GetPlayerProfileAccountId(profile));
            }

            //_ = Request.Instance.PostJsonAsync("/coop/server/update", JsonConvert.SerializeObject(data));

            if (useWebSocket)
            {
                Request.Instance.PostDownWebSocketImmediately(data);
            }
            else
                Request.Instance.SendDataToPool("/coop/server/update", data);
            generatedData = data;
        }
    }
}
