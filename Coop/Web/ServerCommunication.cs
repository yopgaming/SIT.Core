using Newtonsoft.Json;
using SIT.Core.Coop;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;

namespace SIT.Coop.Core.Web
{
    public static class ServerCommunication
    {
        #region Not Used
        /// <summary>
        /// The User Profile of the Local Player. I usually set this as soon as we load MatchMakerAcceptScreen (probably could be done a lot sooner and globally)
        /// </summary>
        //public static object UserProfile { get; set; }

        //private static List<UdpClient> udpClients = new List<UdpClient>();

        //private static int udpClientsCount = 1;

        //private static int udpServerPort = 7070;

        //private static string backendUrlIp { get; set; }
        //public static bool CommunicationError { get; private set; }

        //private static string backendUrlPort { get; set; }

        //public delegate void OnDataReceivedHandler(byte[] buffer);
        //public static event OnDataReceivedHandler OnDataReceived;

        //public delegate void OnDataStringReceivedHandler(string @string);
        //public static event OnDataStringReceivedHandler OnDataStringReceived;

        //public delegate void OnDataArrayReceivedHandler(string[] array);
        //public static event OnDataArrayReceivedHandler OnDataArrayReceived;

        //public static void CloseAllUdpClients()
        //{
        //	foreach (var udpClient in udpClients)
        //	{
        //		udpClient.Close();
        //	}

        //	udpClients.Clear();
        //}

        //public static UdpClient GetUdpClient(bool reliable = false)
        //{
        //	if (ServerCommunication.CommunicationError || MatchmakerAcceptPatches.IsSinglePlayer)
        //		return null;

        //	if (udpClients.Any())
        //		return udpClients[0];

        //	//if (!reliable && udpClients.Any())
        //	//	return udpClients[0];

        //	//if (reliable && udpClients.Count > 1)
        //	//	return udpClients[1];

        //	// TODO: Get the game server ip properly!
        //	Dictionary<string, string> dataDict = new Dictionary<string, string>();
        //	dataDict.Add("groupId", MatchmakerAcceptPatches.GetGroupId());
        //	if (string.IsNullOrEmpty(backendUrlIp))
        //	{
        //		var backendUrl = PatchConstants.GetBackendUrl();
        //		if (backendUrl.Contains("localhost"))
        //			backendUrlIp = "127.0.0.1";
        //		else
        //			backendUrlIp = backendUrl.Split(':')[1].Replace("//", "");
        //		//backendUrlPort = array[2];
        //		PatchConstants.Logger.LogInfo("Setting ServerCommunicationCoopImplementation backendurlip::" + backendUrlIp);
        //	}
        //	//var returnedIp = new Request().PostJson("/client/match/group/server/getGameServerIp", data: dataDict.ToJson());
        //	//if (!string.IsNullOrEmpty(returnedIp))
        //	//{
        //	//	if (IPAddress.TryParse(returnedIp, out _))
        //	//	{
        //	//		backendUrlIp = returnedIp;
        //	//	}
        //	//}
        //	dataDict.Clear();
        //	var returnedIpPort = new SIT.Tarkov.Core.Request().PostJson("/coop/getCoopIpAndPort", data: dataDict.ToJson());
        //	//if (!string.IsNullOrEmpty(returnedIpPort))
        //	//{
        //	//	var returnedIpPortDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(returnedIpPort);
        //	//	if(returnedIpPortDictionary != null)
        // //             {
        //	//		if(returnedIpPortDictionary.ContainsKey("ip"))
        // //                 {
        //	//			backendUrlIp = returnedIpPortDictionary["ip"];
        // //                 }
        //	//		if (returnedIpPortDictionary.ContainsKey("port") && int.TryParse(returnedIpPortDictionary["port"], out int port))
        // //                 {
        //	//			udpServerPort = port;
        // //                 }
        // //             }
        //	//}

        //	PatchConstants.Logger.LogInfo("GetUdpClient::Game Server IP is " + backendUrlIp);

        //	//UdpClient udpClient = new UdpClient(backendUrlIp, (reliable ? udpServerPort + 1 : udpServerPort));
        //	UdpClient udpClient = new UdpClient(backendUrlIp, udpServerPort);
        //	udpClient.Client.SendBufferSize = 16384;
        //	udpClient.Client.ReceiveBufferSize = 16384;
        //	udpClient.Client.ReceiveTimeout = 0;
        //	udpClient.Client.SendTimeout = 0;
        //	udpClient.BeginReceive(ReceiveUdp, udpClient);
        //	if (MatchmakerAcceptPatches.IsClient)
        //	{
        //		var connectMessage = "Connect=" + SIT.Tarkov.Core.PatchConstants.GetPHPSESSID();
        //		PatchConstants.Logger.LogInfo(connectMessage);
        //		var connectMessageBytes = Encoding.UTF8.GetBytes(connectMessage);
        //		udpClient.Send(connectMessageBytes, connectMessageBytes.Length);
        //		//udpClient.BeginSend(Encoding.UTF8.GetBytes("Connect="), Encoding.UTF8.GetBytes("Connect=").Length, (IAsyncResult ar) => { }, null);
        //	}
        //	if (!udpClients.Contains(udpClient))
        //		udpClients.Add(udpClient);

        //	PatchConstants.Logger.LogInfo("GetUdpClient::Created Udp Client and running");


        //	return udpClient;
        //}

        //public static void ReceiveUdp(IAsyncResult ar)
        //{
        //	var udpClient = ar.AsyncState as UdpClient;
        //	IPEndPoint endPoint = null;
        //	var data = udpClient.EndReceive(ar, ref endPoint);
        //	if (OnDataReceived != null)
        //	{
        //		OnDataReceived(data);
        //	}
        //	string @string = UTF8Encoding.UTF8.GetString(data);
        //	if (@string.Length > 0 && @string[0] == '{' && @string[@string.Length - 1] == '}')
        //          {
        //		if(OnDataStringReceived != null)
        //              {
        //			OnDataStringReceived(@string);
        //              }
        //          }
        //	if (@string.Length > 0 && @string[0] == '[' && @string[@string.Length - 1] == ']')
        //	{
        //		var stringArray = JsonConvert.DeserializeObject<string[]>(@string);
        //		if (OnDataArrayReceived != null)
        //		{
        //			OnDataArrayReceived(stringArray);
        //		}
        //	}
        //	udpClient.BeginReceive(ReceiveUdp, udpClient);
        //}

        //   public static string SerializeObject(object o)
        //   {
        //       try
        //       {
        //           JsonSerializerSettings settings = new JsonSerializerSettings()
        //           {
        //               NullValueHandling = NullValueHandling.Ignore,
        //               //ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        //               //FloatParseHandling = FloatParseHandling.Decimal,
        //               MissingMemberHandling = MissingMemberHandling.Ignore,
        //               //ObjectCreationHandling = ObjectCreationHandling.Reuse,
        //               ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        //           };
        //           settings.Error = (serializer, err) =>
        //           {
        ////PatchConstants.Logger.LogInfo(err.ErrorContext.Error.Message);
        //PatchConstants.Logger.LogError(err.ErrorContext.Error.Message);
        //               //err.ErrorContext.Handled = true;
        //           };
        //           return JsonConvert.SerializeObject(o, settings);
        //       }
        //       catch (Exception)
        //       {

        //       }
        //       return string.Empty;
        //   }

        #endregion
        public delegate void PostLocalPlayerDataHandler(object player, Dictionary<string, object> data);
        public static event PostLocalPlayerDataHandler OnPostLocalPlayerData;

        public static void PostLocalPlayerData(
            object player
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
            object player
            , Dictionary<string, object> data
            , out string returnedData
            , out Dictionary<string, object> generatedData)
        {
            //if (!data.ContainsKey("groupId"))
            //{
            //	data.Add("groupId", MatchmakerAcceptPatches.GetGroupId());
            //}
            if (!data.ContainsKey("t"))
            {
                data.Add("t", DateTime.Now.Ticks);
            }
            //if (!data.ContainsKey("profileId"))
            //{
            //	data.Add("profileId", player.Profile.Id);
            //}
            if (!data.ContainsKey("accountId"))
            {
                var profile = PatchConstants.GetPlayerProfile(player);
                data.Add("accountId", PatchConstants.GetPlayerProfileAccountId(profile));
            }
            if (!data.ContainsKey("serverId"))
            {
                data.Add("serverId", CoopGameComponent.GetServerId());
            }
            //_ = SendDataDownWebSocket(data, useReliable);
            //returnedData = Request.Instance.PostJson("/coop/server/update", data.SITToJson());
            returnedData = Request.Instance.PostJson("/coop/server/update", JsonConvert.SerializeObject(data));

            var cgc = CoopGameComponent.GetCoopGameComponent();
            if (cgc != null)
            {
                cgc.ReadFromServerLastActionsParseData(returnedData);

            }

            //if (OnPostLocalPlayerData != null)
            //{
            //	OnPostLocalPlayerData(player, data);
            //}
            generatedData = data;
        }
    }
}
