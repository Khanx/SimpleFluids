
using BlockTypes;
using Pipliz;

namespace SimpleFluids
{
    [ModLoader.ModManager]
    public static class NoWaterDamage
    {
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, "Khanx.SimpleFluids.NotKillPlayerOnHitWater")]
        public static void NotKillPlayerOnHitWater(Players.Player player, ModLoader.OnHitData d)
        {
            if (null == player || null == d || d.HitSourceType != ModLoader.OnHitData.EHitSourceType.FallDamage)
                return;

            Vector3Int position = new Vector3Int(player.Position);
            ushort hitType = 0;

            int max = 10;
            do
            {
                if (!World.TryGetTypeAt(position, out hitType))
                    break;

                if (hitType == FluidManager._fluids[(int)EFluids.Water].source || hitType == FluidManager._fluids[(int)EFluids.Water].fake)
                {
                    d.ResultDamage = 0;
                    break;
                }

                position += Vector3Int.down;

                if (max-- <= 0)
                    break;
            }
            while (hitType == BuiltinBlocks.Indices.air);
        }

    }
}
