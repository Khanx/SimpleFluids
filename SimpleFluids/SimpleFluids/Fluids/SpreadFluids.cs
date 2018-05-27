using System.Collections.Generic;
using BlockTypes.Builtin;
using Pipliz;


namespace SimpleFluids
{
    public struct Node
    {
        public int distance;
        public Vector3Int position;
        public int gravity;

        public Node(int distance, Vector3Int position, int gravity = 0)
        {
            this.distance = distance;
            this.position = position;
            this.gravity = gravity;
        }
    }

    [ModLoader.ModManager]
    public static class SpreadFluids
    {
        public static string MODPATH;

        private static Vector3Int[] adjacents = { Vector3Int.left, Vector3Int.forward, Vector3Int.right, Vector3Int.back };

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, "Khanx.SimpleFluids.GetModPath")]
        public static void GetModPath(string path)
        {
            MODPATH = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        }

        public static List<Vector3Int>[] GetOrderedPositionsToSpread(Vector3Int start, int distance, ushort fluid, ushort fakeFluid)
        {
            List<Node> unorderedPositions = new List<Node>();
            int maxDistance = distance + 1;

            Queue<Node> toVisit = new Queue<Node>();
            List<Vector3Int> alreadyVisited = new List<Vector3Int>();

            toVisit.Enqueue(new Node(0, start, 0));
            while(toVisit.Count > 0)
            {
                Node current = toVisit.Dequeue();

                if(alreadyVisited.Contains(current.position))
                    continue;

                alreadyVisited.Add(current.position);
                unorderedPositions.Add(current);
                if(maxDistance < current.distance + current.gravity)
                    maxDistance = current.distance + current.gravity;

                //We do not look for adjacent ones if we are already at the maximum distance
                if(current.distance == distance)
                    continue;

                //If the lower block is air, it propagates downward
                Vector3Int checkPositionDown = current.position + Vector3Int.down;
                if(World.TryGetTypeAt(checkPositionDown, out ushort actualDown))
                    if(actualDown == BuiltinBlocks.Air || actualDown == fakeFluid || actualDown == fluid)
                    {
                        if(actualDown != fluid)
                            toVisit.Enqueue(new Node(current.distance, checkPositionDown, current.gravity + 1));
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
                            if(actualAdjacent != BuiltinBlocks.Air && actualAdjacent != fakeFluid)
                                continue;

                            Vector3Int checkAdjacentDown = checkAdjacent + Vector3Int.down;
                            World.TryGetTypeAt(checkAdjacentDown, out ushort actualAdjacentDown);

                            //If the below the adjacent one is air or water, we spread down
                            if(actualAdjacentDown == BuiltinBlocks.Air || actualAdjacentDown == fakeFluid)
                                toVisit.Enqueue(new Node(current.distance, checkAdjacentDown, current.gravity + 1));
                            else
                                toVisit.Enqueue(new Node(current.distance + 1, checkAdjacent, current.gravity));
                        }
                    }
            }

            List<Vector3Int>[] orderedPositions = new List<Vector3Int>[maxDistance + 1];

            foreach(Node e in unorderedPositions)
            {
                int realdistance = e.distance + e.gravity;
                if(orderedPositions[realdistance] == null)
                {
                    orderedPositions[realdistance] = new List<Vector3Int>();
                    orderedPositions[realdistance].Add(e.position);
                }
                else
                    orderedPositions[realdistance].Add(e.position);
            }

            //Free memory
            toVisit.Clear();
            alreadyVisited.Clear();
            unorderedPositions.Clear();

            //Remove start
            orderedPositions[0].Remove(start);

            return orderedPositions;
        }

        public static List<Vector3Int> GetUnorderedPositionsToSpread(Vector3Int start, int distance, ushort fluid, ushort fakeFluid)
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

                resultL.Add(current.position);

                //We do not look for adjacent ones if we are already at the maximum distance
                if(current.distance == distance)
                    continue;

                //If the lower block is air, it propagates downward
                Vector3Int checkPositionDown = current.position + Vector3Int.down;
                if(World.TryGetTypeAt(checkPositionDown, out ushort actualDown))
                    if(actualDown == BuiltinBlocks.Air || actualDown == fakeFluid || actualDown == fluid)
                    {
                        if(actualDown != fluid)
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
                            if(actualAdjacent != BuiltinBlocks.Air && actualAdjacent != fakeFluid)
                                continue;

                            Vector3Int checkAdjacentDown = checkAdjacent + Vector3Int.down;
                            World.TryGetTypeAt(checkAdjacentDown, out ushort actualAdjacentDown);

                            //If the below the adjacent one is air or water, we spread down
                            if(actualAdjacentDown == BuiltinBlocks.Air || actualAdjacentDown == fakeFluid)
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

        public static List<Vector3Int> LookForSources(Vector3Int start, int distance, ushort fluid, ushort fakeFluid)
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
                        if(actualDown == fakeFluid)
                        {
                            toVisit.Enqueue(new Node(current.distance, checkPositionDown));
                        }
                        else if(actualDown == fluid)
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
                        if(actualUp == fakeFluid)
                        {
                            toVisit.Enqueue(new Node(current.distance, checkPositionUp));
                        }
                        else if(( actualUp == fluid ))
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
                            if(actualAdjacent == fakeFluid)
                            {
                                toVisit.Enqueue(new Node(current.distance + 1, checkAdjacent));
                            }
                            else if(actualAdjacent == fluid)
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
                                    if(actualAdjacentDown == fakeFluid)
                                    {
                                        toVisit.Enqueue(new Node(current.distance, checkAdjacentDown));
                                    }
                                    else if(actualAdjacentDown == fluid)
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
                                    if(actualAdjacentUp == fakeFluid)
                                        toVisit.Enqueue(new Node(current.distance, checkAdjacentUp));
                                    else if(actualAdjacentUp == fluid)
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
