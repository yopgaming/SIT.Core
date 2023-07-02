using Newtonsoft.Json;

namespace SIT.Core.Coop.NetworkPacket
{
    public class BasePlayerPacket : BasePacket
    {
        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }
    }
}
