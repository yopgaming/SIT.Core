using EFT;
using EFT.InventoryLogic;

namespace SIT.Core.Coop
{
    internal class CoopHealthController : PlayerHealthController
    {
        public CoopHealthController(Profile.Health0 healthInfo, EFT.Player player, InventoryController inventoryController, Skills skillManager, bool aiHealth)
            : base(healthInfo, player, inventoryController, skillManager, aiHealth)
        {
        }

        public override bool ApplyItem(Item item, EBodyPart bodyPart, float? amount = null)
        {
            return base.ApplyItem(item, bodyPart, amount);
        }
    }
}
