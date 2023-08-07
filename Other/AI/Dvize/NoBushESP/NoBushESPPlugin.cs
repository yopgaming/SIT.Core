using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Other.AI.Dvize.NoBushESP
{
    using System;
    using System.Diagnostics;
    using BepInEx;

    namespace SIT.Core.Other.AI.Dvize.NoBushESP
    {

        [BepInPlugin("com.dvize.BushNoESP", "dvize.BushNoESP", "1.6.0")]
        public class NoBushESPPlugin : BaseUnityPlugin
        {
            private void Awake()
            {
            }

            public void Start() => new BushPatch().Enable();


        }


    }

}
