using ExtendedAPI.Types;


namespace SimpleFluids.Buckets
{
    [AutoLoadType]
    public class WaterBucket : FluidBucket
    {
        public WaterBucket()
        {
            key = "Khanx.SimpleFluids.WaterBucket";
            fluid = EFluids.Water;
        }

        public override void OnRightClickOn(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            base.OnRightClickOn(player, boxedData);
        }
    }
}
