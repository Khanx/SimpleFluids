using BlockEntities;
using BlockTypes;
using Pipliz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFluids
{
    [ModLoader.ModManager]
    public static class WaterFluid
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, "Khanx.SimpleFluids.GetModPath")]
        public static void LoadConfig()
        {
            FluidInfo Water = new FluidInfo();

            Water.source = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.Water");
            Water.fake = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.Fake.Water");
            Water.bucket = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.WaterBucket");

            Water.distance = 4;
            Water.time = 250;

            FluidManager._fluids[(int)EFluids.Water] = Water;
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, "Khanx.SimpleFluids.RemoveWater.OnPlayerClicked")]
        public static void OnPlayerClicked(Players.Player player, Shared.PlayerClickedData playerClickedData)
        {
            if (player == null || playerClickedData.ClickType != Shared.PlayerClickedData.EClickType.Right || playerClickedData.HitType != Shared.PlayerClickedData.EHitType.Block)
                return;

            if (playerClickedData.GetVoxelHit().TypeHit != FluidManager._fluids[(int)EFluids.Water].source && playerClickedData.GetVoxelHit().TypeHit != FluidManager._fluids[(int)EFluids.Water].fake)
                return;

            ItemTypes.ItemType item = ItemTypes.GetType(playerClickedData.TypeToBuild);
            if (null != item && item.IsPlaceable && !item.NeedsBase) //Check the type that you want to add
            {
                ServerManager.TryChangeBlock(playerClickedData.GetVoxelHit().PositionBuild, BuiltinBlocks.Indices.air);
                FluidManager.Remove(playerClickedData.GetVoxelHit().BlockHit, EFluids.Water, playerClickedData.TypeToBuild);
            }
        }
    }


    [BlockEntityAutoLoader]
    public class WaterType : IUpdatedAdjacentType, IMultiBlockEntityMapping
    {
        public IEnumerable<ItemTypes.ItemType> TypesToRegister { get { return types; } }

        ItemTypes.ItemType[] types = new ItemTypes.ItemType[]
            {
                 ItemTypes.GetType("Khanx.SimpleFluids.Water"),
                 ItemTypes.GetType("Khanx.SimpleFluids.Fake.Water")
            };

        public void OnUpdateAdjacent(AdjacentUpdateData adjacent)
        {
            if (adjacent.NewType != BuiltinBlocks.Types.air)
                return;

            ushort source = FluidManager._fluids[(int)EFluids.Water].source;
            ushort fake = FluidManager._fluids[(int)EFluids.Water].fake;

            if (adjacent.OldType.ItemIndex == source || adjacent.OldType.ItemIndex == fake)
                return;

            Vector3Int pos = FluidManager.ClosestSource(adjacent.ChangePosition, EFluids.Water);

            if (pos != Vector3Int.maximum)
                FluidManager.Spread(pos, EFluids.Water);
        }
    }
}
