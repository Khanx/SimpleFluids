using ExtendedAPI.Types;
using Pipliz;
using Shared;

namespace SimpleWater.Buckets
{
    [AutoLoadType]
    public class EmptyWaterBucket : BaseType
    {
        public EmptyWaterBucket() { key = "EmptyWaterBucket"; }

        public override void OnRightClickWith(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player)
                return;

            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("water");
            ushort oldBucket = ItemTypes.IndexLookup.GetIndex("EmptyWaterBucket");
            ushort newBucket = ItemTypes.IndexLookup.GetIndex("WaterBucket");

            if(World.TryGetTypeAt(boxedData.item1.VoxelHit, out ushort voxelHitType) && ( voxelHitType == SpreadFluids.waterIndex || voxelHitType == waterIndex ))
            {
                if(SpreadFluids.LookForWater(boxedData.item1.VoxelHit, SpreadFluids.spreadDistance + 1).Count == 0)
                    ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, SpreadFluids.airIndex, player);
                else
                    ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, SpreadFluids.fakewaterIndex, player);

                Inventory inv = Inventory.GetInventory(player);

                inv.TryRemove(oldBucket);
                if(!inv.TryAdd(newBucket))
                    Stockpile.GetStockPile(player).Add(newBucket, 1);
            }
            else if(World.TryGetTypeAt(boxedData.item1.VoxelBuild, out ushort voxelBuildType) && voxelBuildType == waterIndex)
            {
                if(SpreadFluids.LookForWater(boxedData.item1.VoxelBuild, SpreadFluids.spreadDistance + 1).Count == 0)
                    ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, SpreadFluids.airIndex, player);
                else
                    ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, SpreadFluids.fakewaterIndex, player);

                Inventory inv = Inventory.GetInventory(player);

                inv.TryRemove(oldBucket);
                if(!inv.TryAdd(newBucket))
                    Stockpile.GetStockPile(player).Add(newBucket, 1);
            }
        }
    }
}
