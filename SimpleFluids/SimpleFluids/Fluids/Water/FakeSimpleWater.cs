using ExtendedAPI.Types;


namespace SimpleFluids.Fluids.Water
{
    [AutoLoadType]
    public class FakeSimpleWater : Fluid
    {
        public FakeSimpleWater()
        {
            key = "Khanx.SimpleFluids.Fake.Water";
            fluid = EFluids.Water;
        }

        public override void OnRightClickOn(Players.Player player, Pipliz.Box<Shared.PlayerClickedData> boxedData)
        {
            base.OnRightClickOn(player, boxedData);
        }

        public override void RegisterOnUpdateAdjacent(ItemTypesServer.OnUpdateData adjacent)
        {
            base.RegisterOnUpdateAdjacent(adjacent);
        }
    }
}
