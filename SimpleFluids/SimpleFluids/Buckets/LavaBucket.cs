using BlockTypes.Builtin;
using ExtendedAPI.Types;
using Pipliz;
using Shared;


namespace SimpleFluids.Buckets
{
    [AutoLoadType]
    public class LavaBucket : BaseType
    {
        public LavaBucket() { key = "Khanx.SimpleFluids.LavaBucket"; }

        public override void OnRightClickWith(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player)
                return;

            Vector3Int position = boxedData.item1.VoxelBuild;

            ushort oldBucket = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.LavaBucket");
            ushort newBucket = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.EmptyBucket");

            if(World.TryGetTypeAt(position, out ushort actualType) && actualType == BuiltinBlocks.Air)
            {
                ServerManager.TryChangeBlock(position, SimpleFluids.Fluids.Lava.Lava.fluid, player);

                Inventory inv = Inventory.GetInventory(player);

                inv.TryRemove(oldBucket);
                if(!inv.TryAdd(newBucket))
                    Stockpile.GetStockPile(player).Add(newBucket, 1);
            }
        }
    }

}
