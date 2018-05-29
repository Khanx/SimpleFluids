using BlockTypes.Builtin;
using ExtendedAPI.Types;
using Pipliz;
using Shared;


namespace SimpleFluids.Buckets
{
    [AutoLoadType]
    public class EmptyBucket : BaseType
    {
        public EmptyBucket() { key = "Khanx.SimpleFluids.EmptyBucket"; }

        public override void OnRightClickWith(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player)
                return;

            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("water");
            ushort oldBucket = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.EmptyBucket");
            ushort newBucketW = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.WaterBucket");
            ushort newBucketL = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.LavaBucket");

            if(World.TryGetTypeAt(boxedData.item1.VoxelHit, out ushort voxelHitType) && ( voxelHitType == Fluids.Water.Water.fluid || voxelHitType == Fluids.Lava.Lava.fluid ))
            {
                if(voxelHitType == Fluids.Water.Water.fluid)
                {
                    if(SpreadFluids.LookForSources(boxedData.item1.VoxelHit, Fluids.Water.Water.spreadDistance + 1, Fluids.Water.Water.fluid, Fluids.Water.Water.fluid).Count == 0)
                        ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, BuiltinBlocks.Air, player);
                    else
                        ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, Fluids.Water.Water.fakeFluid, player);

                    Inventory inv = Inventory.GetInventory(player);

                    inv.TryRemove(oldBucket);
                    if(!inv.TryAdd(newBucketW))
                        Stockpile.GetStockPile(player).Add(newBucketW, 1);
                }
                else if(voxelHitType == Fluids.Lava.Lava.fluid)
                {
                    if(SpreadFluids.LookForSources(boxedData.item1.VoxelHit, Fluids.Lava.Lava.spreadDistance + 1, Fluids.Lava.Lava.fluid, Fluids.Lava.Lava.fluid).Count == 0)
                        ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, BuiltinBlocks.Air, player);
                    else
                        ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, Fluids.Lava.Lava.fakeFluid, player);

                    Inventory inv = Inventory.GetInventory(player);

                    inv.TryRemove(oldBucket);
                    if(!inv.TryAdd(newBucketL))
                        Stockpile.GetStockPile(player).Add(newBucketL, 1);
                }
            }
            else if(World.TryGetTypeAt(boxedData.item1.VoxelBuild, out ushort voxelBuildType) && voxelBuildType == waterIndex)
            {
                if(SpreadFluids.LookForSources(boxedData.item1.VoxelBuild, Fluids.Water.Water.spreadDistance + 1, Fluids.Water.Water.fluid, Fluids.Water.Water.fakeFluid).Count == 0)
                    ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, BuiltinBlocks.Air, player);
                else
                    ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, Fluids.Water.Water.fakeFluid, player);

                Inventory inv = Inventory.GetInventory(player);

                inv.TryRemove(oldBucket);
                if(!inv.TryAdd(newBucketW))
                    Stockpile.GetStockPile(player).Add(newBucketW, 1);
            }
        }
    }
}
