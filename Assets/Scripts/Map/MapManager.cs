using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    [SerializeField]
    private int levelLength;
    [SerializeField]
    private int levelHeight;
    [SerializeField]
    private int rowsPerCard;
    public Tile[,] map { get; private set; }
    [SerializeField]
    private int maxTriesProcGen;
    public List<Card> cardsOnBoard;
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

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void GenerateLevel()
    {
        map = new Tile[levelLength, levelHeight];
        for (int x = 0; x < levelLength; x++)
        {
            for (int y = 0; y < levelHeight; y++)
            {
                if(x == levelLength-1 && y == levelHeight - 1)
                {
                    map[x, y] = new Tile(new Coord(x, y), true);
                }
                else map[x, y] = new Tile(new Coord(x, y), false);
            }
        }
        FindAllNeighbors();
        SetSprites();
        int cardsToGenerate = levelLength / rowsPerCard;
        GenerateCardsOnBoard(cardsToGenerate, 1);
    }

    void SetSprites()
    {
        foreach(Tile tile in map) SetSprite(tile);
    }

    void SetSprite(Tile tile)
    {
        if (tile.isExit)
        {
            GameObject door = 
                Instantiate(Services.Prefabs.ExitDoor, tile.controller.transform);
            door.transform.localPosition = Vector3.zero;
            return;
        }
        Sprite sprite;
        Quaternion rot = Quaternion.identity;
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
        for (int j = 0; j < cardsOnBoard.Count; j++)
        {
            if (cardsOnBoard[j].currentTile.coord.Distance(tile.coord) < minDistFromCard)
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

    void GenerateCardsOnBoard(int numCards, int tier)
    {
        cardsOnBoard = new List<Card>();
        for (int i = 0; i < numCards; i++)
        {
            Card.CardType cardType = GenerateTypeOfTier(tier);
            Tile cardTile = GenerateValidTile(
                Services.CardConfig.MinSpawnDistFromMonsters,
                Services.CardConfig.MinSpawnDistFromItems, 
                Services.CardConfig.MinSpawnCol, 
                levelLength - 1);
            if (cardTile != null)
            {
                cardsOnBoard.Add(GenerateCard(cardType, cardTile));
            }
            else break;
        }
    }

    Card GenerateCard(Card.CardType cardType, Tile tile)
    {
        Card card = Services.CardConfig.CreateCardOfType(cardType);
        card.CreatePhysicalCard(tile);
        return card;
    }

    Card.CardType GenerateTypeOfTier(int tier)
    {
        List<Card.CardType> potentialTypes = new List<Card.CardType>();
        foreach(CardInfo cardInfo in Services.CardConfig.Cards)
        {
            if (cardInfo.Tier == tier) potentialTypes.Add(cardInfo.CardType);
        }
        int randomIndex = Random.Range(0, potentialTypes.Count);
        return potentialTypes[randomIndex];
    }
}
