using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Core.Coop.NetworkPacket
{
    public class BasePlayerPacket : BasePacket
    {
        [JsonProperty(PropertyName = "accountId")]
        public string AccountId { get; set; }
    }
}
