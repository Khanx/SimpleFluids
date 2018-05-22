using ExtendedAPI.Types;
using Pipliz;
using Shared;

namespace SimpleWater
{
    [AutoLoadType]
    public class WaterBucket : BaseType
    {
        public WaterBucket() { key = "WaterBucket"; }

        public override void OnRightClickWith(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            Vector3Int position = boxedData.item1.VoxelBuild;

            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("SimpleWater");
            ushort oldBucket = ItemTypes.IndexLookup.GetIndex("WaterBucket");
            ushort newBucket = ItemTypes.IndexLookup.GetIndex("EmptyWaterBucket");

            if(World.TryGetTypeAt(position, out ushort actualType) && actualType == airIndex)
            {
                ServerManager.TryChangeBlock(position, waterIndex, player);

                Inventory inv =Inventory.GetInventory(player);

                inv.TryRemove(oldBucket);
                if(!inv.TryAdd(newBucket))
                    Stockpile.GetStockPile(player).Add(newBucket, 1);
            }
        }
    }

    [AutoLoadType]
    public class EmptyWaterBucket : BaseType
    {
        public EmptyWaterBucket() { key = "EmptyWaterBucket"; }

        public override void OnRightClickWith(Players.Player player, Box<PlayerClickedData> boxedData)
        {

            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("water");
            ushort simplewaterIndex = ItemTypes.IndexLookup.GetIndex("SimpleWater");
            ushort oldBucket = ItemTypes.IndexLookup.GetIndex("EmptyWaterBucket");
            ushort newBucket = ItemTypes.IndexLookup.GetIndex("WaterBucket");

            if(World.TryGetTypeAt(boxedData.item1.VoxelHit, out ushort voxelHitType) && (voxelHitType == simplewaterIndex || voxelHitType == waterIndex))
            {
                ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, airIndex, player);

                Inventory inv = Inventory.GetInventory(player);
                
                inv.TryRemove(oldBucket);
                if(!inv.TryAdd(newBucket))
                    Stockpile.GetStockPile(player).Add(newBucket, 1);
            }
            else if(World.TryGetTypeAt(boxedData.item1.VoxelBuild, out ushort voxelBuildType) && voxelBuildType == waterIndex)
            {
                ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, airIndex, player);

                Inventory inv = Inventory.GetInventory(player);

                inv.TryRemove(oldBucket);
                if(!inv.TryAdd(newBucket))
                    Stockpile.GetStockPile(player).Add(newBucket, 1);
            }
        }
    }
}
