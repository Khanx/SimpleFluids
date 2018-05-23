using System.Collections.Generic;
using Pipliz;
using Pipliz.JSON;

namespace SimpleWater
{
    public struct Node
    {
        public int distance;
        public Vector3Int position;

        public Node(int distance, Vector3Int position)
        {
            this.distance = distance;
            this.position = position;
        }
    }

    [ModLoader.ModManager]
    public static class SpreadWater
    {
        public const int MAX_DISTANCE = 7;
        public const int MIN_DISTANCE = 2;
        public const int DEFAULT_DISTANCE = 3;

        public const float MAX_SPEED = 10;
        public const float MIN_SPEED = 2;
        public const float DEFAULT_SPEED = 4;

        public static int spreadDistance;
        public static float spreadSpeed;

        public static ushort airIndex;
        public static ushort waterIndex;
        public static ushort fakewaterIndex;

        private static Vector3Int[] adjacents = { Vector3Int.left, Vector3Int.forward, Vector3Int.right, Vector3Int.back };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterAddingBaseTypes, "Khanx.SimpleWater.Load")]
        public static void Load(Dictionary<string, ItemTypesServer.ItemTypeRaw> a)
        {
            airIndex = ItemTypes.IndexLookup.GetIndex("air");
            waterIndex = ItemTypes.IndexLookup.GetIndex("SimpleWater");
            fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            try
            {
                JSONNode config = JSON.Deserialize("./gamedata/mods/Khanx/SimpleWater/config.json");

                if(!config.TryGetAs<int>("spreadDistance", out spreadDistance))
                    spreadDistance = 3;
                else if(spreadDistance > MAX_DISTANCE || spreadDistance < MIN_DISTANCE)
                {
                    Log.Write(string.Format("<color=red>Warning: spreadDistance must be between {0} and {1} included</color>", MAX_DISTANCE, MIN_DISTANCE));
                    spreadDistance = DEFAULT_DISTANCE;
                }

                if(!config.TryGetAs<float>("spreadSpeed", out spreadSpeed))
                    spreadSpeed = 4;
                else if(spreadSpeed > MAX_SPEED || spreadSpeed < MIN_SPEED)
                {
                    Log.Write(string.Format("<color=red>Warning: spreadSpeed must be between {0} and {1} included</color>", MAX_SPEED, MIN_SPEED));
                    spreadSpeed = DEFAULT_SPEED;
                }
            }
            catch(System.Exception)
            {
                spreadDistance = DEFAULT_DISTANCE;
                spreadSpeed = DEFAULT_SPEED;
            }
        }

        public static List<Vector3Int>[] GetOrderedPositionsToSpreadWater(Vector3Int start, int distance)
        {
            List<Vector3Int>[] orderedPositions = new List<Vector3Int>[distance + 1];

            Queue<Node> toVisit = new Queue<Node>();
            List<Vector3Int> alreadyVisited = new List<Vector3Int>();

            toVisit.Enqueue(new Node(0, start));
            while(toVisit.Count > 0)
            {
                Node current = toVisit.Dequeue();

                if(alreadyVisited.Contains(current.position))
                    continue;

                alreadyVisited.Add(current.position);

                if(orderedPositions[current.distance] != null)
                    orderedPositions[current.distance].Add(current.position);
                else
                {
                    List<Vector3Int> lista = new List<Vector3Int>();
                    lista.Add(current.position);
                    orderedPositions[current.distance] = lista;
                }



                //We do not look for adjacent ones if we are already at the maximum distance
                if(current.distance == distance)
                    continue;

                //If the lower block is air, it propagates downward
                Vector3Int checkPositionDown = current.position + Vector3Int.down;
                if(World.TryGetTypeAt(checkPositionDown, out ushort actualDown))
                    if(actualDown == airIndex || actualDown == fakewaterIndex || actualDown == waterIndex)
                    {
                        if(actualDown != waterIndex)
                            toVisit.Enqueue(new Node(current.distance, checkPositionDown));
                    }
                    else
                    {
                        //We try to spread towards the sides
                        foreach(Vector3Int adjacent in adjacents)
                        {
                            Vector3Int checkAdjacent = current.position + adjacent;

                            //If we have already visited this type, we ignore it
                            if(alreadyVisited.Contains(checkAdjacent))
                                continue;

                            World.TryGetTypeAt(checkAdjacent, out ushort actualAdjacent);

                            //If the adjacent one is not air or water, we ignore it
                            if(actualAdjacent != airIndex && actualAdjacent != fakewaterIndex)
                                continue;

                            Vector3Int checkAdjacentDown = checkAdjacent + Vector3Int.down;
                            World.TryGetTypeAt(checkAdjacentDown, out ushort actualAdjacentDown);

                            //If the below the adjacent one is air or water, we spread down
                            if(actualAdjacentDown == airIndex || actualAdjacentDown == fakewaterIndex)
                                toVisit.Enqueue(new Node(current.distance, checkAdjacentDown));
                            else
                                toVisit.Enqueue(new Node(current.distance + 1, checkAdjacent));
                        }
                    }
            }

            //Free memory
            toVisit.Clear();
            alreadyVisited.Clear();

            //Remove start
            orderedPositions[0].Remove(start);

            return orderedPositions;
        }

        public static List<Vector3Int> GetPositionsToSpreadWater(Vector3Int start, int distance)
        {
            List<Vector3Int> resultL = new List<Vector3Int>();

            Queue<Node> toVisit = new Queue<Node>();
            List<Vector3Int> alreadyVisited = new List<Vector3Int>();
            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("SimpleWater");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            toVisit.Enqueue(new Node(0, start));
            while(toVisit.Count > 0)
            {
                Node current = toVisit.Dequeue();

                if(alreadyVisited.Contains(current.position))
                    continue;

                alreadyVisited.Add(current.position);

                resultL.Add(current.position);

                //We do not look for adjacent ones if we are already at the maximum distance
                if(current.distance == distance)
                    continue;

                //If the lower block is air, it propagates downward
                Vector3Int checkPositionDown = current.position + Vector3Int.down;
                if(World.TryGetTypeAt(checkPositionDown, out ushort actualDown))
                    if(actualDown == airIndex || actualDown == fakewaterIndex || actualDown == waterIndex)
                    {
                        if(actualDown != waterIndex)
                            toVisit.Enqueue(new Node(current.distance, checkPositionDown));
                    }
                    else
                    {
                        //We try to spread towards the sides
                        foreach(Vector3Int adjacent in adjacents)
                        {
                            Vector3Int checkAdjacent = current.position + adjacent;

                            //If we have already visited this type, we ignore it
                            if(alreadyVisited.Contains(checkAdjacent))
                                continue;

                            World.TryGetTypeAt(checkAdjacent, out ushort actualAdjacent);

                            //If the adjacent one is not air or water, we ignore it
                            if(actualAdjacent != airIndex && actualAdjacent != fakewaterIndex)
                                continue;

                            Vector3Int checkAdjacentDown = checkAdjacent + Vector3Int.down;
                            World.TryGetTypeAt(checkAdjacentDown, out ushort actualAdjacentDown);

                            //If the below the adjacent one is air or water, we spread down
                            if(actualAdjacentDown == airIndex || actualAdjacentDown == fakewaterIndex)
                                toVisit.Enqueue(new Node(current.distance, checkAdjacentDown));
                            else
                                toVisit.Enqueue(new Node(current.distance + 1, checkAdjacent));
                        }
                    }
            }

            //Free memory
            toVisit.Clear();
            alreadyVisited.Clear();

            //Remove start
            resultL.Remove(start);

            return resultL;
        }

        public static List<Vector3Int> LookForWater(Vector3Int start, int distance)
        {
            List<Vector3Int> resultL = new List<Vector3Int>();

            Queue<Node> toVisit = new Queue<Node>();
            List<Vector3Int> alreadyVisited = new List<Vector3Int>();

            toVisit.Enqueue(new Node(0, start));
            while(toVisit.Count > 0)
            {
                Node current = toVisit.Dequeue();
                if(alreadyVisited.Contains(current.position))
                    continue;

                alreadyVisited.Add(current.position);

                //We do not look for adjacent ones if we are already at the maximum distance
                if(current.distance == distance)
                    continue;

                //Look Down
                Vector3Int checkPositionDown = current.position + Vector3Int.down;
                if(!alreadyVisited.Contains(checkPositionDown))
                {
                    if(World.TryGetTypeAt(checkPositionDown, out ushort actualDown))
                    {
                        if(actualDown == fakewaterIndex)
                        {
                            toVisit.Enqueue(new Node(current.distance, checkPositionDown));
                        }
                        else if(actualDown == waterIndex)
                        {
                            resultL.Add(checkPositionDown);
                        }
                    }
                }

                //Look Down
                Vector3Int checkPositionUp = current.position + Vector3Int.up;
                if(!alreadyVisited.Contains(checkPositionUp))
                {
                    if(World.TryGetTypeAt(checkPositionUp, out ushort actualUp))
                    {
                        if(actualUp == fakewaterIndex)
                        {
                            toVisit.Enqueue(new Node(current.distance, checkPositionUp));
                        }
                        else if(( actualUp == waterIndex ))
                        {
                            resultL.Add(checkPositionUp);
                        }
                    }
                }

                //We try to spread towards the sides
                foreach(Vector3Int adjacent in adjacents)
                {
                    Vector3Int checkAdjacent = current.position + adjacent;

                    //If we have already visited this type, we ignore it
                    if(!alreadyVisited.Contains(checkAdjacent))
                    {
                        if(World.TryGetTypeAt(checkAdjacent, out ushort actualAdjacent))
                        {
                            if(actualAdjacent == fakewaterIndex)
                            {
                                toVisit.Enqueue(new Node(current.distance + 1, checkAdjacent));
                            }
                            else if(actualAdjacent == waterIndex)
                            {
                                resultL.Add(checkAdjacent);
                                toVisit.Enqueue(new Node(current.distance + 1, checkAdjacent));
                            }

                            //Look for cascade down
                            Vector3Int checkAdjacentDown = checkAdjacent + Vector3Int.down;
                            if(!alreadyVisited.Contains(checkAdjacentDown))
                            {
                                if(World.TryGetTypeAt(checkAdjacentDown, out ushort actualAdjacentDown))
                                {
                                    if(actualAdjacentDown == fakewaterIndex)
                                    {
                                        toVisit.Enqueue(new Node(current.distance, checkAdjacentDown));
                                    }
                                    else if(actualAdjacentDown == waterIndex)
                                    {
                                        resultL.Add(checkAdjacentDown);
                                        toVisit.Enqueue(new Node(current.distance, checkAdjacentDown));
                                    }
                                }
                            }

                            //Look for cascade up
                            Vector3Int checkAdjacentUp = checkAdjacent + Vector3Int.up;
                            if(!alreadyVisited.Contains(checkAdjacentUp))
                            {
                                if(World.TryGetTypeAt(checkAdjacentUp, out ushort actualAdjacentUp))
                                {
                                    if(actualAdjacentUp == fakewaterIndex)
                                        toVisit.Enqueue(new Node(current.distance, checkAdjacentUp));
                                    else if(actualAdjacentUp == waterIndex)
                                    {
                                        resultL.Add(checkAdjacentUp);
                                        toVisit.Enqueue(new Node(current.distance, checkAdjacentUp));
                                    }
                                }
                            }
                        }   //TryGetTypeAt
                    }   //Already Visited
                }   //ForEach
            }   //While

            //Free memory
            toVisit.Clear();
            alreadyVisited.Clear();

            //Remove start
            resultL.Remove(start);

            return resultL;
        }

    }
}
