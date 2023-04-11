using System.Collections.Generic;

namespace SIT.Core.Coop.Components
{
    internal interface IPlayerPacketHandlerComponent
    {
        public void HandlePacket(Dictionary<string, object> packet);
    }
}
