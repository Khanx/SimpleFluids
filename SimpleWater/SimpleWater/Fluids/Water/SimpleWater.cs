using ExtendedAPI.Types;
using Pipliz;
using Shared;
using System.Collections.Generic;

namespace SimpleWater.Water
{
    [AutoLoadType]
    public class SimpleWater : BaseType
    {
        public SimpleWater()
        {
            key = "SimpleWater";
        }

        //This method allows to "build" (replace) this block [CALLBACK]
        public override void OnRightClickOn(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player || null == boxedData)
                return;

            ItemTypes.ItemType item = ItemTypes.GetType(boxedData.item1.typeSelected);
            if(null != item && item.IsPlaceable && !item.NeedsBase) //Check the type that you want to add
            {
                ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, boxedData.item1.typeSelected);
                ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, BlockTypes.Builtin.BuiltinBlocks.Air);
            }
        }

        //This method spread water when change the status of one block adjacent of this one [CALLBACK]
        public override void RegisterOnUpdateAdjacent(ItemTypesServer.OnUpdateData onUpdateAdjacent)
        {
            if(onUpdateAdjacent.changedOldType != SpreadFluids.fakewaterIndex && onUpdateAdjacent.changedOldType != SpreadFluids.waterIndex && onUpdateAdjacent.changedNewType == SpreadFluids.airIndex)
            {
                List<Vector3Int>[] typesToAddOrderedByDistance = SpreadFluids.GetOrderedPositionsToSpread(onUpdateAdjacent.updatePosition, SpreadFluids.spreadDistance);

                //Spread
                float time = SpreadFluids.spreadSpeed;   //It is float because later it use time / 10
                if(typesToAddOrderedByDistance.Length > 0)
                    for(int i = 0; i < typesToAddOrderedByDistance.Length; i++)
                    {
                        List<Vector3Int> positions = typesToAddOrderedByDistance[i];
                        if(null != positions && positions.Count != 0)
                            Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of spread by time
                            {
                                foreach(Vector3Int pos in positions)
                                    if(World.TryGetTypeAt(pos, out ushort posType) && SpreadFluids.airIndex == posType)
                                        ServerManager.TryChangeBlock(pos, SpreadFluids.fakewaterIndex);

                                //Free Memory
                                positions.Clear();
                            }, time / 10);
                        time += SpreadFluids.spreadSpeed;
                    }
            }
        }

        //Spread the water when this block is added to the world
        public override void RegisterOnAdd(Vector3Int position, ushort newType, Players.Player causedBy)
        {

            List<Vector3Int>[] typesToAddOrderedByDistance = SpreadFluids.GetOrderedPositionsToSpread(position, SpreadFluids.spreadDistance);

            //Spread
            float time = SpreadFluids.spreadSpeed;   //It is float because later it use time / 10
            if(typesToAddOrderedByDistance.Length > 0)
                for(int i = 0; i < typesToAddOrderedByDistance.Length; i++)
                {
                    List<Vector3Int> positions = typesToAddOrderedByDistance[i];
                    if(null != positions && positions.Count != 0)
                        Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of spread by time
                        {
                            foreach(Vector3Int pos in positions)
                                if(World.TryGetTypeAt(pos, out ushort posType) && SpreadFluids.airIndex == posType)
                                    ServerManager.TryChangeBlock(pos, SpreadFluids.fakewaterIndex);

                            //Free Memory
                            positions.Clear();
                        }, time / 10);
                    time += SpreadFluids.spreadSpeed;
                }

        }

        //Remove the water produced by this block
        public override void RegisterOnRemove(Vector3Int position, ushort type, Players.Player causedBy)
        {
            //List of types that shouldn't be removed
            List<Vector3Int> notRemoveTypes = new List<Vector3Int>();
            //Positions where there are water that can affect
            List<Vector3Int> nearWater = SpreadFluids.LookForWater(position, ( SpreadFluids.spreadDistance * 2 + 1 ));

            foreach(Vector3Int pos in nearWater)
                notRemoveTypes.AddRange(SpreadFluids.GetUnorderedPositionsToSpread(pos, SpreadFluids.spreadDistance));

            //Fake water blocks generate by this block of water source
            List<Vector3Int>[] positionsToRemoveWater = SpreadFluids.GetOrderedPositionsToSpread(position, SpreadFluids.spreadDistance);

            float time = SpreadFluids.spreadSpeed;
            if(positionsToRemoveWater.Length > 0)
                for(int i = 0; i < positionsToRemoveWater.Length; i++)
                {
                    List<Vector3Int> positions = positionsToRemoveWater[i];
                    if(null != positions && positions.Count != 0)
                        Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of remove by time
                        {
                            foreach(Vector3Int pos in positions)
                                if(!notRemoveTypes.Contains(pos))
                                    if(World.TryGetTypeAt(pos, out ushort posType) && SpreadFluids.fakewaterIndex == posType)
                                        ServerManager.TryChangeBlock(pos, SpreadFluids.airIndex);

                            //Free Memory
                            positions.Clear();
                        }, time / 10);
                    time += SpreadFluids.spreadSpeed;
                }
        }
    }
}
