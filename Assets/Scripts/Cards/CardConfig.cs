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
            default:
                return null;
        }
    }
}
