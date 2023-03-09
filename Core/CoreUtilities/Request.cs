using ComponentAce.Compression.Libs.zlib;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Tarkov.Core
{
    public class Request : IDisposable
    {
        private string m_Session;

        public string Session
        {
            get
            {
                return m_Session;
            }
            set { m_Session = value; }
        }



        private string m_RemoteEndPoint;

        public string RemoteEndPoint
        {
            get
            {
                if (string.IsNullOrEmpty(m_RemoteEndPoint))
                    m_RemoteEndPoint = PatchConstants.GetBackendUrl();

                return m_RemoteEndPoint;

            }
            set { m_RemoteEndPoint = value; }
        }

        //public bool isUnity;
        private Dictionary<string, string> m_RequestHeaders { get; set; }

        private static Request m_Instance { get; set; }
        public static Request Instance
        {
            get
            {
                if (m_Instance == null || m_Instance.Session == null || m_Instance.RemoteEndPoint == null)
                    m_Instance = new Request();

                return m_Instance;
            }
        }


        private Request()
        {
            if (string.IsNullOrEmpty(RemoteEndPoint))
                RemoteEndPoint = PatchConstants.GetBackendUrl();
            GetHeaders();
            PeriodicallySendPooledData();
        }

        public static Request GetRequestInstance(bool createInstance = false)
        {
            if (createInstance)
            {
                return new Request();
            }

            return Request.Instance;
        }

        ConcurrentQueue<KeyValuePair<string, Dictionary<string, object>>> m_PooledDictionariesToPost = new ConcurrentQueue<KeyValuePair<string, Dictionary<string, object>>>();
        ConcurrentQueue<KeyValuePair<string, string>> m_PooledStringToPost = new ConcurrentQueue<KeyValuePair<string, string>>();

        public void SendDataToPool(string url, Dictionary<string, object> data)
        {
            //PatchConstants.Logger.LogDebug($"SendDataToPool({url}, some data)");
            m_PooledDictionariesToPost.Enqueue(new(url, data));
            //PatchConstants.Logger.LogDebug($"m_PooledDictionariesToPost now has:{m_PooledDictionariesToPost.Count}:entries");
        }

        public void SendDataToPool(string url, string stringData)
        {
            m_PooledStringToPost.Enqueue(new(url, stringData));
        }

        public long PostPing { get; private set; }

        private Task PeriodicallySendPooledDataTask;

        private void PeriodicallySendPooledData()
        {
            //PatchConstants.Logger.LogDebug($"PeriodicallySendPooledData()");

            PeriodicallySendPooledDataTask = Task.Run(async() => 
            {
                //PatchConstants.Logger.LogDebug($"PeriodicallySendPooledData():In Async Task");

                //while (m_Instance != null)
                Stopwatch swPing = new Stopwatch();

                while (true)
                {
                    swPing.Restart();
                    await Task.Delay(33);
                    //PatchConstants.Logger.LogDebug($"m_PooledDictionariesToPost:{m_PooledDictionariesToPost.Count}:entries");
                    if (m_PooledDictionariesToPost.Any())
                    {
                        PatchConstants.Logger.LogDebug($"m_PooledDictionariesToPost:{m_PooledDictionariesToPost.Count}:entries");
                        m_PooledDictionariesToPost.TryDequeue(out var d);
                        var url = d.Key;
                        var json = JsonConvert.SerializeObject(d.Value);
                        await PostJsonAsync(url, json);
                    }

                    if (m_PooledStringToPost.Any())
                    {
                        PatchConstants.Logger.LogDebug($"m_PooledStringToPost:{m_PooledStringToPost.Count}:entries");
                        m_PooledStringToPost.TryDequeue(out var d);
                        var url = d.Key;
                        var json = d.Value;
                        PostJson(url, json);
                    }
                    PostPing = swPing.ElapsedMilliseconds - 33;
                }
            });
        }

        private Dictionary<string, string> GetHeaders()
        {
            //if (string.IsNullOrEmpty(Session) || m_RequestHeaders == null)
            //{
            string[] args = Environment.GetCommandLineArgs();

            foreach (string arg in args)
            {
                //if (arg.Contains("BackendUrl"))
                //{
                //    string json = arg.Replace("-config=", string.Empty);
                //    _host = Json.Deserialize<ServerConfig>(json).BackendUrl;
                //}

                if (arg.Contains("-token="))
                {
                    Session = arg.Replace("-token=", string.Empty);
                    m_RequestHeaders = new Dictionary<string, string>()
                        {
                            { "Cookie", $"PHPSESSID={Session}" },
                            { "SessionId", Session }
                        };
                }
            }
            //}
            return m_RequestHeaders;
        }

        public Request(string session, string remoteEndPoint, bool isUnity = true)
        {
            Session = session;
            RemoteEndPoint = remoteEndPoint;
        }
        /// <summary>
        /// Send request to the server and get Stream of data back
        /// </summary>
        /// <param name="url">String url endpoint example: /start</param>
        /// <param name="method">POST or GET</param>
        /// <param name="data">string json data</param>
        /// <param name="compress">Should use compression gzip?</param>
        /// <returns>Stream or null</returns>
        private MemoryStream Send(string url, string method = "GET", string data = null, bool compress = true, int timeout = 1000)
        {
            // disable SSL encryption
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var fullUri = url;
            if (!Uri.IsWellFormedUriString(fullUri, UriKind.Absolute))
                fullUri = RemoteEndPoint + fullUri;

            //PatchConstants.Logger.LogInfo(fullUri);

            var uri = new Uri(fullUri);
            if (uri.Scheme == "https")
            {
                // disable SSL encryption
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.ServerCertificateValidationCallback = delegate { return true; };
            //var request = WebRequest.CreateHttp(fullUri);

            //if (!string.IsNullOrEmpty(Session))
            //{
            //    request.Headers.Add("Cookie", $"PHPSESSID={Session}");
            //    request.Headers.Add("SessionId", Session);
            //}
            foreach (var item in GetHeaders())
            {
                request.Headers.Add(item.Key, item.Value);
            }

            request.Headers.Add("Accept-Encoding", "deflate");

            request.Method = method;
            request.Timeout = timeout;

            if (method != "GET" && !string.IsNullOrEmpty(data))
            {
                // set request body
                //byte[] bytes = (compress) ? SimpleZlib.CompressToBytes(data, zlibConst.Z_BEST_COMPRESSION) : Encoding.UTF8.GetBytes(data);
                byte[] bytes = (compress) ? SimpleZlib.CompressToBytes(data, zlibConst.Z_BEST_SPEED) : Encoding.UTF8.GetBytes(data);
                data = null;
                request.ContentType = "application/json";
                request.ContentLength = bytes.Length;

                if (compress)
                {
                    request.Headers.Add("content-encoding", "deflate");
                }

                try
                {
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                catch (Exception e)
                {
                    PatchConstants.Logger.LogError(e);
                }
            }

            // get response stream
            //WebResponse response = null;
            try
            {
                var ms = new MemoryStream();
                using (var response = request.GetResponse())
                {
                    using (var responseStream = response.GetResponseStream())
                        responseStream.CopyTo(ms);
                }
                return ms;
            }
            catch (Exception e)
            {
                //Debug.LogError(e);
                PatchConstants.Logger.LogError(e);
            }
            finally
            {
                fullUri = null;
                request = null;
                uri = null;
                //response.Close();
                //response.Dispose();
                //response = null;
            }
            return null;
        }

        public byte[] GetData(string url, bool hasHost = false)
        {
            using (var dataStream = Send(url, "GET"))
                return dataStream.ToArray();
        }

        public void PutJson(string url, string data, bool compress = true)
        {
            using (Stream stream = Send(url, "PUT", data, compress)) { }
        }

        public string GetJson(string url, bool compress = true)
        {
            using (MemoryStream stream = Send(url, "GET", null, compress))
            {
                if (stream == null)
                    return "";
                var bytes = stream.ToArray();
                var result = SimpleZlib.Decompress(bytes, null);
                bytes = null;
                countOfCalls++;
                if (countOfCalls >= 50)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    countOfCalls = 0;
                }
                return result;
            }
        }

        private int countOfCalls = 0;

        public string PostJson(string url, string data, bool compress = true)
        {
            using (MemoryStream stream = Send(url, "POST", data, compress))
            {
                if (stream == null)
                    return "";
                var bytes = stream.ToArray();
                var result = SimpleZlib.Decompress(bytes, null);
                bytes = null;
                countOfCalls++;
                if(countOfCalls >= 50)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    countOfCalls = 0;
                }
                return result;
            }
        }

        public async Task<string> PostJsonAsync(string url, string data)
        {
            return await Task.FromResult(PostJson(url, data, true));
        }


        /// <summary>
        /// Retrieves data asyncronously and parses to the desired type
        /// </summary>
        /// <typeparam name="T">Desired type to Deserialize to</typeparam>
        /// <param name="url">URL to call</param>
        /// <param name="data">data to send</param>
        /// <returns></returns>
        public async Task<T> PostJsonAsync<T>(string url, string data)
        {
            var json = await PostJsonAsync(url, data);
            return await Task.FromResult(JsonConvert.DeserializeObject<T>(json));
        }

        public Texture2D GetImage(string url, bool compress = true)
        {
            using (Stream stream = Send(url, "GET", null, compress))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (stream == null)
                        return null;
                    Texture2D texture = new Texture2D(8, 8);

                    stream.CopyTo(ms);
                    texture.LoadImage(ms.ToArray());
                    return texture;
                }
            }
        }

        public void Dispose()
        {
            //m_RequestHeaders = null;
        }
    }
}
