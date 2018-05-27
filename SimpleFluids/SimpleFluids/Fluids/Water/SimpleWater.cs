using ExtendedAPI.Types;
using Pipliz;
using Shared;


namespace SimpleFluids.Fluids.Water
{
    [AutoLoadType]
    public class SimpleWater : Fluid
    {
        public SimpleWater()
        {
            key = "Khanx.SimpleFluids.Water";
        }

        public override void OnRightClickOn(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            base.OnRightClickOn(player, boxedData);
        }

        public override void RegisterOnAdd(Vector3Int position, ushort newType, Players.Player causedBy)
        {
            base.RegisterOnAdd(position, newType, causedBy);
        }

        public override void RegisterOnRemove(Vector3Int position, ushort type, Players.Player causedBy)
        {
            base.RegisterOnRemove(position, type, causedBy);
        }

        public override void RegisterOnUpdateAdjacent(ItemTypesServer.OnUpdateData onUpdateAdjacent)
        {
            base.RegisterOnUpdateAdjacent(onUpdateAdjacent);
        }
    }
}
