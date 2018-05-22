using ExtendedAPI.Types;
using Pipliz;
using Shared;

namespace SimpleWater
{
    
    [AutoLoadType]
    public class Fake_SimpleWater : BaseType
    {
        public Fake_SimpleWater()
        {
            key = "Fake.SimpleWater";
        }

        public override void OnRightClickOn(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player || null == boxedData)
                return;

            ItemTypes.ItemType item = ItemTypes.GetType(boxedData.item1.typeSelected);
            if(null != item && item.IsPlaceable && !item.NeedsBase)
            {
                ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, boxedData.item1.typeSelected, player);
                ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, BlockTypes.Builtin.BuiltinBlocks.Air);
            }
        }
    }
}
