using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using ComponentAce.Compression.Libs.zlib;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        public Request()
        {
            if (m_Instance == null)
                return;

            //if(string.IsNullOrEmpty(Session))
            //    Session = PatchConstants.GetPHPSESSID();
            if (string.IsNullOrEmpty(RemoteEndPoint))
                RemoteEndPoint = PatchConstants.GetBackendUrl();
            GetHeaders();

            m_Instance = this;
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
        private Stream Send(string url, string method = "GET", string data = null, bool compress = true, int timeout = 300)
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
            foreach(var item in GetHeaders())
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
                    Debug.LogError(e);
                }
            }

            // get response stream
            try
            {
                WebResponse response = request.GetResponse();
                return response.GetResponseStream();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                //fullUri = null;
                //request = null;
                //uri = null;
            }
            return null;
        }

        public byte[] GetData(string url, bool hasHost = false)
        {
            var ms = new MemoryStream();
            var dataStream = Send(url, "GET");
            if (dataStream != null)
            {
                dataStream.CopyTo(ms);

                return ms.ToArray();
            }
            return null;
        }

        public void PutJson(string url, string data, bool compress = true)
        {
            using (Stream stream = Send(url, "PUT", data, compress)) { }
        }

        public string GetJson(string url, bool compress = true)
        {
            using (Stream stream = Send(url, "GET", null, compress))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    if (stream == null)
                        return "";
                    stream.CopyTo(ms);
                    return SimpleZlib.Decompress(ms.ToArray(), null);
                }
            }
        }

        public string PostJson(string url, string data, bool compress = true)
        {
            using (Stream stream = Send(url, "POST", data, compress))
            {
                data = null;

                using (MemoryStream ms = new MemoryStream())
                {
                    if (stream == null)
                        return "";
                    stream.CopyTo(ms);
                    return SimpleZlib.Decompress(ms.ToArray(), null);
                }
            }
        }

        public async Task<string> PostJsonAsync(string url, string data, bool compress = true)
        {
            return await Task.FromResult(PostJson(url, data, compress));
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
