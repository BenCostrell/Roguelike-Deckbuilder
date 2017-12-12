using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DelaunayTriangulator;
using System.Linq;

public class MapManager : MonoBehaviour {

    [SerializeField]
    private int baseLevelDimension;
    [SerializeField]
    private float heightToWidthRatio;
    [SerializeField]
    private int dimensionIncreasePerLevel;
    [SerializeField]
    private float initialHeavyProb;
    [SerializeField]
    private int spawnThresh;
    [SerializeField]
    private int growthThresh;
    [SerializeField]
    private int matureThresh;
    [SerializeField]
    private int friendThresh;
    [SerializeField]
    private int phaseOneStepCount;
    [SerializeField]
    private float minFillPct;
    [SerializeField]
    private int maxTriesLevelGen;
    [SerializeField]
    private int minMapObjDist;
    [SerializeField]
    private float chestsPerLevel;
    [SerializeField]
    private float fountainsPerLevel;
    [SerializeField]
    private float initSaplingProportion;
    [SerializeField]
    private int initSaplingRadius;
    private int width;
    private int height;
    public Tile[,] mapGrid { get; private set; }
    public enum SpaceType { Empty, LightGrowth, HeavyGrowth }
    private List<Tile> emptyTiles;
    private List<Tile> tilesWithSpecialStuff;
    private List<Tile> bufferTiles;
    [SerializeField]
    private int bufferLength;
    private List<Tile> targetTiles;
    private List<Sprout> currentLiveSprouts;


    [SerializeField]
    private int minRoomDimension;
    [SerializeField]
    private int maxRoomDimension;
    [SerializeField]
    private int levelOneRoomDimension;
    [SerializeField]
    private int hallwayWidth;
    [SerializeField]
    private int minRoomDist;
    [SerializeField]
    private int maxRoomDist;
    [SerializeField]
    private int baseRooms;
    [SerializeField]
    private float roomsPerLevel;
    private int numRooms;
    [SerializeField]
    private float proportionOfEdgesToReAdd;
    public Dictionary<Coord, Tile> mapDict { get; private set; }
    [SerializeField]
    private int maxTriesProcGen;
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
    private int levelsPerCardTierIncrease;
    [SerializeField]
    private float chestsPerRoom;
    [SerializeField]
    private float fountainsPerRoom;
    private List<Room> rooms;
    private List<Room> roomsWithoutChests;
    public Tile playerSpawnTile { get; private set; }
    public Tile exitTile { get; private set; }
    private List<MapObject> litObjects;

    private void Update()
    {
        for (int i = 0; i < litObjects.Count; i++)
        {
            litObjects[i].AdjustLighting();
        }
    }

    public void GenerateLevel(int levelNum)
    {
        #region Generate rooms
        numRooms = baseRooms + Mathf.RoundToInt(levelNum * roomsPerLevel);
        if (numRooms == 1)
        {
            rooms = new List<Room>(){new Room(new Coord(0, 0),
                new IntVector2(levelOneRoomDimension, levelOneRoomDimension)) };
        }
        else
        {
            rooms = GenerateInitialRooms(numRooms);
        }
        List<Tile> allTiles = new List<Tile>();
        for (int i = 0; i < rooms.Count; i++)
        {
            allTiles.AddRange(GenerateRoomTiles(rooms[i]));
        }
        #endregion

        List<Edge> edges = new List<Edge>();
        List<Edge> minSpanningTree = new List<Edge>();
        if (numRooms >= 2)
        {

            #region Calculate edges
            if (numRooms >= 3)
            {
                List<Edge> delaunayTriangulation = DelaunayTriangulationOfRooms(rooms);
                delaunayTriangulation = RemoveEdgeDuplicates(delaunayTriangulation);
                List<Edge> orderedEdges =
                    new List<Edge>(delaunayTriangulation.OrderBy(edge => edge.Length));
                minSpanningTree = GetMinimumSpanningTree(delaunayTriangulation);
                edges = ReAddEdgesToSpanningTree(minSpanningTree, orderedEdges);
            }
            else if (numRooms == 2)
            {
                edges = new List<Edge>() { new Edge(rooms[0], rooms[1]) };
                minSpanningTree = edges;
            }
            #endregion

            #region Assign room neighbors
            foreach (Room room in rooms)
            {
                List<Edge> neighborEdges = AdjacentEdgesToVertex(room, edges);
                List<Tuple<Room, float>> neighbors = new List<Tuple<Room, float>>();
                foreach (Edge edge in neighborEdges)
                {
                    if (edge.a != room) neighbors.Add(new Tuple<Room, float>(edge.a, edge.Length));
                    else neighbors.Add(new Tuple<Room, float>(edge.b, edge.Length));
                }
                room.neighbors = neighbors;
            }
            #endregion

            #region Generate hallways
            List<Coord> hallwayCoords = new List<Coord>();
            for (int i = 0; i < edges.Count; i++)
            {
                hallwayCoords.AddRange(GenerateHallway(edges[i]));
            }
            hallwayCoords = RemoveCoordDuplicates(hallwayCoords);
            allTiles.AddRange(GenerateHallwayTiles(hallwayCoords));
            #endregion
        }

        #region Generate map dictionary and assign tile neighbors
        mapDict = new Dictionary<Coord, Tile>();
        foreach (Tile tile in allTiles)
        {
            mapDict.Add(tile.coord, tile);
        }
        foreach (Tile tile in allTiles)
        {
            FindNeighbors(tile);
        }
        #endregion

        #region Assign start and exit rooms
        Room startRoom, exitRoom;
        if (numRooms >= 2)
        {
            startRoom = GetFarthestRoom(rooms[0], edges, rooms);
            playerSpawnTile = GetFarthestTileFromHallwayEntrances(startRoom);
            exitRoom = GetFarthestRoom(startRoom, minSpanningTree, rooms);
            exitTile = GetFarthestTileFromHallwayEntrances(exitRoom);
        }
        else
        {
            startRoom = rooms[0];
            exitRoom = rooms[0];
            playerSpawnTile = mapDict[new Coord(startRoom.Left, startRoom.Bottom)];
            exitTile = mapDict[new Coord(startRoom.Right - 1, startRoom.Top - 1)];
        }
        exitTile.isExit = true;
        exitTile.SetSprite(exitDoorSprite, Quaternion.identity);
        #endregion
        GenerateChests(levelNum);
        GenerateFountains(levelNum);
    }

    public void GenerateLevelTest(int levelNum)
    {
        //Services.UIManager.canvas.SetActive(false);
        SpaceType[,] spaceTypeMap = new SpaceType[width, height];
        for (int i = 0; i < maxTriesLevelGen; i++)
        {
            spaceTypeMap = GenerateSpaceTypeMap(levelNum);
            SpaceType[,] floodFilledMap = FloodFillCheck(spaceTypeMap,
                i == maxTriesLevelGen - 1);
            if (floodFilledMap != null)
            {
                spaceTypeMap = floodFilledMap;
                Debug.Log("num map tries: " + (i + 1));
                break;
            }
        }


        List<Tile> openTiles = new List<Tile>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                mapGrid[i, j] = new Tile(new Coord(i, j), false);
                switch (spaceTypeMap[i, j])
                {
                    case SpaceType.Empty:
                        openTiles.Add(mapGrid[i, j]);
                        break;
                    case SpaceType.LightGrowth:
                        LightBrush lightBrush = Services.MapObjectConfig
                            .CreateMapObjectOfType(MapObject.ObjectType.LightBrush)
                            as LightBrush;
                        lightBrush.CreatePhysicalObject(mapGrid[i, j]);
                        break;
                    case SpaceType.HeavyGrowth:
                        HeavyBrush heavyBrush = Services.MapObjectConfig
                            .CreateMapObjectOfType(MapObject.ObjectType.HeavyBrush)
                            as HeavyBrush;
                        heavyBrush.CreatePhysicalObject(mapGrid[i, j]);
                        break;
                    default:
                        break;
                }
            }
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                FindNeighborsInGrid(mapGrid[i, j]);
            }
        }


        Tile referenceTile = openTiles[0];

        playerSpawnTile = GetFarthestTile(referenceTile, openTiles);
        List<Tile> tilesNearbySpawnPoint = AStarSearch.FindAllAvailableGoals(playerSpawnTile, 4, true);
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            List<Tile> freeMovementSpaces = AStarSearch.FindAllAvailableGoals(playerSpawnTile, 3);
            if (freeMovementSpaces.Count >= 4) break;
            else
            {
                playerSpawnTile = tilesNearbySpawnPoint[0];
                tilesNearbySpawnPoint.RemoveAt(0);
                if (tilesNearbySpawnPoint.Count == 0) break;
            }
        }
        if (playerSpawnTile.containedMapObject != null)
            playerSpawnTile.containedMapObject.RemoveThis(false);
        else openTiles.Remove(playerSpawnTile);

        exitTile = GetFarthestTile(playerSpawnTile, openTiles);
        exitTile.isExit = true;
        //exitTile.SetSprite(exitDoorSprite, Quaternion.identity);
        Door door = Services.MapObjectConfig.CreateMapObjectOfType(MapObject.ObjectType.Door) as Door;
        door.CreatePhysicalObject(exitTile);
        openTiles.Remove(exitTile);
        emptyTiles = openTiles;
        tilesWithSpecialStuff = new List<Tile>() { playerSpawnTile, exitTile };
        litObjects = new List<MapObject>();
        GenerateChests(levelNum);
        GenerateFountains(levelNum);

        currentLiveSprouts = new List<Sprout>();
        SproutSaplings(mapGrid[width/2, height/2], initSaplingProportion,
            99999, false, 0);

        bufferTiles = new List<Tile>();
        for (int i = -bufferLength; i < width + bufferLength; i++)
        {
            for (int j = -bufferLength; j < height + bufferLength; j++)
            {
                if (i < 0 || i >= width || j < 0 || j >= height)
                {
                    Tile bufferTile = new Tile(new Coord(i, j), false);
                    HeavyBrush heavyBrush = Services.MapObjectConfig
                            .CreateMapObjectOfType(MapObject.ObjectType.HeavyBrush)
                            as HeavyBrush;
                    heavyBrush.CreatePhysicalObject(bufferTile);
                    bufferTiles.Add(bufferTile);
                }
            }
        }
    }

    SpaceType[,] GenerateSpaceTypeMap(int levelNum)
    {
        width = baseLevelDimension + (levelNum * dimensionIncreasePerLevel);
        height = Mathf.RoundToInt(width * heightToWidthRatio);
        mapGrid = new Tile[width, height];
        SpaceType[,] spaceTypeMap = InitializeMap();
        for (int i = 0; i < phaseOneStepCount; i++)
        {
            spaceTypeMap = DoSimulationStep(spaceTypeMap, spawnThresh, growthThresh,
                matureThresh, friendThresh);
        }
        return spaceTypeMap;
    }

    SpaceType[,] InitializeMap()
    {
        SpaceType[,] map = new SpaceType[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (i == 0 || j == 0 || i == width - 1 || j == height - 1)
                    map[i, j] = SpaceType.HeavyGrowth;
                else if (Random.Range(0, 1f) < initialHeavyProb)
                {
                    map[i, j] = SpaceType.LightGrowth;
                }
                else map[i, j] = SpaceType.Empty;
            }
        }
        return map;
    }

    SpaceType[,] DoSimulationStep(SpaceType[,] oldMap, int spawnT, int growthT,
        int matureT, int friendT)
    {
        SpaceType[,] newMap = new SpaceType[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //if (i == 0 || j == 0 || i == width - 1 || j == height - 1)
                //    newMap[i, j] = SpaceType.HeavyGrowth;
                //else
                //{
                int dist1HeavyCount =
                    GetNeighborCount(SpaceType.HeavyGrowth, oldMap, i, j, 1);
                int dist1LightCount =
                    GetNeighborCount(SpaceType.LightGrowth, oldMap, i, j, 1);
                int dist2HeavyCount =
                    GetNeighborCount(SpaceType.HeavyGrowth, oldMap, i, j, 2);
                int dist2LightCount =
                    GetNeighborCount(SpaceType.LightGrowth, oldMap, i, j, 2);
                if (oldMap[i, j] == SpaceType.Empty)
                {
                    if (dist1LightCount >= growthT || dist2LightCount <= spawnT)
                        newMap[i, j] = SpaceType.LightGrowth;
                    else newMap[i, j] = SpaceType.Empty;
                }
                else if (oldMap[i, j] == SpaceType.LightGrowth)
                {
                    if (dist1LightCount + dist1HeavyCount < friendT)
                        newMap[i, j] = SpaceType.Empty;
                    else if (dist1LightCount + dist1HeavyCount >= matureT)
                        newMap[i, j] = SpaceType.HeavyGrowth;
                    else newMap[i, j] = SpaceType.LightGrowth;
                }
                else if (oldMap[i, j] == SpaceType.HeavyGrowth)
                {
                    if (dist1HeavyCount + dist1LightCount < friendT)
                        newMap[i, j] = SpaceType.Empty;
                    else
                        newMap[i, j] = SpaceType.HeavyGrowth;
                }
                //}
            }
        }
        return newMap;
    }

    List<Tile> GetSproutCandidateTiles(Tile centerTile, int radius)
    {
        List<Tile> candidateTiles = new List<Tile>();
        targetTiles = new List<Tile>();

        foreach (Tile tile in mapGrid)
        {
            if (tile.coord.Distance(centerTile.coord) <= radius &&
                tile.containedMapObject != null &&
                (tile.containedMapObject is LightBrush ||
                tile.containedMapObject is HeavyBrush) && OpenNeighbors(tile).Count > 0)
            {
                candidateTiles.Add(tile);
            }
        }
        return candidateTiles;
    }

    public TaskTree SproutSaplings(Tile centerTile, float proportion, int radius, bool animate,
        float staggerTime)
    {
        List<Tile> candidateTiles = GetSproutCandidateTiles(centerTile, radius);

        int numNewSprouts = Mathf.RoundToInt(candidateTiles.Count * proportion);
        return SproutSaplings(numNewSprouts, candidateTiles, animate, staggerTime);
        
    }

    public TaskTree SproutSaplings(Tile centerTile, int numSeeds, int radius, bool animate, 
        float staggerTime)
    {
        List<Tile> candidateTiles = GetSproutCandidateTiles(centerTile, radius);
        return SproutSaplings(numSeeds, candidateTiles, animate, staggerTime);
    }

    public TaskTree SproutSaplings(int numSprouts, List<Tile> candidateTiles, 
        bool animate, float staggerTime)
    {
        TaskTree sproutTasks = new TaskTree(new EmptyTask());
        for (int i = 0; i < numSprouts; i++)
        {
            if (candidateTiles.Count > 0)
            {
                Tile sproutTile = candidateTiles[Random.Range(0, candidateTiles.Count)];
                candidateTiles.Remove(sproutTile);
                List<Tile> sproutNeighbors = OpenNeighbors(sproutTile);
                if (sproutNeighbors.Count > 0)
                {
                    Tile tileToGrowOn = sproutNeighbors[Random.Range(0, sproutNeighbors.Count)];
                    targetTiles.Add(tileToGrowOn);
                    Sprout sprout = Services.MapObjectConfig
                            .CreateMapObjectOfType(MapObject.ObjectType.Sprout) as Sprout;
                    if (animate)
                    {
                        sproutTasks.Then(new GrowObject(tileToGrowOn, staggerTime, sprout, true));
                    }
                    else
                    {
                        sprout.CreatePhysicalObject(tileToGrowOn);
                    }
                }
            }
            else break;
        }
        return sproutTasks;
    }

    public void OnSproutBirth(Sprout sprout)
    {
        currentLiveSprouts.Add(sprout);
    }

    public void OnSproutDeath(Sprout sprout)
    {
        currentLiveSprouts.Remove(sprout);
    }

    int GetNeighborCount(SpaceType type, SpaceType[,] map, int x, int y, int dist)
    {
        int count = 0;
        for (int i = -dist; i <= dist; i++)
        {
            for (int j = -dist; j <= dist; j++)
            {
                int neighborX = x + i;
                int neighborY = y + j;
                if ((i == 0 && j == 0)) { }
                else if (neighborX < 0 || neighborY < 0
                    || neighborX >= width || neighborY >= height) { }
                else if (map[neighborX, neighborY] == type)
                {
                    count += 1;
                }
            }
        }
        return count;
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

    Tile GetFarthestTile(Tile tile, List<Tile> openTiles)
    {
        float longestDistance = 0f;
        Tile farthestTile = tile;
        foreach (Tile otherTile in openTiles)
        {
            float dist =
                AStarSearch.ShortestPath(tile, otherTile, false, false, false, true).Count;
            if (dist > longestDistance)
            {
                farthestTile = otherTile;
                longestDistance = dist;
            }
        }
        return farthestTile;
    }

    Room GetFarthestRoom(Room room, List<Edge> graph, List<Room> otherRooms)
    {
        Room farthestRoom = room;
        float longestDistance = 0;
        foreach (Room otherRoom in otherRooms)
        {
            float distance = AStarSearch.ShortestPathDistance(room, otherRoom);
            if (distance > longestDistance)
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
        foreach (Edge edge in edges)
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
        foreach (Room room in rooms)
        {
            foreach (Tile tile in room.tiles)
            {
                roomCoords.Add(tile.coord);
            }
        }
        foreach (Coord coord in coordList)
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
        while (spanningTree.Count != numRooms - 1)
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
        foreach (Edge edge in graph)
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
            for (int j = 0; j < room.dimensions.y - 1; j++)
            {
                Tile tile = new Tile(new Coord(room.origin.x + i, room.origin.y + j), false);
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
            hallwayTiles.Add(new Tile(hallwayCoords[i], false));
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
        foreach (Coord coord in hallwayCoords)
        {
            if (coord.Distance(edge.a.CenterCoord) < shortestDistToRoomA)
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
                int distance = Random.Range(minRoomDist, maxRoomDist + 1);
                int distanceX = Random.Range(
                    dimensions.x / 2,
                    distance + dimensions.x / 2);
                int distanceY = distance - (distanceX - dimensions.x / 2) + dimensions.y / 2;
                int offsetX = referenceRoom.dimensions.x / 2;
                int offsetY = referenceRoom.dimensions.y / 2;
                if (Random.Range(0, 2) == 0)
                {
                    distanceX *= -1;
                    offsetX *= -1;
                }
                if (Random.Range(0, 2) == 0)
                {
                    distanceY *= -1;
                    offsetY *= -1;
                }
                coord = new Coord(
                    referenceRoom.CenterCoord.x + offsetX + distanceX,
                    referenceRoom.CenterCoord.y + offsetY + distanceY);
            }

            room = new Room(coord, dimensions);
            //Debug.Log("trying room of size " + room.dimensions.x + "," + room.dimensions.y +
            //    " at " + room.origin.x + "," + room.origin.y);
            if (IsRoomValid(room, otherRooms))
            {
                return room;
            }
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

    //void SetSprite(Tile tile)
    //{
    //    Sprite sprite;
    //    Quaternion rot = Quaternion.identity;

    //    if (tile.isExit)
    //    {
    //        sprite = exitDoorSprite;
    //    }
    //    else
    //    {
    //        bool hasUpNeighbor = false;
    //        bool hasDownNeighbor = false;
    //        bool hasLeftNeighbor = false;
    //        bool hasRightNeighbor = false;
    //        foreach (Tile neighbor in tile.neighbors)
    //        {
    //            Coord diff = neighbor.coord.Subtract(tile.coord);
    //            if (diff == new Coord(0, 1))
    //            {
    //                hasUpNeighbor = true;
    //            }
    //            if (diff == new Coord(0, -1))
    //            {
    //                hasDownNeighbor = true;
    //            }
    //            if (diff == new Coord(1, 0))
    //            {
    //                hasRightNeighbor = true;
    //            }
    //            if (diff == new Coord(-1, 0))
    //            {
    //                hasLeftNeighbor = true;
    //            }
    //        }
    //        if (hasUpNeighbor && hasDownNeighbor && hasLeftNeighbor && hasRightNeighbor)
    //            sprite = centerSprites[Random.Range(0, centerSprites.Length)];
    //        else if (hasUpNeighbor && hasDownNeighbor && hasRightNeighbor) sprite = leftSprite;
    //        else if (hasUpNeighbor && hasDownNeighbor && hasLeftNeighbor) sprite = rightSprite;
    //        else if (hasUpNeighbor && hasRightNeighbor && hasLeftNeighbor) sprite = botSprite;
    //        else if (hasDownNeighbor && hasRightNeighbor && hasLeftNeighbor)
    //        {
    //            sprite = botSprite;
    //            rot = Quaternion.Euler(new Vector3(0, 0, 180));
    //        }
    //        else if (hasUpNeighbor && hasLeftNeighbor) sprite = botRightCornerSprite;
    //        else if (hasUpNeighbor && hasRightNeighbor) sprite = botLeftCornerSprite;
    //        else if (hasDownNeighbor && hasLeftNeighbor)
    //        {
    //            sprite = botLeftCornerSprite;
    //            rot = Quaternion.Euler(new Vector3(0, 0, 180));
    //        }
    //        else if (hasDownNeighbor && hasRightNeighbor)
    //        {
    //            sprite = botRightCornerSprite;
    //            rot = Quaternion.Euler(new Vector3(0, 0, 180));
    //        }
    //        else sprite = centerSprites[Random.Range(0, centerSprites.Length)];
    //    }
    //    tile.SetSprite(sprite, rot);
    //}

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
            if (mapDict.TryGetValue(tile.coord.Add(directions[i]), out neighbor))
            {
                neighbors.Add(neighbor);
            }
        }
        tile.neighbors = neighbors;
    }

    void FindNeighborsInGrid(Tile tile)
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
            Coord neighborCoord = tile.coord.Add(directions[i]);
            if (ContainedInMap(neighborCoord)) {
                neighbors.Add(mapGrid[neighborCoord.x, neighborCoord.y]);
            }
        }
        tile.neighbors = neighbors;
    }

    public bool ContainedInMap(Coord coord)
    {
        return coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;
    }

    SpaceType[,] FloodFillCheck(SpaceType[,] oldMap, bool force)
    {
        SpaceType[,] newMap = new SpaceType[width, height];
        List<Coord> uncheckedSpaces = new List<Coord>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (oldMap[i, j] != SpaceType.HeavyGrowth)
                {
                    uncheckedSpaces.Add(new Coord(i, j));
                }
            }
        }
        List<Coord> biggestRoom = new List<Coord>();
        int biggestRoomSize = 0;
        while (uncheckedSpaces.Count > 0) {
            Queue<Coord> queue = new Queue<Coord>();
            queue.Enqueue(uncheckedSpaces[0]);
            uncheckedSpaces.RemoveAt(0);
            List<Coord> cavernSpaces = new List<Coord>();
            while (queue.Count > 0)
            {
                Coord space = queue.Dequeue();
                cavernSpaces.Add(space);
                Coord[] directions = Coord.Directions();
                for (int i = 0; i < directions.Length; i++)
                {
                    Coord neighbor = space.Add(directions[i]);
                    if (uncheckedSpaces.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        uncheckedSpaces.Remove(neighbor);
                    }
                }
            }
            Debug.Log("finished with cavern sized " + cavernSpaces.Count);
            if (cavernSpaces.Count > biggestRoomSize)
            {
                biggestRoom = cavernSpaces;
                biggestRoomSize = biggestRoom.Count;
            }
        }
        if (!force && biggestRoomSize / (width * height * heightToWidthRatio) < minFillPct)
            return null;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!biggestRoom.Contains(new Coord(i, j)))
                    newMap[i, j] = SpaceType.HeavyGrowth;
                else newMap[i, j] = oldMap[i, j];
            }
        }
        return newMap;
    }

    void GenerateChests(int levelNum)
    {
        //int numChests = Mathf.CeilToInt(rooms.Count * chestsPerRoom);
        int numChests = Mathf.CeilToInt(chestsPerLevel * levelNum);
        chestsOnBoard = new List<Chest>();
        //roomsWithoutChests = new List<Room>(rooms);
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
            //Room randomRoomWithoutChest = 
            //    roomsWithoutChests[Random.Range(0, roomsWithoutChests.Count)];
            //roomsWithoutChests.Remove(randomRoomWithoutChest);
            //Tile chestTile = randomRoomWithoutChest.tiles
            //    [Random.Range(0, randomRoomWithoutChest.tiles.Count)];
            Tile chestTile = GetValidObjectTile();
            if (chestTile != null)
            {
                Chest chest = Services.MapObjectConfig.CreateMapObjectOfType(MapObject.ObjectType.Chest) as Chest;
                chest.CreatePhysicalObject(chestTile);
                litObjects.Add(chest);
                chest.tier = tier;
                chestsOnBoard.Add(chest);
                emptyTiles.Remove(chestTile);
                tilesWithSpecialStuff.Add(chestTile);
            }
            else break;
        }
    }

    void GenerateFountains(int levelNum)
    {
        //int numFountains = Mathf.CeilToInt(fountainsPerRoom * rooms.Count);
        int numFountains = Mathf.CeilToInt(fountainsPerLevel * levelNum);
        //List<Room> roomsWithoutFountains = new List<Room>(rooms);
        for (int i = 0; i < numFountains; i++)
        {
            //Room randomRoomWithoutFountain =
            //    roomsWithoutFountains[Random.Range(0, roomsWithoutFountains.Count)];
            //roomsWithoutChests.Remove(randomRoomWithoutFountain);
            //Tile fountainTile = randomRoomWithoutFountain.tiles
            //    [Random.Range(0, randomRoomWithoutFountain.tiles.Count)];
            //while (fountainTile == exitTile || fountainTile == playerSpawnTile || fountainTile.isExit
            //    || fountainTile.containedChest != null)
            //{
            //    fountainTile = randomRoomWithoutFountain.tiles
            //    [Random.Range(0, randomRoomWithoutFountain.tiles.Count)];
            //}
            Tile fountainTile = GetValidObjectTile();
            if (fountainTile != null)
            {
                MapObject fountain =
                    Services.MapObjectConfig.CreateMapObjectOfType(MapObject.ObjectType.Fountain);
                fountain.CreatePhysicalObject(fountainTile);
                litObjects.Add(fountain);
                emptyTiles.Remove(fountainTile);
                tilesWithSpecialStuff.Add(fountainTile);
            }
            else break;
        }
    }

    Tile GetValidObjectTile()
    {
        if (emptyTiles.Count == 0) return null;
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            Tile tile = emptyTiles[Random.Range(0, emptyTiles.Count)];
            if (IsObjectPlacementValid(tile)) return tile;
        }
        return null;
    }

    bool IsObjectPlacementValid(Tile tile)
    {
        for (int i = 0; i < tilesWithSpecialStuff.Count; i++)
        {
            if (AStarSearch.ShortestPath(tile, tilesWithSpecialStuff[i],
                false, false, false, true).Count < minMapObjDist)
                return false;
        }
        return true;
    }

    List<Tile> OpenNeighbors(Tile tile)
    {
        List<Tile> openNeighbors = new List<Tile>();
        for (int i = 0; i < tile.neighbors.Count; i++)
        {
            Tile neighbor = tile.neighbors[i];
            if (!neighbor.IsImpassable() && !neighbor.isExit && !targetTiles.Contains(neighbor)
                && neighbor.containedMapObject == null && 
                neighbor != Services.GameManager.player.currentTile)
                openNeighbors.Add(neighbor);
        }
        return openNeighbors;
    }

    //public TaskTree Growth(int radius, float proportion, float staggerTime)
    //{
    //    List<Tile> brushTiles = new List<Tile>();
    //    targetTiles = new List<Tile>();
    //    foreach (Tile tile in mapGrid)
    //    {
    //        if (tile.coord.Distance(Services.GameManager.player.currentTile.coord) <= radius &&
    //            tile.containedMapObject != null &&
    //            (tile.containedMapObject is LightBrush ||
    //            tile.containedMapObject is HeavyBrush) && OpenNeighbors(tile).Count > 0) {
    //            brushTiles.Add(tile);
    //        }
    //    }
    //    int numNewGrowth = Mathf.RoundToInt(brushTiles.Count * proportion);
    //    Debug.Log(brushTiles.Count + " brush tiles");
    //    Debug.Log("growing " + numNewGrowth + " new brushes");
    //    TaskTree growthTask = new TaskTree(new EmptyTask());
    //    for (int i = 0; i < numNewGrowth; i++)
    //    {
    //        Tile brushTile = brushTiles[Random.Range(0, brushTiles.Count)];
    //        brushTiles.Remove(brushTile);
    //        List<Tile> brushNeighbors = OpenNeighbors(brushTile);
    //        if (brushNeighbors.Count > 0)
    //        {
    //            Tile tileToGrowOn = brushNeighbors[Random.Range(0, brushNeighbors.Count)];
    //            targetTiles.Add(tileToGrowOn);
    //            growthTask.Then(new GrowObject(tileToGrowOn, staggerTime, MapObject.ObjectType.LightBrush));
    //        }
    //    }
    //    return growthTask;
    //}

    public TaskTree Overgrowth(int radius, int numNewBrushes, float staggerTime)
    {
        TaskTree growthTasks = new TaskTree(new EmptyTask());
        for (int i = 0; i < numNewBrushes; i++)
        {
            Sprout sprout = GetLiveSprout(radius);
            if (sprout != null)
            {
                currentLiveSprouts.Remove(sprout);
                TaskTree growthSubtask = new TaskTree(new EmptyTask());
                growthSubtask.AddChild(new FadeOutSprout(sprout));
                growthSubtask.AddChild(new GrowObject(sprout.GetCurrentTile(), staggerTime,
                    Services.MapObjectConfig.CreateMapObjectOfType(
                        MapObject.ObjectType.LightBrush)));
                growthTasks.Then(growthSubtask);
            }
            else break;
        }
        return growthTasks;
    }

    public Sprout GetLiveSprout(int radius)
    {
        List<Sprout> sproutsInRange = new List<Sprout>();
        for (int i = 0; i < currentLiveSprouts.Count; i++)
        {
            Sprout sprout = currentLiveSprouts[i];
            Tile sproutTile = sprout.GetCurrentTile();
            if (sproutTile.coord
                .Distance(Services.GameManager.player.currentTile.coord) <= radius &&
                sproutTile != Services.GameManager.player.currentTile && 
                sproutTile.containedMonster == null)
            {
                sproutsInRange.Add(sprout);
            }
        }
        if (sproutsInRange.Count > 0)
            return sproutsInRange[Random.Range(0, sproutsInRange.Count)];
        else
            return null;
    }

    public void RemoveLitMapObject(MapObject obj)
    {
        litObjects.Remove(obj);
    }
}
