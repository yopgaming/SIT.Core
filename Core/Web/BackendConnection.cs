using Newtonsoft.Json;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Tarkov.Core.Web
{
    public class BackendConnection
    {
        public string BackendUrl { get; }
        public string Version { get; }

        public string PHPSESSID { get; private set; }

        public BackendConnection(string backendUrl, string version)
        {
            BackendUrl = backendUrl;
            Version = version;
        }

        //static BackendConnection Instance = null;

        private static BackendConnection CreateBackendConnectionFromEnvVars()
        {
            //if (Instance != null)
            //    return Instance;

            string[] args = Environment.GetCommandLineArgs();
            if(args == null)
                return null;

            var beUrl = string.Empty;
            var php = string.Empty;

            // Get backend url
            foreach (string arg in args)
            {
                //PatchConstants.Logger.LogInfo(arg);

                if (arg.Contains("BackendUrl"))
                {
                    string json = arg.Replace("-config=", string.Empty);
                    var item = JsonConvert.DeserializeObject<BackendConnection>(json);
                    beUrl = item.BackendUrl;
                }

                // get token / phpsessid
                if (arg.Contains("-token="))
                {
                    php = arg.Replace("-token=", string.Empty);
                }
            }



            if (!string.IsNullOrEmpty(php) && !string.IsNullOrEmpty(beUrl))
            {
                //Instance = new BackendConnection(beUrl, php);
                return new BackendConnection(beUrl, php);
            }
            //return Instance;


            // return juicy mem leak for now
            return null;
        }

        public static BackendConnection GetBackendConnection()
        {
            //if (Instance != null)
            //    return Instance;

            return CreateBackendConnectionFromEnvVars();
        }
    }
}
