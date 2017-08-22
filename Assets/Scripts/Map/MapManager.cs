using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour {

    [SerializeField]
    private int levelLength;
    [SerializeField]
    private int levelHeight;
    public Tile[,] map { get; private set; }
    [SerializeField]
    private int maxTriesProcGen;
    public List<Card> cardsOnBoard;

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
                map[x, y] = new Tile(new Coord(x, y));
            }
        }
        FindAllNeighbors();
        GenerateCardsOnBoard(3, 1);
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
