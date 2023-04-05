using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.Components
{
    internal interface IPlayerPacketHandlerComponent
    {
        public void HandlePacket(Dictionary<string, object> packet);
    }
}
