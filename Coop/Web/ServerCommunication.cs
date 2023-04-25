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
            , bool useReliable = false
            )
        {
            PostLocalPlayerData(player, data, useReliable, out _, out _);
        }

        //public static void PostLocalPlayerData(
        //   EFT.Player player
        //   , Dictionary<string, object> data
        //   , Request requestInstance
        //   )
        //{
        //    if (!data.ContainsKey("t"))
        //        data.Add("t", DateTime.Now.Ticks);
        //    if (!data.ContainsKey("accountId"))
        //    {
        //        var profile = player.Profile; //  PatchConstants.GetPlayerProfile(player);
        //        data.Add("accountId", profile.AccountId); // PatchConstants.GetPlayerProfileAccountId(profile));
        //    }
        //    if (!data.ContainsKey("serverId"))
        //        data.Add("serverId", CoopGameComponent.GetServerId());

        //    requestInstance.SendDataToPool("/coop/server/update", data);
        //}


        /// <summary>
        /// Posts the data to the Udp Socket and returns the changed Dictionary for any extra use
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <param name="useReliable">Use Reliable Forceably makes the Request without any pooling or timeout</param>
        /// <returns></returns>
        public static void PostLocalPlayerData(
            EFT.Player player
            , Dictionary<string, object> data
            , bool useReliable
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

            //_ = Request.Instance.PostJsonAsync("/coop/server/update", JsonConvert.SerializeObject(data));

            if (useReliable)
            {
                var req = Request.GetRequestInstance(true);
                _ = req.PostJsonAsync("/coop/server/update", JsonConvert.SerializeObject(data), timeout: 9999).ContinueWith((str) => {
                    req = null;
                });
            }
            else
                Request.Instance.SendDataToPool("/coop/server/update", data);
            generatedData = data;
        }
    }
}
