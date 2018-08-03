using ExtendedAPI.Types;
using Pipliz;
using Shared;


namespace SimpleFluids.Fluids
{
    public abstract class Fluid : BaseType
    {
        public EFluids fluid;

        public override void OnRightClickOn(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player || null == boxedData)
                return;

            ItemTypes.ItemType item = ItemTypes.GetType(boxedData.item1.typeSelected);
            if(null != item && item.IsPlaceable && !item.NeedsBase) //Check the type that you want to add
            {
                ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, BlockTypes.Builtin.BuiltinBlocks.Air);
                FluidManager.Remove(boxedData.item1.VoxelHit, fluid, boxedData.item1.typeSelected);
            }
        }

        public override void RegisterOnUpdateAdjacent(ItemTypesServer.OnUpdateData adjacent)
        {
            if(adjacent.changedNewType != BlockTypes.Builtin.BuiltinBlocks.Air)
                return;

            ushort source = FluidManager._fluids[(int)fluid].source;
            ushort fake = FluidManager._fluids[(int)fluid].fake;

            if(adjacent.changedOldType == source || adjacent.changedOldType == fake)
                return;

            Vector3Int pos = FluidManager.ClosestSource(adjacent.changedPosition, fluid);

            if(pos != Vector3Int.maximum)
                FluidManager.Spread(pos, fluid);
        }
    }
}
