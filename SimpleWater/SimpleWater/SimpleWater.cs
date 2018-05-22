using ExtendedAPI.Types;
using Pipliz;
using Shared;
using System.Collections.Generic;

namespace SimpleWater
{
    public class Path
    {
        public int distance;
        public Vector3Int position;

        public Path(int distance, Vector3Int position)
        {
            this.distance = distance;
            this.position = position;
        }
    }

    [AutoLoadType]
    public class SimpleWater : BaseType
    {
        private int spreadDistance = 3;
        private int spreadSpeed = 4;
        private List<Vector3Int> adjacents = new List<Vector3Int>();

        public SimpleWater()
        {
            key = "SimpleWater";
            adjacents.Add(Vector3Int.left);
            adjacents.Add(Vector3Int.forward);
            adjacents.Add(Vector3Int.right);
            adjacents.Add(Vector3Int.back);
        }

        public override void OnRightClickOn(Players.Player player, Box<PlayerClickedData> boxedData)
        {
            if(null == player || null == boxedData)
                return;

            ItemTypes.ItemType item = ItemTypes.GetType(boxedData.item1.typeSelected);
            if(item.IsPlaceable && !item.NeedsBase)
            {
                ServerManager.TryChangeBlock(boxedData.item1.VoxelHit, boxedData.item1.typeSelected);
                ServerManager.TryChangeBlock(boxedData.item1.VoxelBuild, BlockTypes.Builtin.BuiltinBlocks.Air);
            }
        }

        public override void RegisterOnAdd(Vector3Int position, ushort newType, Players.Player causedBy)
        {
            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("SimpleWater");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            GetPositionsToSpreadWater(position, out List<Vector3Int>[] bloques);

            //Spread
            float time = spreadSpeed;
            for(int i = 0; i < bloques.Length; i++)
            {
                List<Vector3Int> positions = bloques[i];
                if(positions.Count != 0)
                    Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate ()
                    {
                        foreach(Vector3Int pos in positions)
                            ServerManager.TryChangeBlock(pos, fakewaterIndex);
                    }, time / 10);
                time += spreadSpeed;
            }
        }

        public override void RegisterOnRemove(Vector3Int position, ushort type, Players.Player causedBy)
        {
            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort waterIndex = ItemTypes.IndexLookup.GetIndex("SimpleWater");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            List<Vector3Int> notRemoveTypes = new List<Vector3Int>();

            //Look for Water sources near
            Vector3Int pos1 = new Vector3Int(position.x + ( spreadDistance * 3 ), position.y + ( spreadDistance * 3 ), position.z + ( spreadDistance * 3 ));
            Vector3Int pos2 = new Vector3Int(position.x - ( spreadDistance * 3 ), position.y - ( spreadDistance * 3 ), position.z - ( spreadDistance * 3 ));

            Vector3Int start = Vector3Int.Min(pos1, pos2);
            Vector3Int end = Vector3Int.Max(pos1, pos2);

            for(int x = start.x; x <= end.x; x++)
                for(int y = start.y; y <= end.y; y++)
                    for(int z = start.z; z <= end.z; z++)
                    {
                        Vector3Int newPos = new Vector3Int(x, y, z);
                        if(World.TryGetTypeAt(newPos, out ushort posType) && waterIndex == posType)
                            notRemoveTypes.AddRange(GetPositionsNOTRemove(newPos));
                    }

            //Fake water blocks generate by this block of water source
            GetPositionsToSpreadWater(position, out List<Vector3Int>[] bloques);

            float time = spreadSpeed;
            for(int i = 0; i < bloques.Length; i++)
            {
                List<Vector3Int> positions = bloques[i];
                if(positions.Count != 0)
                    Pipliz.Threading.ThreadManager.InvokeOnMainThread(delegate ()
                    {
                        foreach(Vector3Int pos in positions)
                            if(!notRemoveTypes.Contains(pos))
                                if(World.TryGetTypeAt(pos, out ushort posType) && fakewaterIndex == posType)
                                    ServerManager.TryChangeBlock(pos, airIndex);
                    }, time / 10);
                time += spreadSpeed;
            }
        }


        public void GetPositionsToSpreadWater(Vector3Int start, out List<Vector3Int>[] resultado)
        {
            resultado = new List<Vector3Int>[spreadDistance + 1];

            Queue<Path> porVisitar = new Queue<Path>();
            List<Vector3Int> yaVisitados = new List<Vector3Int>();
            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            porVisitar.Enqueue(new Path(0, start));
            while(porVisitar.Count > 0)
            {
                Path actual = porVisitar.Dequeue();
                if(resultado[actual.distance] != null)
                    resultado[actual.distance].Add(actual.position);
                else
                {
                    List<Vector3Int> lista = new List<Vector3Int>();
                    lista.Add(actual.position);
                    resultado[actual.distance] = lista;
                }

                yaVisitados.Add(actual.position);

                //No buscamos adyacentes si ya estamos a la distancia maxima
                if(actual.distance == spreadDistance)
                    continue;

                //Si el bloque inferior es aire, se propaga hacia abajo
                Vector3Int checkPositionDown = actual.position + Vector3Int.down;
                if(World.TryGetTypeAt(checkPositionDown, out ushort actualDown) && ( actualDown == airIndex || actualDown == fakewaterIndex ))
                    porVisitar.Enqueue(new Path(actual.distance, checkPositionDown));
                else
                {
                    //Intentamos propagar hacia los lados
                    foreach(Vector3Int adjacent in adjacents)
                    {
                        Vector3Int checkAdjacent = actual.position + adjacent;

                        //Si ya hemos visitado este bloque, lo ignoramos
                        if(yaVisitados.Contains(checkAdjacent))
                            continue;

                        World.TryGetTypeAt(checkAdjacent, out ushort actualAdjacent);

                        //Si el adyacente no es aire lo ignoramos
                        if(actualAdjacent != airIndex && actualAdjacent != fakewaterIndex)
                            continue;

                        Vector3Int checkAdjacentDown = checkAdjacent + Vector3Int.down;
                        World.TryGetTypeAt(checkAdjacentDown, out ushort actualAdjacentDown);

                        //Si el debajo del adyacente es aire, propagamos hacia abajo
                        if(actualAdjacentDown == airIndex || actualAdjacentDown == fakewaterIndex)
                            porVisitar.Enqueue(new Path(actual.distance, checkAdjacentDown));
                        else
                            porVisitar.Enqueue(new Path(actual.distance + 1, checkAdjacent));
                    }
                }
            }

            //Eliminamos el 1
            resultado[0].Remove(start);
        }

        public List<Vector3Int> GetPositionsNOTRemove(Vector3Int start)
        {
            Queue<Path> porVisitar = new Queue<Path>();
            List<Vector3Int> yaVisitados = new List<Vector3Int>();
            List<Vector3Int> resultado = new List<Vector3Int>();
            ushort airIndex = ItemTypes.IndexLookup.GetIndex("air");
            ushort fakewaterIndex = ItemTypes.IndexLookup.GetIndex("Fake.SimpleWater");

            List<Vector3Int> adjacents = new List<Vector3Int>();
            adjacents.Add(Vector3Int.left);
            adjacents.Add(Vector3Int.forward);
            adjacents.Add(Vector3Int.right);
            adjacents.Add(Vector3Int.back);

            porVisitar.Enqueue(new Path(0, start));
            while(porVisitar.Count > 0)
            {
                Path actual = porVisitar.Dequeue();
                resultado.Add(actual.position);
                yaVisitados.Add(actual.position);

                //No buscamos adyacentes si ya estamos a la distancia maxima
                if(actual.distance == spreadDistance)
                    continue;

                //Si el bloque inferior es aire, se propaga hacia abajo
                Vector3Int checkPositionDown = actual.position + Vector3Int.down;
                if(World.TryGetTypeAt(checkPositionDown, out ushort actualDown) && ( actualDown == airIndex || actualDown == fakewaterIndex ))
                    porVisitar.Enqueue(new Path(actual.distance, checkPositionDown));
                else
                {
                    //Intentamos propagar hacia los lados
                    foreach(Vector3Int adjacent in adjacents)
                    {
                        Vector3Int checkAdjacent = actual.position + adjacent;

                        //Si ya hemos visitado este bloque, lo ignoramos
                        if(yaVisitados.Contains(checkAdjacent))
                            continue;

                        World.TryGetTypeAt(checkAdjacent, out ushort actualAdjacent);

                        //Si el adyacente no es aire lo ignoramos
                        if(actualAdjacent != airIndex && actualAdjacent != fakewaterIndex)
                            continue;

                        Vector3Int checkAdjacentDown = checkAdjacent + Vector3Int.down;
                        World.TryGetTypeAt(checkAdjacentDown, out ushort actualAdjacentDown);

                        //Si el debajo del adyacente es aire, propagamos hacia abajo
                        if(actualAdjacentDown == airIndex || actualAdjacentDown == fakewaterIndex)
                            porVisitar.Enqueue(new Path(actual.distance, checkAdjacentDown));
                        else
                            porVisitar.Enqueue(new Path(actual.distance + 1, checkAdjacent));
                    }
                }
            }

            //Eliminamos el 1
            resultado.Remove(start);

            return resultado;
        }
    }



}
