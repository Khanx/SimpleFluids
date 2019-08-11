
using BlockTypes;
using Pipliz;

namespace SimpleFluids
{
    [ModLoader.ModManager]
    public static class Buckets
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, "Khanx.SimpleFluids.Buckets.OnPlayerClicked")]
        public static void OnPlayerClicked(Players.Player player, Shared.PlayerClickedData playerClickedData)
        {
            if (player == null || playerClickedData.ClickType != Shared.PlayerClickedData.EClickType.Right)
                return;

            EFluids fluid = EFluids.MAX;

            if (playerClickedData.TypeSelected == ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.WaterBucket"))
                fluid = EFluids.Water;
            else if (playerClickedData.TypeSelected == ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.LavaBucket"))
                fluid = EFluids.Lava;
            else if (playerClickedData.TypeSelected == ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.EmptyBucket"))
            {
                //fluid -> BUCKET
                return;
            }
            else
                return;

            Pipliz.Vector3Int position = playerClickedData.GetVoxelHit().PositionBuild;

            if (World.TryGetTypeAt(position, out ushort actualType) && actualType == BuiltinBlocks.Indices.air)
            {
                FluidManager.Spread(position, fluid);

                Inventory inv = player.Inventory;

                inv.TryRemove(FluidManager._fluids[(int)fluid].bucket);
                if (!inv.TryAdd(ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.EmptyBucket")))
                    if(player.ActiveColony != null && player.ActiveColony.Stockpile != null)
                        player.ActiveColony.Stockpile.Add(ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.EmptyBucket"));
            }
        }
    }
}
