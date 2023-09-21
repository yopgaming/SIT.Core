using Newtonsoft.Json;
using System;

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

        private static BackendConnection CreateBackendConnectionFromEnvVars()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args == null)
                return null;

            var beUrl = string.Empty;
            var php = string.Empty;

            // Get backend url
            foreach (string arg in args)
            {
                if (arg.Contains("BackendUrl"))
                {
                    string json = arg.Replace("-config=", string.Empty);
                    var item = JsonConvert.DeserializeObject<BackendConnection>(json);
                    beUrl = item.BackendUrl;
                }
                if (arg.Contains("-token="))
                {
                    php = arg.Replace("-token=", string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(php) && !string.IsNullOrEmpty(beUrl))
            {
                return new BackendConnection(beUrl, php);
            }
            return null;
        }

        public static BackendConnection GetBackendConnection()
        {
            return CreateBackendConnectionFromEnvVars();
        }
    }
}
