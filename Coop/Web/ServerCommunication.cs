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
            )
        {
            PostLocalPlayerData(player, data, out _, out _);
        }

        /// <summary>
        /// Posts the data to the Udp Socket and returns the changed Dictionary for any extra use
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <param name="useReliable"></param>
        /// <returns></returns>
        public static void PostLocalPlayerData(
            EFT.Player player
            , Dictionary<string, object> data
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

            //returnedData = Request.Instance.PostJson("/coop/server/update", data.SITToJson());
            //returnedData = Request.Instance.PostJson("/coop/server/update", JsonConvert.SerializeObject(data));
            _ = Request.Instance.PostJson("/coop/server/update", JsonConvert.SerializeObject(data));

            //var cgc = CoopGameComponent.GetCoopGameComponent();
            //if (cgc != null)
            //{
            //    cgc.ReadFromServerLastActionsParseData(returnedData);

            //}

            generatedData = data;
        }
    }
}
