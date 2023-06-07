using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop.NetworkPacket
{
    public class HealPacket : BasePlayerPacket
    {
        public EBodyPart bodyPart { get; set; }
        public float value { get; set; }
        public override string Method { get => "Heal"; }
    }
}
