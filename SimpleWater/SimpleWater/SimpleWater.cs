using ExtendedAPI.Types;
using Pipliz;
using Shared;
using System.Collections.Generic;

namespace SimpleWater
{
    [AutoLoadType]
    public class SimpleWater : BaseType
    {
        public SimpleWater()
        {
            key = "SimpleWater";
        }

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

        public override void RegisterOnAdd(Vector3Int position, ushort newType, Players.Player causedBy)
        {

            List<Vector3Int>[] typesToAddOrderedByDistance = SpreadWater.GetOrderedPositionsToSpreadWater(position, SpreadWater.spreadDistance);

            //Spread
            float time = SpreadWater.spreadSpeed;   //It is float because later it use time / 10
            if(typesToAddOrderedByDistance.Length > 0)
                for(int i = 0; i < typesToAddOrderedByDistance.Length; i++)
                {
                    List<Vector3Int> positions = typesToAddOrderedByDistance[i];
                    if(null != positions && positions.Count != 0)
                        Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of spread by time
                        {
                            foreach(Vector3Int pos in positions)
                                if(World.TryGetTypeAt(pos, out ushort posType) && SpreadWater.airIndex == posType)
                                    ServerManager.TryChangeBlock(pos, SpreadWater.fakewaterIndex);
                        }, time / 10);
                    time += SpreadWater.spreadSpeed;
                }
        }

        public override void RegisterOnRemove(Vector3Int position, ushort type, Players.Player causedBy)
        {
            //List of types that shouldn't be removed
            List<Vector3Int> notRemoveTypes = new List<Vector3Int>();
            //Positions where there are water that can affect
            List<Vector3Int> nearWater = SpreadWater.LookForWater(position, ( SpreadWater.spreadDistance * 2 + 1 ));

            foreach(Vector3Int pos in nearWater)
                notRemoveTypes.AddRange(SpreadWater.GetPositionsToSpreadWater(pos, SpreadWater.spreadDistance));

            //Fake water blocks generate by this block of water source
            List<Vector3Int>[] positionsToRemoveWater = SpreadWater.GetOrderedPositionsToSpreadWater(position, SpreadWater.spreadDistance);

            float time = SpreadWater.spreadSpeed;
            if(positionsToRemoveWater.Length > 0)
                for(int i = 0; i < positionsToRemoveWater.Length; i++)
                {
                    List<Vector3Int> positions = positionsToRemoveWater[i];
                    if(null != positions && positions.Count != 0)
                        Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of remove by time
                        {
                            foreach(Vector3Int pos in positions)
                                if(!notRemoveTypes.Contains(pos))
                                    if(World.TryGetTypeAt(pos, out ushort posType) && SpreadWater.fakewaterIndex == posType)
                                        ServerManager.TryChangeBlock(pos, SpreadWater.airIndex);
                        }, time / 10);
                    time += SpreadWater.spreadSpeed;
                }
        }
    }
}
