using BlockTypes.Builtin;
using ExtendedAPI.Types;
using Pipliz;
using Shared;
using System.Collections.Generic;


namespace SimpleFluids.Fluids
{
    public class FakeFluid : BaseType
    {
        private ushort fluid { get; set; }
        private ushort fakeFluid { get; set; }
        private int spreadDistance { get; set; }
        private float spreadSpeed { get; set; }

        public void Inicialize(ushort fluid, ushort fakeFluid, int spreadDistance, float spreadSpeed)
        {
            this.fluid = fluid;
            this.fakeFluid = fakeFluid;
            this.spreadDistance = spreadDistance;
            this.spreadSpeed = spreadSpeed;
        }

        //This method allows to "build" (replace) this block [CALLBACK]
        public override void OnRightClickOn(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player || null == boxedData)
                return;

            ItemTypes.ItemType item = ItemTypes.GetType(boxedData.item1.typeSelected);
            if(null != item && item.IsPlaceable && !item.NeedsBase)
            {
                ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, boxedData.item1.typeSelected, player);
                ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, BlockTypes.Builtin.BuiltinBlocks.Air);
            }
        }

        public override void RegisterOnUpdateAdjacent(ItemTypesServer.OnUpdateData onUpdateAdjacent)
        {

            //Remove
            if(onUpdateAdjacent.changedOldType == fakeFluid && onUpdateAdjacent.changedNewType != fluid && onUpdateAdjacent.changedNewType != fakeFluid)
            {
                if(SpreadFluids.LookForSources(onUpdateAdjacent.updatePosition, spreadDistance + 1, fluid, fakeFluid).Count == 0)
                    Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of spread by time
                    {
                        if(World.TryGetTypeAt(onUpdateAdjacent.updatePosition, out ushort actualPosType) && actualPosType == fakeFluid)
                            ServerManager.TryChangeBlock(onUpdateAdjacent.updatePosition, BuiltinBlocks.Air);
                    }, spreadSpeed / 10);
            }

            //Add
            if(onUpdateAdjacent.changedOldType != fakeFluid && onUpdateAdjacent.changedNewType == BuiltinBlocks.Air)
            {
                List<Vector3Int> nearSource = SpreadFluids.LookForSources(onUpdateAdjacent.changedPosition, spreadDistance + 1, fluid, fakeFluid);
                if(nearSource.Count > 0)
                {
                    foreach(Vector3Int source in nearSource)
                    {
                        List<Vector3Int>[] typesToAddOrderedByDistance = SpreadFluids.GetOrderedPositionsToSpread(source, spreadDistance, fluid, fakeFluid);
                        //Spread
                        if(typesToAddOrderedByDistance.Length > 0)
                            for(int i = 0; i < typesToAddOrderedByDistance.Length; i++)
                            {
                                List<Vector3Int> positions = typesToAddOrderedByDistance[i];
                                if(null != positions && positions.Count > 0)
                                    foreach(Vector3Int pos in positions)
                                        if(World.TryGetTypeAt(pos, out ushort actualPosType) && actualPosType == BuiltinBlocks.Air)
                                            ServerManager.TryChangeBlock(pos, fakeFluid);
                            }
                    }
                } //nearSourceOfWater > 0
            }   // Add
        }
    }
}
