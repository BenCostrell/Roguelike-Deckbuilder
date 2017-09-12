using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunayTriangulator;
using System.Linq;

public class MapManager : MonoBehaviour {

    [SerializeField]
    private int baseLevelLength;
    [HideInInspector]
    public int levelLength, levelHeight;
    [SerializeField]
    private int minRoomDimension, maxRoomDimension;
    [SerializeField]
    private int hallwayWidth;
    [SerializeField]
    private int roomGenRectDimension;
    [SerializeField]
    private int startingRoomWidth, startingRoomHeight;
    [SerializeField]
    private int minRoomDist, maxRoomDist;
    [SerializeField]
    private int numRoomsToGen;
    [SerializeField]
    private float proportionOfEdgesToReAdd;
    [SerializeField]
    private int rowsPerCard;
    public Tile[,] map { get; private set; }
    public Dictionary<Coord, Tile> mapDict {get; private set;}
    [SerializeField]
    private int maxTriesProcGen;
    //public List<Card> cardsOnBoard;
    public List<Chest> chestsOnBoard;
    [SerializeField]
    private Sprite roomSprite;
    [SerializeField]
    private Sprite botLeftCornerSprite, botRightCornerSprite;
    [SerializeField]
    private Sprite botSprite, leftSprite, rightSprite;
    [SerializeField]
    private Sprite[] centerSprites;
    [SerializeField]
    private Sprite exitDoorSprite;
    [SerializeField]
    private float extraLevelLengthPerLevel;
    [SerializeField]
    private int levelsPerCardTierIncrease;
    [SerializeField]
    private float chestsPerRoom;
    private List<Room> rooms;
    private List<Room> roomsWithoutChests;
    public Tile playerSpawnTile { get; private set; }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void GenerateLevel(int levelNum)
    {
        int extraLevelLength = Mathf.RoundToInt((levelNum - 1) * extraLevelLengthPerLevel);
        levelLength = baseLevelLength + extraLevelLength;
        map = new Tile[levelLength, levelHeight];
        for (int x = 0; x < levelLength; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                if(x == 0 && y == levelHeight - 1)
                {
                    map[x, y] = new Tile(new Coord(x, y), true, true);
                }
                else map[x, y] = new Tile(new Coord(x, y), false, true);
            }
        }
        PlaceKey(map[levelLength - 1, levelHeight - 1]);
        FindAllNeighbors();
        SetSprites();
        int chestsToGenerate = levelLength / rowsPerCard;
        //GenerateChests(levelNum, chestsToGenerate);
    }

    public void MakeTestTiles(int levelNum)
    {
        rooms = GenerateInitialRooms(numRoomsToGen);
        List<Tile> allTiles = new List<Tile>();
        for (int i = 0; i < rooms.Count; i++)
        {
            allTiles.AddRange(GenerateRoomTiles(rooms[i]));
        }
        List<Edge> delaunayTriangulation = DelaunayTriangulationOfRooms(rooms);
        delaunayTriangulation = RemoveEdgeDuplicates(delaunayTriangulation);
        List<Edge> orderedEdges =
            new List<Edge>(delaunayTriangulation.OrderBy(edge => edge.Length));
        List<Edge> minSpanningTree = GetMinimumSpanningTree(delaunayTriangulation);
        List<Edge> edges = ReAddEdgesToSpanningTree(minSpanningTree, orderedEdges);

        foreach(Room room in rooms)
        {
            List<Edge> neighborEdges = AdjacentEdgesToVertex(room, edges);
            List<Tuple<Room, float>> neighbors = new List<Tuple<Room, float>>();
            foreach(Edge edge in neighborEdges)
            {
                if (edge.a != room) neighbors.Add(new Tuple<Room, float>(edge.a, edge.Length));
                else neighbors.Add(new Tuple<Room, float>(edge.b, edge.Length));
            }
            room.neighbors = neighbors;
        }

        List<Coord> hallwayCoords = new List<Coord>();
        for (int i = 0; i < edges.Count; i++)
        {
            hallwayCoords.AddRange(GenerateHallway(edges[i]));
        }
        hallwayCoords = RemoveCoordDuplicates(hallwayCoords);
        allTiles.AddRange(GenerateHallwayTiles(hallwayCoords));
        mapDict = new Dictionary<Coord, Tile>();
        foreach(Tile tile in allTiles)
        {
            mapDict.Add(tile.coord, tile);
        }
        foreach(Tile tile in allTiles)
        {
            FindNeighbors(tile);
        }

        Room startRoom, keyRoom;
        startRoom = GetFarthestRoom(rooms[0], edges, rooms);
        playerSpawnTile = GetFarthestTileFromHallwayEntrances(startRoom);
        foreach (Tile tile in startRoom.tiles)
        {
            tile.controller.GetComponent<SpriteRenderer>().color = Color.blue;

        }
        keyRoom = GetFarthestRoom(startRoom, minSpanningTree, rooms);
        PlaceKey(GetFarthestTileFromHallwayEntrances(keyRoom));
        foreach (Tile tile in keyRoom.tiles)
        {
            tile.controller.GetComponent<SpriteRenderer>().color = Color.red;
        }

        GenerateChests(levelNum);
    }

    Tile GetFarthestTileFromHallwayEntrances(Room room)
    {
        float maxMinDist = 0;
        Tile furthestTileFromEntrances = null;
        foreach (Tile tile in room.tiles)
        {
            float minDist = Mathf.Infinity;

            foreach (Coord hallwayEntrance in room.hallwayEntrances)
            {
                if (hallwayEntrance.Distance(tile.coord) < minDist)
                {
                    minDist = hallwayEntrance.Distance(tile.coord);
                }
            }
            if (minDist > maxMinDist)
            {
                maxMinDist = minDist;
                furthestTileFromEntrances = tile;
            }
        }
        return furthestTileFromEntrances;
    }

    Room GetFarthestRoom(Room room, List<Edge> graph, List<Room> otherRooms)
    {
        Room farthestRoom = room;
        float longestDistance = 0;
        foreach (Room otherRoom in otherRooms)
        {
            float distance = AStarSearch.ShortestPathDistance(room, otherRoom);
            if(distance > longestDistance)
            {
                farthestRoom = otherRoom;
                longestDistance = distance;
            }  
        }
        return farthestRoom;
    }

    List<Edge> RemoveEdgeDuplicates(List<Edge> edges)
    {
        List<Edge> filteredList = new List<Edge>();
        foreach(Edge edge in edges)
        {
            if (!DoesListContainEquivalentEdge(filteredList, edge))
                filteredList.Add(edge);
        }
        return filteredList;
    }

    bool DoesListContainEquivalentEdge(List<Edge> edges, Edge edgeToCompare)
    {
        for (int i = 0; i < edges.Count; i++)
        {
            if (edges[i].a == edgeToCompare.a && edges[i].b == edgeToCompare.b ||
                edges[i].a == edgeToCompare.b && edges[i].b == edgeToCompare.a)
                return true;
        }
        return false;
    }

    List<Coord> RemoveCoordDuplicates(List<Coord> coordList)
    {
        List<Coord> filteredList = new List<Coord>();
        List<Coord> roomCoords = new List<Coord>();
        foreach(Room room in rooms)
        {
            foreach(Tile tile in room.tiles)
            {
                roomCoords.Add(tile.coord);
            }
        }
        foreach(Coord coord in coordList)
        {
            if (!filteredList.Contains(coord) && !roomCoords.Contains(coord))
                filteredList.Add(coord);
        }
        return filteredList;
    }

    List<Edge> DelaunayTriangulationOfRooms(List<Room> rooms)
    {
        List<Vertex> vertices = new List<Vertex>();
        foreach (Room room in rooms)
        {
            vertices.Add(new Vertex(room.Center.x, room.Center.y));
        }
        Triangulator triangulator = new Triangulator();
        List<Triad> triangles = triangulator.Triangulation(vertices);
        List<Edge> edges = new List<Edge>();
        foreach (Triad triangle in triangles)
        {
            edges.Add(new Edge(rooms[triangle.a], rooms[triangle.b]));
            edges.Add(new Edge(rooms[triangle.b], rooms[triangle.c]));
            edges.Add(new Edge(rooms[triangle.a], rooms[triangle.c]));
        }
        foreach (Edge edge in edges)
        {
            Debug.DrawLine(edge.pointA + (Vector2.one * 0.2f),
                edge.pointB + (Vector2.one * 0.2f), Color.blue, 60f);
        }
        return edges;
    }

    List<Edge> GetMinimumSpanningTree(List<Edge> edges)
    {
        List<Edge> orderedEdges = new List<Edge>(edges.OrderBy(edge => edge.Length));
        List<Edge> spanningTree = new List<Edge>();
        int edgeIndex = 0;
        while (spanningTree.Count != numRoomsToGen - 1)
        {
            List<Edge> potentialTree = new List<Edge>(spanningTree);
            Edge edge = orderedEdges[edgeIndex];
            edgeIndex += 1;
            potentialTree.Add(edge);
            if (!CheckForCycles(potentialTree))
            {
                spanningTree.Add(edge);
            }
        }
        return spanningTree;
    }

    List<Edge> ReAddEdgesToSpanningTree(List<Edge> spanningTree, List<Edge> orderedEdges) {
        List<Edge> alteredSpanningTree = new List<Edge>(spanningTree);
        int edgesToReAdd = Mathf.RoundToInt(
            (orderedEdges.Count - alteredSpanningTree.Count) * proportionOfEdgesToReAdd);
        Debug.Log("delaunay triangulation had " + orderedEdges.Count + " edges");
        Debug.Log("spanning tree had " + alteredSpanningTree.Count + " edges");
        Debug.Log("added back in " + edgesToReAdd + " edges");
        int edgesReAddedSoFar = 0;
        if (edgesToReAdd > 0)
        {
            for (int i = 0; i < orderedEdges.Count; i++)
            {
                if (!alteredSpanningTree.Contains(orderedEdges[i]))
                {
                    alteredSpanningTree.Add(orderedEdges[i]);
                    edgesReAddedSoFar += 1;
                    if (edgesReAddedSoFar == edgesToReAdd) break;
                }
            }
        }
        foreach (Edge edge in alteredSpanningTree)
        {
            Debug.DrawLine(edge.pointA, edge.pointB, Color.red, 60f);
        }
        return alteredSpanningTree;
    }

    bool CheckForCycles(List<Edge> graph)
    {
        List<Edge> exploredEdges = new List<Edge>();
        for (int i = 0; i < graph.Count; i++)
        {
            if (!exploredEdges.Contains(graph[i]))
            {
                List<Room> visitedRooms = new List<Room>();
                Stack<Room> roomsToExplore = new Stack<Room>();
                roomsToExplore.Push(graph[i].a);
                while (roomsToExplore.Count > 0)
                {
                    Room currentRoom = roomsToExplore.Pop();
                    if (visitedRooms.Contains(currentRoom)) return true;
                    else
                    {
                        visitedRooms.Add(currentRoom);
                        List<Edge> adjEdges = AdjacentEdgesToVertex(currentRoom, graph);
                        for (int j = 0; j < adjEdges.Count; j++)
                        {
                            if (!exploredEdges.Contains(adjEdges[j]))
                            {
                                Room adjRoom;
                                if (adjEdges[j].a != currentRoom) adjRoom = adjEdges[j].a;
                                else adjRoom = adjEdges[j].b;
                                roomsToExplore.Push(adjRoom);
                                exploredEdges.Add(adjEdges[j]);
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    List<Edge> AdjacentEdgesToVertex(Room room, List<Edge> graph)
    {
        List<Edge> adjEdges = new List<Edge>();
        foreach(Edge edge in graph)
        {
            if (edge.a == room || edge.b == room) adjEdges.Add(edge);
        }
        return adjEdges;
    }

    List<Tile> GenerateRoomTiles(Room room)
    {
        List<Tile> roomTiles = new List<Tile>();
        for (int i = 0; i < room.dimensions.x - 1; i++)
        {
            for (int j = 0; j < room.dimensions.y -1; j++)
            {
                Tile tile =
                    new Tile(new Coord(room.origin.x + i, room.origin.y + j), false, true);
                tile.SetSprite(roomSprite, Quaternion.identity);
                room.tiles.Add(tile);
                roomTiles.Add(tile);
            }
        }
        return roomTiles;
    }

    List<Tile> GenerateHallwayTiles(List<Coord> hallwayCoords)
    {
        List<Tile> hallwayTiles = new List<Tile>();
        for (int i = 0; i < hallwayCoords.Count; i++)
        {
            hallwayTiles.Add(new Tile(hallwayCoords[i], false, false));
        }
        return hallwayTiles;
    }

    List<Coord> GenerateHallway(Edge edge)
    {
        List<Coord> hallwayCoords = new List<Coord>();
        Vector2 midpointVector2 = (edge.a.Center + edge.b.Center) / 2;
        Coord midpointCoord = new Coord(
            Mathf.RoundToInt(midpointVector2.x), 
            Mathf.RoundToInt(midpointVector2.y));
        if ((midpointCoord.y >= edge.a.Bottom && midpointCoord.y <= edge.a.Top) &&
            (midpointCoord.y >= edge.b.Bottom && midpointCoord.y <= edge.b.Top))
        {
            int startX = Mathf.Min(edge.a.Right, edge.b.Right);
            int endX = Mathf.Max(edge.a.Left, edge.b.Left);
            for (int i = startX; i < endX; i++)
            {
                for (int j = -hallwayWidth / 2; j < (hallwayWidth - hallwayWidth / 2); j++)
                {
                    hallwayCoords.Add(new Coord(i, midpointCoord.y + j));
                }
            }
        }
        else if ((midpointCoord.x >= edge.a.Left && midpointCoord.x <= edge.a.Right) &&
            (midpointCoord.x >= edge.b.Left && midpointCoord.x <= edge.b.Right))
        {
            int startY = Mathf.Min(edge.a.Top, edge.b.Top);
            int endY = Mathf.Max(edge.a.Bottom, edge.b.Bottom);
            for (int i = startY; i < endY; i++)
            {
                for (int j = -hallwayWidth / 2; j < (hallwayWidth - hallwayWidth / 2); j++)
                {
                    hallwayCoords.Add(new Coord(midpointCoord.x + j, i));
                }
            }
        }
        else
        {
            int startX = Mathf.Min(edge.a.Right, edge.b.CenterCoord.x);
            int endX = Mathf.Max(edge.a.Left, edge.b.CenterCoord.x);
            for (int i = startX; i < endX; i++)
            {
                for (int j = -hallwayWidth / 2; j < (hallwayWidth - hallwayWidth / 2); j++)
                {
                    hallwayCoords.Add(new Coord(i, edge.a.CenterCoord.y + j));
                }
            }
            int startY = Mathf.Min(
                edge.b.Top, 
                edge.a.CenterCoord.y - hallwayWidth / 2);
            int endY = Mathf.Max(
                edge.b.Bottom, 
                edge.a.CenterCoord.y + (hallwayWidth - hallwayWidth / 2));
            for (int i = startY; i < endY; i++)
            {
                for (int j = -hallwayWidth / 2; j < (hallwayWidth - hallwayWidth / 2); j++)
                {
                    hallwayCoords.Add(new Coord(edge.b.CenterCoord.x + j, i));
                }
            }
        }
        Coord closestToRoomA = new Coord(0, 0);
        Coord closestToRoomB = new Coord(0, 0);
        float shortestDistToRoomA = Mathf.Infinity;
        float shortestDistToRoomB = Mathf.Infinity;
        foreach(Coord coord in hallwayCoords)
        {
            if(coord.Distance(edge.a.CenterCoord) < shortestDistToRoomA)
            {
                closestToRoomA = coord;
                shortestDistToRoomA = coord.Distance(edge.a.CenterCoord);
            }
            if (coord.Distance(edge.b.CenterCoord) < shortestDistToRoomB)
            {
                closestToRoomB = coord;
                shortestDistToRoomB = coord.Distance(edge.b.CenterCoord);
            }
        }
        if (!edge.a.hallwayEntrances.Contains(closestToRoomA))
        {
            edge.a.hallwayEntrances.Add(closestToRoomA);
        }
        if (!edge.b.hallwayEntrances.Contains(closestToRoomB))
        {
            edge.b.hallwayEntrances.Add(closestToRoomB);
        }
        return hallwayCoords;
    }

    List<Room> GenerateInitialRooms(int numRooms)
    {
        List<Room> rooms = new List<Room>();
        for (int i = 0; i < numRooms; i++)
        {
            Coord coord = 
                GenerateRandomCoord(0, roomGenRectDimension, 0, roomGenRectDimension);
            IntVector2 dimensions = GenerateRoomDimensions();
            Room room = GenerateValidRoom(rooms);
            if (room == null)
            {
                Debug.Log("stopping early after " + i + " rooms");
                break;
            }
            rooms.Add(room);
        }
        return rooms;
    }

    bool IsRoomValid(Room room, List<Room> otherRooms)
    {
        for (int i = 0; i < otherRooms.Count; i++)
        {
            if (room.Distance(otherRooms[i]) < minRoomDist)
            {
                //Debug.Log("too close, distance of " + room.Distance(otherRooms[i]));
                return false;
            }
        }
        return true;
    }

    Room GenerateValidRoom(List<Room> otherRooms)
    {
        Room room;
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            IntVector2 dimensions = GenerateRoomDimensions();
            Coord coord = new Coord(0, 0);
            if (otherRooms.Count > 0)
            {
                Room referenceRoom = 
                    otherRooms[Random.Range(0, otherRooms.Count)];
                int distanceX = Random.Range(
                    minRoomDist + minRoomDimension / 2,
                    maxRoomDist / 2 + maxRoomDimension / 2);
                int distanceY = Random.Range(
                    minRoomDist + minRoomDimension / 2,
                    maxRoomDist / 2 + maxRoomDimension / 2);
                if (Random.Range(0, 2) == 0) distanceX *= -1;
                if (Random.Range(0, 2) == 0) distanceY *= -1;
                coord = new Coord(
                    referenceRoom.origin.x + (referenceRoom.dimensions.x / 2) + distanceX,
                    referenceRoom.origin.y + (referenceRoom.dimensions.y / 2) + distanceY);
            }
            
            room = new Room(coord, dimensions);
            //Debug.Log("trying room of size " + room.dimensions.x + "," + room.dimensions.y +
            //    " at " + room.origin.x + "," + room.origin.y);
            if (IsRoomValid(room, otherRooms)) return room;
        }
        return null;
    }

    Coord GenerateRandomCoord(int xMin, int xMax, int yMin, int yMax)
    {
        int x = Random.Range(xMin, xMax + 1);
        int y = Random.Range(yMin, yMax + 1);
        return new Coord(x, y);
    }

    IntVector2 GenerateRoomDimensions()
    {
        int x = Random.Range(minRoomDimension, maxRoomDimension + 1);
        int y = Random.Range(minRoomDimension, maxRoomDimension + 1);
        return new IntVector2(x, y);
    }

    void PlaceKey(Tile tile)
    {
        tile.containedKey = 
            Instantiate(Services.Prefabs.Key, Services.Main.transform)
            .GetComponent<DoorKey>();
        tile.containedKey.transform.position = tile.controller.transform.position;
    }

    void SetSprites()
    {
        foreach(Tile tile in map) SetSprite(tile);
    }

    void SetSprite(Tile tile)
    {
        Sprite sprite;
        Quaternion rot = Quaternion.identity;

        if (tile.isExit)
        {
            sprite = exitDoorSprite;
        }
        else
        {
            bool hasUpNeighbor = false;
            bool hasDownNeighbor = false;
            bool hasLeftNeighbor = false;
            bool hasRightNeighbor = false;
            foreach (Tile neighbor in tile.neighbors)
            {
                Coord diff = neighbor.coord.Subtract(tile.coord);
                if (diff == new Coord(0, 1))
                {
                    hasUpNeighbor = true;
                }
                if (diff == new Coord(0, -1))
                {
                    hasDownNeighbor = true;
                }
                if (diff == new Coord(1, 0))
                {
                    hasRightNeighbor = true;
                }
                if (diff == new Coord(-1, 0))
                {
                    hasLeftNeighbor = true;
                }
            }
            if (hasUpNeighbor && hasDownNeighbor && hasLeftNeighbor && hasRightNeighbor)
                sprite = centerSprites[Random.Range(0, centerSprites.Length)];
            else if (hasUpNeighbor && hasDownNeighbor && hasRightNeighbor) sprite = leftSprite;
            else if (hasUpNeighbor && hasDownNeighbor && hasLeftNeighbor) sprite = rightSprite;
            else if (hasUpNeighbor && hasRightNeighbor && hasLeftNeighbor) sprite = botSprite;
            else if (hasDownNeighbor && hasRightNeighbor && hasLeftNeighbor)
            {
                sprite = botSprite;
                rot = Quaternion.Euler(new Vector3(0, 0, 180));
            }
            else if (hasUpNeighbor && hasLeftNeighbor) sprite = botRightCornerSprite;
            else if (hasUpNeighbor && hasRightNeighbor) sprite = botLeftCornerSprite;
            else if (hasDownNeighbor && hasLeftNeighbor)
            {
                sprite = botLeftCornerSprite;
                rot = Quaternion.Euler(new Vector3(0, 0, 180));
            }
            else if (hasDownNeighbor && hasRightNeighbor)
            {
                sprite = botRightCornerSprite;
                rot = Quaternion.Euler(new Vector3(0, 0, 180));
            }
            else sprite = centerSprites[Random.Range(0, centerSprites.Length)];
        }
        tile.SetSprite(sprite, rot);
    }

    void FindAllNeighbors()
    {
        for (int x = 0; x < levelLength; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                FindTileNeighbors(map[x, y]);
            }
        }
    }

    void FindTileNeighbors(Tile tile)
    {
        List<Tile> neighborsFound = new List<Tile>();
        if (tile.coord.x + 1 < levelLength)
            neighborsFound.Add(map[tile.coord.x + 1, tile.coord.y]);
        if (tile.coord.x - 1 >= 0)
            neighborsFound.Add(map[tile.coord.x - 1, tile.coord.y]);
        if (tile.coord.y + 1 < levelHeight)
            neighborsFound.Add(map[tile.coord.x, tile.coord.y + 1]);
        if (tile.coord.y - 1 >= 0)
            neighborsFound.Add(map[tile.coord.x, tile.coord.y - 1]);
        tile.neighbors = neighborsFound;
    }

    void FindNeighbors(Tile tile)
    {
        Coord[] directions = new Coord[4]
        {
            new Coord(0,1),
            new Coord(0,-1),
            new Coord(1,0),
            new Coord(-1,0)
        };
        List<Tile> neighbors = new List<Tile>();

        for (int i = 0; i < directions.Length; i++)
        {
            Tile neighbor;
            if(mapDict.TryGetValue(tile.coord.Add(directions[i]), out neighbor))
            {
                neighbors.Add(neighbor);
            }
        }
        tile.neighbors = neighbors;
    }

    Tile GenerateRandomTile(int minColumn, int maxColumn)
    {
        int x = Random.Range(
            Mathf.Min(minColumn, levelLength - 1),
            Mathf.Min(maxColumn, levelLength));
        int y = Random.Range(0, levelHeight);
        return map[x, y];
    }

    Tile GenerateRandomTile(Tile origin, int radius)
    {
        List<Tile> possibleTiles = AStarSearch.FindAllAvailableGoals(origin, radius, true);
        return possibleTiles[Random.Range(0, possibleTiles.Count)];
    }

    bool ValidateTile(Tile tile, int minDistFromMonster, int minDistFromCard)
    {
        if (Services.GameManager.player.currentTile == tile) return false;
        for (int i = 0; i < Services.MonsterManager.monsters.Count; i++)
        {
            if (Services.MonsterManager.monsters[i].currentTile != null &&
                Services.MonsterManager.monsters[i].currentTile.coord.Distance(tile.coord) 
                < minDistFromMonster)
                return false;
        }
        for (int j = 0; j < chestsOnBoard.Count; j++)
        {
            if (chestsOnBoard[j].currentTile.coord.Distance(tile.coord) < minDistFromCard)
                return false;
        }
        return true;
    }

    public Tile GenerateValidTile(int minDistFromMonster, int minDistFromCard,
        int minCol, int maxCol)
    {
        Tile tile;
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            tile = GenerateRandomTile(minCol, maxCol);
            if (ValidateTile(tile, minDistFromMonster, minDistFromCard))
                return tile;
        }
        return null;
    }

    public Tile GenerateValidTile(Tile origin, int radius, int minDIstFromMonster,
        int minDistFromCard)
    {
        Tile tile;
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            tile = GenerateRandomTile(origin, radius);
            if (ValidateTile(tile, minDIstFromMonster, minDistFromCard))
                return tile;
        }
        return null;
    }

    void GenerateChests(int levelNum)
    {
        int numChests = Mathf.RoundToInt(rooms.Count * chestsPerRoom);
        chestsOnBoard = new List<Chest>();
        roomsWithoutChests = new List<Room>(rooms);
        int highestTier = Services.CardConfig.HighestTierOfCardsAvailable(false);
        int potentialHighEnd = 1 + (levelNum / levelsPerCardTierIncrease);
        int highEndTier = Mathf.Min(potentialHighEnd, highestTier);
        int lowEndTier = Mathf.Max(highEndTier - 1, 1);
        if (potentialHighEnd > highestTier)
        {
            lowEndTier = highEndTier;
        }

        float ratioOfLowTierCards =
            ((levelsPerCardTierIncrease - (levelNum % levelsPerCardTierIncrease)) /
            (float)(levelsPerCardTierIncrease + 1));
        int numLowTierCards = Mathf.RoundToInt(ratioOfLowTierCards * numChests);
        int numHighTierCards = numChests - numLowTierCards;


        GenerateChestsOfTier(numLowTierCards, lowEndTier);
        GenerateChestsOfTier(numHighTierCards, highEndTier);
    }

    void GenerateChestsOfTier(int numChests, int tier)
    {
        for (int i = 0; i < numChests; i++)
        {
            Room randomRoomWithoutChest = 
                roomsWithoutChests[Random.Range(0, roomsWithoutChests.Count)];
            roomsWithoutChests.Remove(randomRoomWithoutChest);
            Tile chestTile = randomRoomWithoutChest.tiles
                [Random.Range(0, randomRoomWithoutChest.tiles.Count)];
            while(chestTile.containedKey != null || 
                Services.GameManager.player.currentTile == chestTile)
            {
                chestTile = randomRoomWithoutChest.tiles
                [Random.Range(0, randomRoomWithoutChest.tiles.Count)];
            }
            if (chestTile != null)
            {
                Chest chest = Instantiate(Services.Prefabs.Chest, Services.Main.transform).
                    GetComponent<Chest>();
                chest.currentTile = chestTile;
                chest.transform.position = chestTile.controller.transform.position;
                chest.tier = tier;
                chestTile.containedChest = chest;
                chestsOnBoard.Add(chest);
            }
            else break;
        }
        Debug.Log("generated " + numChests + " chests of tier " + tier);
    }
}
