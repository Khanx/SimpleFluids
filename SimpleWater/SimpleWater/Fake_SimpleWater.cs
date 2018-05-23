using ExtendedAPI.Types;
using Pipliz;
using Shared;
using System.Collections.Generic;

namespace SimpleWater
{
    [AutoLoadType]
    public class Fake_SimpleWater : BaseType
    {
        public Fake_SimpleWater()
        {
            key = "Fake.SimpleWater";
        }

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
            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("SimpleWater");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            //Remove
            if(onUpdateAdjacent.changedOldType == fakewaterIndex && onUpdateAdjacent.changedNewType != waterIndex)
            {
                if(SpreadWater.LookForWater(onUpdateAdjacent.updatePosition, SpreadWater.spreadDistance + 1).Count == 0)
                    Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of spread by time
                        {
                            ServerManager.TryChangeBlock(onUpdateAdjacent.updatePosition, airIndex);
                        }, SpreadWater.spreadSpeed/10);
            }

            //Add
            if(onUpdateAdjacent.changedOldType != fakewaterIndex && onUpdateAdjacent.changedNewType == airIndex)
            {
                List<Vector3Int> nearSourceOfWater = SpreadWater.LookForWater(onUpdateAdjacent.changedPosition, SpreadWater.spreadDistance + 1);
                if(nearSourceOfWater.Count > 0)
                    foreach(Vector3Int source in nearSourceOfWater)
                    {
                        List<Vector3Int>[] typesToAddOrderedByDistance = SpreadWater.GetOrderedPositionsToSpreadWater(source, SpreadWater.spreadDistance);

                        //Spread
                        for(int i = 0; i < typesToAddOrderedByDistance.Length; i++)
                        {
                            List<Vector3Int> positions = typesToAddOrderedByDistance[i];
                            if(positions.Count != 0)
                                foreach(Vector3Int pos in positions)
                                    ServerManager.TryChangeBlock(pos, fakewaterIndex);
                        }
                    }
            }
        }
    }
}
