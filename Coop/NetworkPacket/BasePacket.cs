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
        public string TimeSerializedBetter
        {
            get
            {
                return DateTime.Now.Ticks.ToString("G");
            }
        }

        [JsonProperty(PropertyName = "m")]
        public virtual string Method { get; set; } = null;

        public BasePacket()
        {
            ServerId = CoopGameComponent.GetServerId();
        }

    }
}
