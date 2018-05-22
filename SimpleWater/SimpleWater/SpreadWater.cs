using Pipliz;
using System.Collections.Generic;

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
        public static int spreadDistance { get; } = 3;

        private static Vector3Int[] adjacents = { Vector3Int.left, Vector3Int.forward, Vector3Int.right, Vector3Int.back };

        public static void GetPositionsToSpreadWater(Vector3Int start, out List<Vector3Int>[] result)
        {
            result = new List<Vector3Int>[spreadDistance + 1];

            Queue<Node> toVisit = new Queue<Node>();
            List<Vector3Int> alreadyVisited = new List<Vector3Int>();
            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            toVisit.Enqueue(new Node(0, start));
            while(toVisit.Count > 0)
            {
                Node current = toVisit.Dequeue();
                if(result[current.distance] != null)
                    result[current.distance].Add(current.position);
                else
                {
                    List<Vector3Int> lista = new List<Vector3Int>();
                    lista.Add(current.position);
                    result[current.distance] = lista;
                }

                alreadyVisited.Add(current.position);

                //We do not look for adjacent ones if we are already at the maximum distance
                if(current.distance == spreadDistance)
                    continue;

                //If the lower block is air, it propagates downward
                Vector3Int checkPositionDown = current.position + Vector3Int.down;
                if(World.TryGetTypeAt(checkPositionDown, out ushort actualDown) && ( actualDown == airIndex || actualDown == fakewaterIndex ))
                    toVisit.Enqueue(new Node(current.distance, checkPositionDown));
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

            //Remove start
            result[0].Remove(start);
        }

        public static List<Vector3Int> GetPositionsNOTRemove(Vector3Int start)
        {
            Queue<Node> toVisit = new Queue<Node>();
            List<Vector3Int> alreadyVisited = new List<Vector3Int>();
            List<Vector3Int> result = new List<Vector3Int>();
            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            List<Vector3Int> adjacents = new List<Vector3Int>();
            adjacents.Add(Vector3Int.left);
            adjacents.Add(Vector3Int.forward);
            adjacents.Add(Vector3Int.right);
            adjacents.Add(Vector3Int.back);

            toVisit.Enqueue(new Node(0, start));
            while(toVisit.Count > 0)
            {
                Node actual = toVisit.Dequeue();
                result.Add(actual.position);
                alreadyVisited.Add(actual.position);

                //We do not look for adjacent ones if we are already at the maximum distance
                if(actual.distance == spreadDistance)
                    continue;

                //If the lower block is air, it propagates downward
                Vector3Int checkPositionDown = actual.position + Vector3Int.down;
                if(World.TryGetTypeAt(checkPositionDown, out ushort actualDown) && ( actualDown == airIndex || actualDown == fakewaterIndex ))
                    toVisit.Enqueue(new Node(actual.distance, checkPositionDown));
                else
                {
                    //We try to spread towards the sides
                    foreach(Vector3Int adjacent in adjacents)
                    {
                        Vector3Int checkAdjacent = actual.position + adjacent;

                        //If we have already visited this type, we ignore it
                        if(alreadyVisited.Contains(checkAdjacent))
                            continue;

                        World.TryGetTypeAt(checkAdjacent, out ushort actualAdjacent);

                        //Si el adyacente no es aire lo ignoramos
                        if(actualAdjacent != airIndex && actualAdjacent != fakewaterIndex)
                            continue;

                        Vector3Int checkAdjacentDown = checkAdjacent + Vector3Int.down;
                        World.TryGetTypeAt(checkAdjacentDown, out ushort actualAdjacentDown);

                        //If the below the adjacent one is air or water, we spread down
                        if(actualAdjacentDown == airIndex || actualAdjacentDown == fakewaterIndex)
                            toVisit.Enqueue(new Node(actual.distance, checkAdjacentDown));
                        else
                            toVisit.Enqueue(new Node(actual.distance + 1, checkAdjacent));
                    }
                }
            }

            //Remove start
            result.Remove(start);

            return result;
        }

    }
}
