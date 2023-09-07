using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIT.Core.Coop
{
    internal class CoopInventoryControllerForClientDrone 
        : InventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public CoopInventoryControllerForClientDrone(EFT.Player player, Profile profile, bool examined) 
            : base(profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryController));
        }

        protected override void Execute(SearchContentOperation operation, Callback callback)
        {
            base.Execute(operation, callback);
        }

        public override Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            return base.UnloadMagazine(magazine);
        }

        public override void ThrowItem(Item item, IEnumerable<ItemsCount> destroyedItems, Callback callback = null, bool downDirection = false)
        {
            destroyedItems = new List<ItemsCount>();
            base.ThrowItem(item, destroyedItems, callback, downDirection);
        }
    }
}
