using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    [SerializeField]
    private int baseLevelLength;
    [HideInInspector]
    public int levelLength, levelHeight;
    [SerializeField]
    private int minRoomDimension, maxRoomDimension;
    [SerializeField]
    private int roomGenRectDimension;
    [SerializeField]
    private int startingRoomWidth, startingRoomHeight;
    [SerializeField]
    private int minRoomDist, maxRoomDist;
    [SerializeField]
    private int numRoomsToGen;
    [SerializeField]
    private int rowsPerCard;
    public Tile[,] map { get; private set; }
    [SerializeField]
    private int maxTriesProcGen;
    //public List<Card> cardsOnBoard;
    public List<Chest> chestsOnBoard;
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
                    map[x, y] = new Tile(new Coord(x, y), true);
                }
                else map[x, y] = new Tile(new Coord(x, y), false);
            }
        }
        PlaceKey(map[levelLength - 1, levelHeight - 1]);
        FindAllNeighbors();
        SetSprites();
        int chestsToGenerate = levelLength / rowsPerCard;
        GenerateChests(levelNum, chestsToGenerate);
    }

    public void MakeTestTiles()
    {
        List<Tuple<Coord, IntVector2>> testRooms = GenerateInitialRooms(numRoomsToGen);
        for (int i = 0; i < testRooms.Count; i++)
        {
            GenerateRoomTiles(testRooms[i]);
        }

    }

    void GenerateRoomTiles(Tuple<Coord, IntVector2> room)
    {
        for (int i = 0; i < room.second.x - 1; i++)
        {
            for (int j = 0; j < room.second.y -1; j++)
            {
                new Tile(new Coord(room.first.x + i, room.first.y + j), false);
            }
        }
    }

    List<Tuple<Coord, IntVector2>> GenerateInitialRooms(int numRooms)
    {
        List<Tuple<Coord, IntVector2>> rooms = new List<Tuple<Coord, IntVector2>>();
        for (int i = 0; i < numRooms; i++)
        {
            Coord coord = 
                GenerateRandomCoord(0, roomGenRectDimension, 0, roomGenRectDimension);
            IntVector2 dimensions = GenerateRoomDimensions();
            Tuple<Coord, IntVector2> room = GenerateValidRoom(rooms);
            if (room == null)
            {
                Debug.Log("stopping early after " + i + " rooms");
                break;
            }
            rooms.Add(room);
        }
        return rooms;
    }

    bool IsRoomValid(Tuple<Coord, IntVector2> room, List<Tuple<Coord, IntVector2>> otherRooms)
    {
        for (int i = 0; i < otherRooms.Count; i++)
        {
            if (MinRoomDistance(room, otherRooms[i]) < minRoomDist)
            {
                Debug.Log("too close, distance of " + MinRoomDistance(room, otherRooms[i]));
                return false;
            }
        }
        return true;
    }

    Tuple<Coord, IntVector2> GenerateValidRoom(List<Tuple<Coord, IntVector2>> otherRooms)
    {
        Tuple<Coord, IntVector2> room;
        for (int i = 0; i < maxTriesProcGen; i++)
        {
            IntVector2 dimensions = GenerateRoomDimensions();
            Coord coord = new Coord(0, 0);
            if (otherRooms.Count > 0)
            {
                Tuple<Coord, IntVector2> referenceRoom = 
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
                    referenceRoom.first.x + (referenceRoom.second.x / 2) + distanceX,
                    referenceRoom.first.y + (referenceRoom.second.y / 2) + distanceY);
            }
            
            room = new Tuple<Coord, IntVector2>(coord, dimensions);
            Debug.Log("trying room of size " + room.second.x + "," + room.second.y +
                " at " + room.first.x + "," + room.first.y);
            if (IsRoomValid(room, otherRooms)) return room;
        }
        return null;
    }

    int MinRoomDistance(Tuple<Coord, IntVector2> roomA, Tuple<Coord, IntVector2> roomB)
    {
        if (RoomsTouching(roomA, roomB)) return 0;
        int xDistance = Mathf.Min(
            Mathf.Abs(RoomLeft(roomA) - RoomRight(roomB)),
            Mathf.Abs(RoomLeft(roomB) - RoomRight(roomA)));
        int yDistance = Mathf.Min(
            Mathf.Abs(RoomBottom(roomA) - RoomTop(roomB)),
            Mathf.Abs(RoomBottom(roomB) - RoomTop(roomA)));
        return Mathf.Max(xDistance, yDistance);
    }

    void SpaceOutRooms(List<Tuple<Coord,IntVector2>> rooms)
    {
        bool anyRoomsTouching = true;
        int shiftX, shiftY, shiftXA, shiftYA, shiftXB, shiftYB;
        while (anyRoomsTouching)
        {
            anyRoomsTouching = false;
            for (int i = 0; i < rooms.Count; i++)
            {
                Tuple<Coord, IntVector2> roomA = rooms[i];
                for (int j = i+1; j < rooms.Count; j++)
                {
                    Tuple<Coord, IntVector2> roomB = rooms[j];
                    if(RoomsTouching(roomA, roomB))
                    {
                        anyRoomsTouching = true;
                        int xOverlap = Mathf.Min(
                            RoomRight(roomA) - RoomLeft(roomB) + 1,
                            RoomRight(roomB) - RoomLeft(roomA) + 1);
                        int yOverlap = Mathf.Min(
                            RoomTop(roomA) - RoomBottom(roomB) + 1,
                            RoomTop(roomB) - RoomBottom(roomA) + 1);
                        bool shiftALeft = false;
                        bool shiftADown = false;
                        bool shiftHorizontal = false;
                        if(xOverlap < yOverlap)
                        {
                            shiftHorizontal = true;
                            if (RoomRight(roomA) - RoomLeft(roomB) + 1 == xOverlap)
                            {
                                shiftALeft = true;
                            }
                        }
                        else
                        {
                            if(RoomTop(roomA) - RoomBottom(roomB) + 1 == yOverlap)
                            {
                                shiftADown = true;
                            }
                        }
                        if (shiftHorizontal)
                        {
                            shiftX = xOverlap;
                            shiftY = 0;
                            shiftYA = 0;
                            shiftYB = 0;
                            if (shiftALeft)
                            {
                                shiftXA = -shiftX / 2;
                                shiftXB = shiftX + shiftXA;
                            }
                            else
                            {
                                shiftXB = -shiftX / 2;
                                shiftXA = shiftX + shiftXB;
                            }
                        }
                        else
                        {
                            shiftX = 0;
                            shiftY = yOverlap;
                            shiftXA = 0;
                            shiftXB = 0;
                            if (shiftADown)
                            {
                                shiftYA = -shiftY / 2;
                                shiftYB = shiftY + shiftYA;
                            }
                            else
                            {
                                shiftYB = -shiftY / 2;
                                shiftYA = shiftY + shiftYB;
                            }
                        }
                        Debug.Log("total x shift " + shiftX);
                        Debug.Log("total y shift " + shiftY);
                        Debug.Log("shifting room A at " + roomA.first.x + ", " + roomA.first.y +
                            " of size " + roomA.second.x + ", " + roomA.second.y +
                            " by " + shiftXA + ", " + shiftYA);
                        Debug.Log("shifting room B at " + roomB.first.x + ", " + roomB.first.y +
                            " of size " + roomB.second.x + ", " + roomB.second.y +
                            " by " + shiftXB + ", " + shiftYB);

                        roomA = new Tuple<Coord, IntVector2>(new Coord(
                            roomA.first.x + shiftXA, roomA.first.y + shiftYA), roomA.second);
                        roomB = new Tuple<Coord, IntVector2>(new Coord(
                            roomB.first.x + shiftXB, roomB.first.y + shiftYB), roomB.second);
                        rooms[i] = roomA;
                        rooms[j] = roomB;
                    }
                }
            }
        }
    }

    int RoomLeft(Tuple<Coord, IntVector2> room)
    {
        return room.first.x;
    }

    int RoomRight(Tuple<Coord, IntVector2> room)
    {
        return room.first.x + room.second.x - 1;
    }

    int RoomBottom(Tuple<Coord, IntVector2> room)
    {
        return room.first.y;
    }

    int RoomTop(Tuple<Coord, IntVector2> room)
    {
        return room.first.y + room.second.y - 1;
    }

    bool RoomsTouching(Tuple<Coord, IntVector2> a, Tuple<Coord, IntVector2> b)
    {
        Coord[] corners = new Coord[4];
        Coord bottomLeftCorner = a.first;
        Coord bottomRightCorner = new Coord(a.first.x + a.second.x - 1, a.first.y);
        Coord topLeftCorner = new Coord(a.first.x, a.first.y + a.second.y - 1);
        Coord topRightCorner = new Coord(a.first.x + a.second.x - 1, a.first.y + a.second.y - 1);
        corners[0] = bottomLeftCorner;
        corners[1] = bottomRightCorner;
        corners[2] = topLeftCorner;
        corners[3] = topRightCorner;
        foreach(Coord corner in corners)
        {
            if (CoordContainedWithinRoom(corner, b)) return true;
        }
        return false;
    }

    bool CoordContainedWithinRoom(Coord coord, Tuple<Coord, IntVector2> room)
    {
        return (coord.x >= room.first.x && coord.x <= room.first.x + room.second.x - 1) &&
            (coord.y >= room.first.y && coord.y <= room.first.y + room.second.y - 1);
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

    void GenerateChests(int levelNum, int numChests)
    {
        chestsOnBoard = new List<Chest>();
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
            Tile chestTile = GenerateValidTile(
                Services.CardConfig.MinSpawnDistFromMonsters,
                Services.CardConfig.MinSpawnDistFromItems,
                Services.CardConfig.MinSpawnCol,
                levelLength - 1);
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
