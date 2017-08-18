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
    private float handCardSpacing;
    public float HandCardSpacing { get { return handCardSpacing; } }

    [SerializeField]
    private Vector3 handStartPos;
    public Vector3 HandStartPos { get { return handStartPos; } }

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


    public CardInfo GetCardOfType(Card.Type cardType)
    {
        foreach(CardInfo cardInfo in cards)
        {
            if (cardInfo.Type == cardType)
            {
                return cardInfo;
            }
        }
        Debug.Assert(false); // we should never be here if cards are properly configured
        return null;
    }

    public Card CreateCardOfType(Card.Type type)
    {
        switch (type)
        {
            case Card.Type.Step:
                return new Step();
            default:
                return null;
        }
    }
}
