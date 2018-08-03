using ExtendedAPI.Types;


namespace SimpleFluids.Buckets
{
    public abstract class FluidBucket : BaseType
    {
        public EFluids fluid;

        public override void OnRightClickWith(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            if(null == player)
                return;

            Pipliz.Vector3Int position = boxedData.item1.VoxelBuild;

            if(World.TryGetTypeAt(position, out ushort actualType) && actualType == BlockTypes.Builtin.BuiltinBlocks.Air)
            {
                FluidManager.Spread(position, fluid);

                Inventory inv = Inventory.GetInventory(player);

                inv.TryRemove(FluidManager._fluids[(int)fluid].bucket);
                if(!inv.TryAdd(EmptyBucket.typeEmptyBucket))
                    Stockpile.GetStockPile(player).Add(EmptyBucket.typeEmptyBucket);
            }
        }
    }
}
