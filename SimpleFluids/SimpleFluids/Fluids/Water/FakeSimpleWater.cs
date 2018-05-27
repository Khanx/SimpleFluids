using ExtendedAPI.Types;
using Pipliz;
using Shared;


namespace SimpleFluids.Fluids.Water
{
    [AutoLoadType]
    public class FakeSimpleWater : FakeFluid
    {
        public FakeSimpleWater()
        {
            key = "Khanx.SimpleFluids.Fake.Water";
        }

        public override void OnRightClickOn(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            base.OnRightClickOn(player, boxedData);
        }

        public override void RegisterOnUpdateAdjacent(ItemTypesServer.OnUpdateData onUpdateAdjacent)
        {
            base.RegisterOnUpdateAdjacent(onUpdateAdjacent);
        }
    }
}
