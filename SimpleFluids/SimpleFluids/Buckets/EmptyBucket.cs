using System.Collections.Generic;

using Pipliz;

using ExtendedAPI.Types;


namespace SimpleFluids.Buckets
{
    [AutoLoadType]
    public class EmptyBucket : BaseType
    {
        public static Dictionary<ushort, EFluids> fluidsInfo = new Dictionary<ushort, EFluids>();
        public static ushort typeEmptyBucket;

        public EmptyBucket()
        {
            key = "Khanx.SimpleFluids.EmptyBucket";

            typeEmptyBucket = ItemTypes.IndexLookup.GetIndex(key);
        }

        public override void OnRightClickWith(Players.Player player, Box<Shared.PlayerClickedData> boxedData)
        {
            if(null == player)
                return;

            ushort typeTouch;
            Vector3Int position = Vector3Int.maximum;

            if(World.TryGetTypeAt(boxedData.item1.VoxelHit, out typeTouch))
                position = boxedData.item1.VoxelHit;
            else if(World.TryGetTypeAt(boxedData.item1.VoxelBuild, out typeTouch))
                position = boxedData.item1.VoxelBuild;
            else
                return;

            foreach(ushort type in fluidsInfo.Keys)
            {
                if(type != typeTouch)
                    continue;

                FluidManager.Remove(position, fluidsInfo[type]);

                Inventory inv = Inventory.GetInventory(player);

                inv.TryRemove(typeEmptyBucket);
                if(!inv.TryAdd(FluidManager._fluids[(int)fluidsInfo[type]].bucket))
                    Stockpile.GetStockPile(player).Add(FluidManager._fluids[(int)fluidsInfo[type]].bucket, 1);

                break;
            }
        }
    }
}
