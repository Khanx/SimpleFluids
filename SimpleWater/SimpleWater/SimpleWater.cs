﻿using ExtendedAPI.Types;
using Pipliz;
using Shared;
using System.Collections.Generic;

namespace SimpleWater
{
    [AutoLoadType]
    public class SimpleWater : BaseType
    {
        private int spreadDistance;
        private int spreadSpeed = 4;

        public SimpleWater()
        {
            key = "SimpleWater";
            spreadDistance = SpreadWater.spreadDistance;
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
            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("SimpleWater");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            List<Vector3Int>[] typesToAddOrderedByDistance = SpreadWater.GetOrderedPositionsToSpreadWater(position, spreadDistance);

            //Spread
            float time = spreadSpeed;   //It is float because later it use time / 10
            for(int i = 0; i < typesToAddOrderedByDistance.Length; i++)
            {
                List<Vector3Int> positions = typesToAddOrderedByDistance[i];
                if(positions.Count != 0)
                    Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of spread by time
                    {
                        foreach(Vector3Int pos in positions)
                            ServerManager.TryChangeBlock(pos, fakewaterIndex);
                    }, time / 10);
                time += spreadSpeed;
            }
        }

        public override void RegisterOnRemove(Vector3Int position, ushort type, Players.Player causedBy)
        {
            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("SimpleWater");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            //List of types that shouldn't be removed
            List<Vector3Int> notRemoveTypes = new List<Vector3Int>();
            //Positions where there are water that can affect
            List<Vector3Int> nearWater = SpreadWater.LookForWater(position, (spreadDistance*2 + 1) );

            foreach(Vector3Int pos in nearWater)
                notRemoveTypes.AddRange(SpreadWater.GetPositionsToSpreadWater(pos, spreadDistance));

            //Fake water blocks generate by this block of water source
            List<Vector3Int>[] positionsToRemoveWater = SpreadWater.GetOrderedPositionsToSpreadWater(position, spreadDistance);

            float time = spreadSpeed;
            for(int i = 0; i < positionsToRemoveWater.Length; i++)
            {
                List<Vector3Int> positions = positionsToRemoveWater[i];
                if(positions.Count != 0)
                    Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate () //Gives the effect of remove by time
                    {
                        foreach(Vector3Int pos in positions)
                            if(!notRemoveTypes.Contains(pos))
                                if(World.TryGetTypeAt(pos, out ushort posType) && fakewaterIndex == posType)
                                    ServerManager.TryChangeBlock(pos, airIndex);
                    }, time / 10);
                time += spreadSpeed;
            }
        }
    }
}
