using ExtendedAPI.Types;


namespace SimpleFluids.Buckets
{
    [AutoLoadType]
    public class LavaBucket : FluidBucket
    {
        public LavaBucket()
        {
            key = "Khanx.SimpleFluids.LavaBucket";
            fluid = EFluids.Lava;
        }

        public override void OnRightClickOn(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            base.OnRightClickOn(player, boxedData);
        }
    }
}
