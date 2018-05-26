using ExtendedAPI.Types;
using Pipliz;
using Shared;

namespace SimpleWater.Buckets
{
    [AutoLoadType]
    public class WaterBucket : BaseType
    {
        public WaterBucket() { key = "WaterBucket"; }

        public override void OnRightClickWith(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player)
                return;

            Vector3Int position = boxedData.item1.VoxelBuild;

            ushort oldBucket = ItemTypes.IndexLookup.GetIndex("WaterBucket");
            ushort newBucket = ItemTypes.IndexLookup.GetIndex("EmptyWaterBucket");

            if(World.TryGetTypeAt(position, out ushort actualType) && actualType == SpreadFluids.airIndex)
            {
                ServerManager.TryChangeBlock(position, SpreadFluids.waterIndex, player);

                Inventory inv = Inventory.GetInventory(player);

                inv.TryRemove(oldBucket);
                if(!inv.TryAdd(newBucket))
                    Stockpile.GetStockPile(player).Add(newBucket, 1);
            }
        }
    }

}
