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
        public string ServerId { get; set; } = CoopGameComponent.GetServerId();

        [JsonProperty(PropertyName = "t")]
        public float Time { get; set; } = DateTime.Now.Ticks;

        [JsonProperty(PropertyName = "m")]
        public virtual string Method { get; set; } = null;

        public BasePacket()
        {
            Time = DateTime.Now.Ticks;
            ServerId = CoopGameComponent.GetServerId();
        }
    }
}
