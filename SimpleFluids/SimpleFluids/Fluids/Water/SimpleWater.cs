using ExtendedAPI.Types;


namespace SimpleFluids.Fluids.Water
{
    [AutoLoadType]
    public class SimpleWater : Fluid
    {
        public SimpleWater()
        {
            key = "Khanx.SimpleFluids.Water";
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
