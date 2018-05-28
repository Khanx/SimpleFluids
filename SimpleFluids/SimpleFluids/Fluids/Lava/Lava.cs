using Pipliz.JSON;
using Pipliz;
using System.Collections.Generic;

namespace SimpleFluids.Fluids.Lava
{
    [ModLoader.ModManager]
    public static class Lava
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

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, "Khanx.SimpleFluids.Lava.LoadConfig")]
        public static void LoadConfig(Dictionary<string, ItemTypesServer.ItemTypeRaw> a)
        {
            fluid = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.Lava");
            fakeFluid = ItemTypes.IndexLookup.GetIndex("Khanx.SimpleFluids.Fake.Lava");

            try
            {
                JSONNode config = JSON.Deserialize(SpreadFluids.MODPATH + "/config.json");

                config.TryGetChild("lava", out JSONNode lavaConfig);

                if(!lavaConfig.TryGetAs<int>("spreadDistance", out spreadDistance))
                    spreadDistance = DEFAULT_DISTANCE;
                else if(spreadDistance > MAX_DISTANCE || spreadDistance < MIN_DISTANCE)
                {
                    Log.Write(string.Format("<color=red>Warning: Lava spreadDistance must be between {0} and {1} included</color>", MIN_DISTANCE, MAX_DISTANCE));
                    spreadDistance = DEFAULT_DISTANCE;
                }

                if(!lavaConfig.TryGetAs<float>("spreadSpeed", out spreadSpeed))
                    spreadSpeed = DEFAULT_SPEED;
                else if(spreadSpeed > MAX_SPEED || spreadSpeed < MIN_SPEED)
                {
                    Log.Write(string.Format("<color=red>Warning: Lava spreadSpeed must be between {0} and {1} included</color>", MIN_SPEED, MAX_SPEED));
                    spreadSpeed = DEFAULT_SPEED;
                }
            }
            catch(System.Exception)
            {
                spreadDistance = DEFAULT_DISTANCE;
                spreadSpeed = DEFAULT_SPEED;
            }

            ExtendedAPI.Types.TypeManager.TryGet("Khanx.SimpleFluids.Lava", out ExtendedAPI.Types.BaseType lava);
            ((SimpleLava)lava).Inicialize(fluid, fakeFluid, spreadDistance, spreadSpeed);

            ExtendedAPI.Types.TypeManager.TryGet("Khanx.SimpleFluids.Fake.Lava", out ExtendedAPI.Types.BaseType fakeLava);
            ( (FakeSimpleLava)fakeLava ).Inicialize(fluid, fakeFluid, spreadDistance, spreadSpeed);
        }
    }
}
