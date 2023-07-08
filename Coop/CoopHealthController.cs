using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop
{
    internal class CoopHealthController : PlayerHealthController
    {
        public CoopHealthController(Profile.Health0 healthInfo, EFT.Player player, InventoryController inventoryController, Skills skillManager, bool aiHealth) 
            : base(healthInfo, player, inventoryController, skillManager, aiHealth)
        {
        }
    }
}
