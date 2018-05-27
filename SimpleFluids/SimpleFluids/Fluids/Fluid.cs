using BlockTypes.Builtin;
using ExtendedAPI.Types;
using Pipliz;
using Shared;
using System.Collections.Generic;


namespace SimpleFluids.Fluids
{
    public abstract class Fluid : BaseType
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

        public override void OnRightClickOn(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player || null == boxedData)
                return;

            ItemTypes.ItemType item = ItemTypes.GetType(boxedData.item1.typeSelected);
            if(null != item && item.IsPlaceable && !item.NeedsBase) //Check the type that you want to add
            {
                ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, boxedData.item1.typeSelected);
                ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, BuiltinBlocks.Air);
            }
        }

        //This method spread fluid when change the status of one block adjacent of this one [CALLBACK]
        public override void RegisterOnUpdateAdjacent(ItemTypesServer.OnUpdateData onUpdateAdjacent)
        {
            if(onUpdateAdjacent.changedOldType != fakeFluid && onUpdateAdjacent.changedOldType != fluid && onUpdateAdjacent.changedNewType == BuiltinBlocks.Air)
            {
                List<Vector3Int>[] typesToAddOrderedByDistance = SpreadFluids.GetOrderedPositionsToSpread(onUpdateAdjacent.updatePosition, spreadDistance, fluid, fakeFluid);

                //Spread
                float time = spreadSpeed;   //It is float because later it use time / 10
                if(typesToAddOrderedByDistance.Length > 0)
                    for(int i = 0; i < typesToAddOrderedByDistance.Length; i++)
                    {
                        List<Vector3Int> positions = typesToAddOrderedByDistance[i];
                        if(null != positions && positions.Count != 0)
                            Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of spread by time
                            {
                                foreach(Vector3Int pos in positions)
                                    if(World.TryGetTypeAt(pos, out ushort posType) && BuiltinBlocks.Air == posType)
                                        ServerManager.TryChangeBlock(pos, fakeFluid);

                                //Free Memory
                                positions.Clear();
                            }, time / 10);
                        time += spreadSpeed;
                    }
            }
        }

        //Spread the fluid when this block is added to the world
        public override void RegisterOnAdd(Vector3Int position, ushort newType, Players.Player causedBy)
        {
            List<Vector3Int>[] typesToAddOrderedByDistance = SpreadFluids.GetOrderedPositionsToSpread(position, spreadDistance, fluid, fakeFluid);

            //Spread
            float time = spreadSpeed;   //It is float because later it use time / 10
            if(typesToAddOrderedByDistance.Length > 0)
                for(int i = 0; i < typesToAddOrderedByDistance.Length; i++)
                {
                    List<Vector3Int> positions = typesToAddOrderedByDistance[i];
                    if(null != positions && positions.Count != 0)
                        Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of spread by time
                        {
                            foreach(Vector3Int pos in positions)
                                if(World.TryGetTypeAt(pos, out ushort posType) && BuiltinBlocks.Air == posType)
                                    ServerManager.TryChangeBlock(pos, fakeFluid);
                            //Free Memory
                            positions.Clear();
                        }, time / 10);
                    time += spreadSpeed;
                }
        }

        //Remove the fluid produced by this block
        public override void RegisterOnRemove(Vector3Int position, ushort type, Players.Player causedBy)
        {
            //List of types that shouldn't be removed
            List<Vector3Int> notRemoveTypes = new List<Vector3Int>();
            //Positions where there are water that can affect
            List<Vector3Int> nearSource = SpreadFluids.LookForSources(position, ( spreadDistance * 2 + 1 ), fluid, fakeFluid);

            foreach(Vector3Int pos in nearSource)
                notRemoveTypes.AddRange(SpreadFluids.GetUnorderedPositionsToSpread(pos, spreadDistance, fluid, fakeFluid));

            //Fake water blocks generate by this block of water source
            List<Vector3Int>[] positionsToRemoveFluid = SpreadFluids.GetOrderedPositionsToSpread(position, spreadDistance, fluid, fakeFluid);

            float time = spreadSpeed;
            if(positionsToRemoveFluid.Length > 0)
                for(int i = 0; i < positionsToRemoveFluid.Length; i++)
                {
                    List<Vector3Int> positions = positionsToRemoveFluid[i];
                    if(null != positions && positions.Count != 0)
                        Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of remove by time
                        {
                            foreach(Vector3Int pos in positions)
                                if(!notRemoveTypes.Contains(pos))
                                    if(World.TryGetTypeAt(pos, out ushort posType) && fakeFluid == posType)
                                        ServerManager.TryChangeBlock(pos, BuiltinBlocks.Air);

                            //Free Memory
                            positions.Clear();
                        }, time / 10);
                    time += spreadSpeed;
                }
        }
    }
}
