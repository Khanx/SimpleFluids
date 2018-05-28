using Pipliz.JSON;
using Pipliz;
using System.Collections.Generic;
using BlockTypes.Builtin;


namespace SimpleFluids.Fluids.Water
{
    [ModLoader.ModManager]
    public static class Water
    {
        public const int MAX_DISTANCE = 7;
        public const int MIN_DISTANCE = 2;
        public const int DEFAULT_DISTANCE = 3;

        public const float MAX_SPEED = 10;
        public const float MIN_SPEED = 2;
        public const float DEFAULT_SPEED = 4;

        public static int spreadDistance;
        public static float spreadSpeed;

        public static ushort fluid;
        public static ushort fakeFluid;

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, "Khanx.SimpleFluids.Water.LoadConfig")]
        public static void LoadConfig(Dictionary<string, ItemTypesServer.ItemTypeRaw> a)
        {
            fluid = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.Water");
            fakeFluid = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.Fake.Water");

            try
            {
                JSONNode config = JSON.Deserialize(SpreadFluids.MODPATH + "/config.json");

                config.TryGetChild("water", out JSONNode waterConfig);

                if(!waterConfig.TryGetAs<int>("spreadDistance", out spreadDistance))
                    spreadDistance = DEFAULT_DISTANCE;
                else if(spreadDistance > MAX_DISTANCE || spreadDistance < MIN_DISTANCE)
                {
                    Log.Write(string.Format("<color=red>Warning: Water spreadDistance must be between {0} and {1} included</color>", MIN_DISTANCE, MAX_DISTANCE));
                    spreadDistance = DEFAULT_DISTANCE;
                }

                if(!waterConfig.TryGetAs<float>("spreadSpeed", out spreadSpeed))
                    spreadSpeed = DEFAULT_SPEED;
                else if(spreadSpeed > MAX_SPEED || spreadSpeed < MIN_SPEED)
                {
                    Log.Write(string.Format("<color=red>Warning: Water spreadSpeed must be between {0} and {1} included</color>", MIN_SPEED, MAX_SPEED));
                    spreadSpeed = DEFAULT_SPEED;
                }
            }
            catch(System.Exception)
            {
                spreadDistance = DEFAULT_DISTANCE;
                spreadSpeed = DEFAULT_SPEED;
            }


            ExtendedAPI.Types.TypeManager.TryGet("Khanx.SimpleFluids.Water", out ExtendedAPI.Types.BaseType water);
            ( (SimpleWater)water ).Inicialize(fluid, fakeFluid, spreadDistance, spreadSpeed);

            ExtendedAPI.Types.TypeManager.TryGet("Khanx.SimpleFluids.Fake.Water", out ExtendedAPI.Types.BaseType fakewater);
            ( (FakeSimpleWater)fakewater ).Inicialize(fluid, fakeFluid, spreadDistance, spreadSpeed);
        }

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerHit, "Khanx.SimpleFluids.NotKillPlayerOnHitWater")]
        public static void NotKillPlayerOnHitWater(Players.Player player, ModLoader.OnHitData d)
        {
            if(null == player || null == d || d.HitSourceType != ModLoader.OnHitData.EHitSourceType.FallDamage)
                return;

            Vector3Int position = new Vector3Int(player.Position);
            ushort hitType = 0;

            do
            {
                if(!World.TryGetTypeAt(position, out hitType))
                    break;

                if(hitType == fluid || hitType == fakeFluid)
                    d.ResultDamage = 0;

                position += Vector3Int.down;
            }
            while(hitType == BuiltinBlocks.Air);
        }
    }
}
