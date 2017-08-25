using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Card Config")]
public class CardConfig : ScriptableObject
{
    [SerializeField]
    private CardInfo[] cards;
    public CardInfo[] Cards { get { return cards; } }

    [SerializeField]
    private CardInfo[] startingDeck;
    public CardInfo[] StartingDeck { get { return startingDeck; } }

    [SerializeField]
    private Vector3 handCardSpacing;
    public Vector3 HandCardSpacing { get { return handCardSpacing; } }

    [SerializeField]
    private Vector3 inPlaySpacing;
    public Vector3 InPlaySpacing { get { return inPlaySpacing; } }

    [SerializeField]
    private float onHoverScaleUp;
    public float OnHoverScaleUp { get { return onHoverScaleUp; } }

    [SerializeField]
    private Vector3 onHoverOffset;
    public Vector3 OnHoverOffset { get { return onHoverOffset; } }

    [SerializeField]
    private float cardPlayThresholdYPos;
    public float CardPlayThresholdYPos { get { return cardPlayThresholdYPos; } }

    [SerializeField]
    private Color playableColor;
    public Color PlayableColor { get { return playableColor; } }

    [SerializeField]
    private float drawAnimDur;
    public float DrawAnimDur { get { return drawAnimDur; } }

    [SerializeField]
    private float drawAnimTimeToMidpoint;
    public float DrawAnimTimeToMidpoint { get { return drawAnimTimeToMidpoint; } }

    [SerializeField]
    private float drawAnimScale;
    public float DrawAnimScale { get { return drawAnimScale; } }

    [SerializeField]
    private Vector3 drawAnimMidpointOffset;
    public Vector3 DrawAnimMidpointOffset { get { return drawAnimMidpointOffset; } }

    [SerializeField]
    private float playAnimDur;
    public float PlayAnimDur { get { return playAnimDur; } }

    [SerializeField]
    private float acquireAnimDur;
    public float AcquireAnimDur { get { return acquireAnimDur; } }

    [SerializeField]
    private int minSpawnDistFromMonsters;
    public int MinSpawnDistFromMonsters { get { return minSpawnDistFromMonsters; } }

    [SerializeField]
    private int minSpawnDistFromItems;
    public int MinSpawnDistFromItems { get { return minSpawnDistFromItems; } }

    [SerializeField]
    private int minSpawnCol;
    public int MinSpawnCol { get { return minSpawnCol; } }

    [SerializeField]
    private Vector3 cardOnBoardOffset;
    public Vector3 CardOnBoardOffset { get { return cardOnBoardOffset; } }

    [SerializeField]
    private Vector3 cardOnBoardHoverOffset;
    public Vector3 CardOnBoardHoverOffset { get { return cardOnBoardHoverOffset; } }


    public CardInfo GetCardOfType(Card.CardType cardType)
    {
        foreach(CardInfo cardInfo in cards)
        {
            if (cardInfo.CardType == cardType)
            {
                return cardInfo;
            }
        }
        Debug.Assert(false); // we should never be here if cards are properly configured
        return null;
    }

    public Card CreateCardOfType(Card.CardType type)
    {
        switch (type)
        {
            case Card.CardType.Step:
                return new Step();
            case Card.CardType.Goblin:
                return new GoblinCard();
            case Card.CardType.Punch:
                return new Punch();
            case Card.CardType.Tome:
                return new Tome();
            case Card.CardType.Bow:
                return new Bow();
            case Card.CardType.Run:
                return new Run();
            case Card.CardType.Sprint:
                return new Sprint();
            case Card.CardType.Crossbow:
                return new Crossbow();
            case Card.CardType.Zombie:
                return new ZombieCard();
            default:
                return null;
        }
    }
}
