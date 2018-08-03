using ExtendedAPI.Types;


namespace SimpleFluids.Fluids.Lava
{
    [AutoLoadType]
    public class SimpleLava : Fluid
    {
        public SimpleLava()
        {
            key = "Khanx.SimpleFluids.Lava";
            fluid = EFluids.Lava;
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
