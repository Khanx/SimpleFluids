using ExtendedAPI.Types;
using Pipliz;
using Shared;


namespace SimpleFluids.Fluids.Lava
{
    [AutoLoadType]
    public class FakeSimpleLava : FakeFluid
    {
        public FakeSimpleLava()
        {
            key = "Khanx.SimpleFluids.Fake.Lava";
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
