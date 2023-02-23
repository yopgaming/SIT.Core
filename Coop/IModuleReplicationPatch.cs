using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop
{
    public interface IModuleReplicationPatch
    {
        public abstract Type InstanceType { get; }
        public abstract string MethodName { get; }
        public bool DisablePatch { get; }
    }
}
