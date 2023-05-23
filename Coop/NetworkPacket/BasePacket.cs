using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.NetworkPacket
{
    public abstract class BasePacket
    {
        [JsonProperty(PropertyName = "serverId")]
        public string ServerId { get; set; }

        [JsonProperty(PropertyName = "t")]
        public float Time { get; set; }

        [JsonProperty(PropertyName = "m")]
        public virtual string Method { get; set; }
    }
}
