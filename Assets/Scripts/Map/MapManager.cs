using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    public int levelLength;
    [SerializeField]
    private int levelHeight;
    [SerializeField]
    private int rowsPerCard;
    public Tile[,] map { get; private set; }
    [SerializeField]
    private int maxTriesProcGen;
    //public List<Card> cardsOnBoard;
    public List<Chest> chestsOnBoard;
    [SerializeField]
    private Sprite botLeftCornerSprite;
    [SerializeField]
    private Sprite botRightCornerSprite;
    [SerializeField]
    private Sprite botSprite;
    [SerializeField]
    private Sprite leftSprite;
    [SerializeField]
    private Sprite rightSprite;
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
        int extraLevelLength = Mathf.RoundToInt(levelNum * extraLevelLengthPerLevel);
        levelLength += extraLevelLength;
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
        //GenerateBoardCards(levelNum, cardsToGenerate);
        GenerateChests(levelNum, chestsToGenerate);
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
        Debug.Log("generated " + numChests + " cards of tier " + tier);
    }

    //void GenerateBoardCards(int levelNum, int numCards)
    //{
    //    cardsOnBoard = new List<Card>();
    //    int highEndTier = 1 + (levelNum / levelsPerCardTierIncrease);
    //    highEndTier = Mathf.Min(highEndTier, 
    //        Services.CardConfig.HighestTierOfCardsAvailable(false));
    //    int lowEndTier = Mathf.Max(highEndTier - 1, 1);

    //    float ratioOfLowTierCards =
    //        ((levelsPerCardTierIncrease - (levelNum % levelsPerCardTierIncrease)) /
    //        (float)(levelsPerCardTierIncrease + 1));
    //    int numLowTierCards = Mathf.RoundToInt(ratioOfLowTierCards * numCards);
    //    int numHighTierCards = numCards - numLowTierCards;

    //    GenerateCardsOnBoardOfTier(numLowTierCards, lowEndTier);
    //    GenerateCardsOnBoardOfTier(numHighTierCards, highEndTier);
    //}

    //void GenerateCardsOnBoardOfTier(int numCards, int tier)
    //{
    //    for (int i = 0; i < numCards; i++)
    //    {
    //        Card.CardType cardType = Services.CardConfig.GenerateTypeOfTier(tier, false);
    //        Tile cardTile = GenerateValidTile(
    //            Services.CardConfig.MinSpawnDistFromMonsters,
    //            Services.CardConfig.MinSpawnDistFromItems, 
    //            Services.CardConfig.MinSpawnCol, 
    //            levelLength - 1);
    //        if (cardTile != null)
    //        {
    //            cardsOnBoard.Add(Services.CardConfig.GenerateCard(cardType, cardTile));
    //        }
    //        else break;
    //    }
    //    Debug.Log("generated " + numCards + " cards of tier " + tier);
    //}
}
