using System;
using System.Collections.Generic;

namespace SIT.Core.Coop
{
    public interface IModuleReplicationWorldPatch
    {
        public Type InstanceType { get; }
        public string MethodName { get; }
        public bool DisablePatch { get; }

        public void Replicated(Dictionary<string, object> packet);

    }
}
